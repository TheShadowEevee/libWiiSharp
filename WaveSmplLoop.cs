// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveSmplLoop
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System.IO;

namespace libWiiSharp
{
    internal class WaveSmplLoop
    {
        private uint cuePointId;
        private uint type;
        private uint start;
        private uint end;
        private uint fraction;
        private uint playCount;

        public uint LoopStart
        {
            get => start;
            set => start = value;
        }

        public uint LoopEnd
        {
            get => end;
            set => end = value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(cuePointId);
            writer.Write(type);
            writer.Write(start);
            writer.Write(end);
            writer.Write(fraction);
            writer.Write(playCount);
        }

        public void Read(BinaryReader reader)
        {
            cuePointId = reader.ReadUInt32();
            type = reader.ReadUInt32();
            start = reader.ReadUInt32();
            end = reader.ReadUInt32();
            fraction = reader.ReadUInt32();
            playCount = reader.ReadUInt32();
        }
    }
}
