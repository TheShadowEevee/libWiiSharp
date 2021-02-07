// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_Header
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class TPL_Header
    {
        private readonly uint tplMagic = 2142000;
        private uint numOfTextures;
        private readonly uint headerSize = 12;

        public uint TplMagic => tplMagic;

        public uint NumOfTextures
        {
            get => numOfTextures;
            set => numOfTextures = value;
        }

        public uint HeaderSize => headerSize;

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tplMagic)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(numOfTextures)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
        }
    }
}
