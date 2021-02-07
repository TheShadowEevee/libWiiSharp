// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_PaletteHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class TPL_PaletteHeader
    {
        private ushort numberOfItems;
        private byte unpacked;
        private byte pad;
        private uint paletteFormat = byte.MaxValue;
        private uint paletteDataOffset;

        public ushort NumberOfItems
        {
            get => numberOfItems;
            set => numberOfItems = value;
        }

        public byte Unpacked
        {
            get => unpacked;
            set => unpacked = value;
        }

        public byte Pad
        {
            get => pad;
            set => pad = value;
        }

        public uint PaletteFormat
        {
            get => paletteFormat;
            set => paletteFormat = value;
        }

        public uint PaletteDataOffset
        {
            get => paletteDataOffset;
            set => paletteDataOffset = value;
        }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(numberOfItems)), 0, 2);
            writeStream.WriteByte(unpacked);
            writeStream.WriteByte(pad);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(paletteFormat)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(paletteDataOffset)), 0, 4);
        }
    }
}
