// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveFmtChunk
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    internal class WaveFmtChunk
    {
        private readonly uint fmtId = 1718449184;
        private uint fmtSize = 16;
        private ushort audioFormat = 1;
        private ushort numChannels = 2;
        private uint sampleRate = 44100;
        private uint byteRate;
        private ushort blockAlign;
        private ushort bitsPerSample = 16;

        public uint FmtSize => fmtSize;

        public ushort NumChannels
        {
            get => numChannels;
            set => numChannels = value;
        }

        public uint SampleRate
        {
            get => sampleRate;
            set => sampleRate = value;
        }

        public ushort BitsPerSample
        {
            get => bitsPerSample;
            set => bitsPerSample = value;
        }

        public uint AudioFormat => audioFormat;

        public void Write(BinaryWriter writer)
        {
            byteRate = sampleRate * numChannels * bitsPerSample / 8U;
            blockAlign = (ushort)(numChannels * bitsPerSample / 8);
            writer.Write(Shared.Swap(fmtId));
            writer.Write(fmtSize);
            writer.Write(audioFormat);
            writer.Write(numChannels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
        }

        public void Read(BinaryReader reader)
        {
            fmtSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)fmtId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
            audioFormat = reader.ReadUInt16();
            numChannels = reader.ReadUInt16();
            sampleRate = reader.ReadUInt32();
            byteRate = reader.ReadUInt32();
            blockAlign = reader.ReadUInt16();
            bitsPerSample = reader.ReadUInt16();
        }
    }
}
