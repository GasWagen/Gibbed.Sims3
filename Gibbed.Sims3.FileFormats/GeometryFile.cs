﻿using System;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Sims3.FileFormats
{
    public class GeometryFile : IFormat
    {
        public UInt32 Version;
        public KeyTable KeyTable;

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            ScenegraphFile scenegraph = new ScenegraphFile();
            scenegraph.Deserialize(input);

            if (scenegraph.ChunkKeys.Count != 1)
            {
                throw new InvalidOperationException();
            }

            Stream chunk = scenegraph.ChunkData[0];

            this.Version = chunk.ReadValueU32();

            if (this.Version > 5)
            {
                throw new InvalidOperationException();
            }

            if (this.Version >= 3)
            {
                UInt32 unk1 = chunk.ReadValueU32();
                UInt32 unk2 = chunk.ReadValueU32();

                if (unk1 == 0 || unk2 == 0)
                {
                    return;
                }

                this.KeyTable = new KeyTable();
                this.KeyTable.Deserialize(chunk);
            }
        }
    }
}
