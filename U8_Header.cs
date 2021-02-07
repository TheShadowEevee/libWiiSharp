// Decompiled with JetBrains decompiler
// Type: libWiiSharp.U8_Header
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class U8_Header
    {
        private readonly uint u8Magic = 1437218861;
        private readonly uint offsetToRootNode = 32;
        private uint headerSize;
        private uint offsetToData;
        private readonly byte[] padding = new byte[16];

        public uint U8Magic => u8Magic;

        public uint OffsetToRootNode => offsetToRootNode;

        public uint HeaderSize
        {
            get => headerSize;
            set => headerSize = value;
        }

        public uint OffsetToData
        {
            get => offsetToData;
            set => offsetToData = value;
        }

        public byte[] Padding => padding;

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(u8Magic)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToRootNode)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToData)), 0, 4);
            writeStream.Write(padding, 0, 16);
        }
    }
}
