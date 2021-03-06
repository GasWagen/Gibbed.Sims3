﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Gibbed.RefPack;
using Gibbed.Sims3.FileFormats;
using Gibbed.Sims3.ResourceLookup;

namespace Gibbed.Sims3.PackageViewer
{
	public partial class SaveAllProgress : Form
	{
		public SaveAllProgress()
		{
			InitializeComponent();
		}

		delegate void SetStatusDelegate(string status, int percent);
		private void SetStatus(string status, int percent)
		{
			if (this.progressBar.InvokeRequired || this.statusLabel.InvokeRequired)
			{
				SetStatusDelegate callback = new SetStatusDelegate(SetStatus);
				this.Invoke(callback, new object[] { status, percent });
				return;
			}

			this.statusLabel.Text = status;
			this.progressBar.Value = percent;
		}

		delegate void SaveDoneDelegate();
		private void SaveDone()
		{
			if (this.InvokeRequired)
			{
				SaveDoneDelegate callback = new SaveDoneDelegate(SaveDone);
				this.Invoke(callback);
				return;
			}

			this.Close();
		}

		public void SaveAll(object oinfo)
		{
			SaveAllInformation info = (SaveAllInformation)oinfo;

			XmlTextWriter writer = new XmlTextWriter(Path.Combine(info.BasePath, "files.xml"), Encoding.UTF8);
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument();
			writer.WriteStartElement("files");

			for (int i = 0; i < info.Files.Length; i++)
			{
                DatabasePackedFile.Entry index = info.Files[i];

				string fileName = null;
				string groupName = null;

                if (Lookup.Files.ContainsKey(index.Key.InstanceId))
				{
                    fileName = Lookup.Files[index.Key.InstanceId];
                    
                    char[] invalids = Path.GetInvalidFileNameChars();
                    for (int j = 0; j < invalids.Length; j++)
                    {
                        fileName = fileName.Replace(invalids[j].ToString(), "");
                    }
				}
				else
				{
                    fileName = "#" + index.Key.InstanceId.ToString("X16");
				}

                if (Lookup.Groups.ContainsKey(index.Key.GroupId))
				{
                    groupName = Lookup.Groups[index.Key.GroupId];
				}
				else
				{
                    groupName = "#" + index.Key.GroupId.ToString("X8");
				}

                string fragmentPath;

                if (Lookup.Types.ContainsKey(index.Key.TypeId) == true)
                {
                    TypeLookup type = Lookup.Types[index.Key.TypeId];
                    fragmentPath = Path.Combine(type.Category, type.Directory);
                    fragmentPath = Path.Combine(fragmentPath, groupName);
                    fileName += "." + Lookup.Types[index.Key.TypeId].Extension;
                }
                else
                {
                    fragmentPath = Path.Combine("unknown", "#" + index.Key.TypeId.ToString("X8"));
                    fragmentPath = Path.Combine(fragmentPath, groupName);
                }

				Directory.CreateDirectory(Path.Combine(info.BasePath, fragmentPath));

				string path = Path.Combine(fragmentPath, fileName);

				this.SetStatus(path, i);

				writer.WriteStartElement("file");
                writer.WriteAttributeString("groupid", "0x" + index.Key.GroupId.ToString("X8"));
                writer.WriteAttributeString("instanceid", "0x" + index.Key.InstanceId.ToString("X16"));
                writer.WriteAttributeString("typeid", "0x" + index.Key.TypeId.ToString("X8"));
				writer.WriteValue(path);
				writer.WriteEndElement();

				path = Path.Combine(info.BasePath, path);

				if (index.Compressed)
				{
					info.Archive.Seek(index.Offset, SeekOrigin.Begin);
					byte[] d = info.Archive.RefPackDecompress();
					FileStream output = new FileStream(path, FileMode.Create);
					output.Write(d, 0, d.Length);
					output.Close();
				}
				else
				{
					info.Archive.Seek(index.Offset, SeekOrigin.Begin);
					byte[] d = new byte[index.DecompressedSize];
					info.Archive.Read(d, 0, d.Length);
					FileStream output = new FileStream(path, FileMode.Create);
					output.Write(d, 0, d.Length);
					output.Close();
				}
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			writer.Close();
			this.SaveDone();
		}

		private struct SaveAllInformation
		{
			public string BasePath;
			public Stream Archive;
            public DatabasePackedFile.Entry[] Files;
		}

		private Thread SaveThread;
        public void ShowSaveProgress(IWin32Window owner, Stream archive, DatabasePackedFile.Entry[] files, string basePath)
		{
			SaveAllInformation info;
			info.BasePath = basePath;
			info.Archive = archive;
			info.Files = files;

			this.progressBar.Value = 0;
			this.progressBar.Maximum = files.Length;

			this.SaveThread = new Thread(new ParameterizedThreadStart(SaveAll));
			this.SaveThread.Start(info);
			this.ShowDialog(owner);
		}

		private void OnCancel(object sender, EventArgs e)
		{
			if (this.SaveThread != null)
			{
				this.SaveThread.Abort();
			}

			this.Close();
		}
	}
}
