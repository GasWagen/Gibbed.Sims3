﻿using System;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Sims3.FileFormats.Effects
{
    public class Sound : IEffectFormat
    {
        public short MinimumVersion { get { return 1; } }
        public short MaximumVersion { get { return 1; } }

        public void Serialize(Stream output, short version)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, short version)
        {
            input.ReadValueU32(false);
            input.ReadValueU64(false);
            input.ReadValueU32(false);
            input.ReadValueU32(false);
            input.ReadValueU32(false);
        }
    }
}
