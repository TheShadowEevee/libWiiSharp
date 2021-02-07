// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    internal class WaveHeader
    {
        private readonly uint headerId = 1380533830;
        private uint fileSize = 12;
        private readonly uint format = 1463899717;

        public uint FileSize
        {
            get => fileSize;
            set => fileSize = value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shared.Swap(headerId));
            writer.Write(fileSize);
            writer.Write(Shared.Swap(format));
        }

        public void Read(BinaryReader reader)
        {
            fileSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)headerId ? reader.ReadUInt32() : throw new Exception("Not a valid RIFF Wave file!");
            if ((int)Shared.Swap(reader.ReadUInt32()) != (int)format)
            {
                throw new Exception("Not a valid RIFF Wave file!");
            }
        }
    }
}
