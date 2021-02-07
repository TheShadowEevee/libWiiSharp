// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_TextureEntry
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class TPL_TextureEntry
    {
        private uint textureHeaderOffset;
        private uint paletteHeaderOffset;

        public uint TextureHeaderOffset
        {
            get => textureHeaderOffset;
            set => textureHeaderOffset = value;
        }

        public uint PaletteHeaderOffset
        {
            get => paletteHeaderOffset;
            set => paletteHeaderOffset = value;
        }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(textureHeaderOffset)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(paletteHeaderOffset)), 0, 4);
        }
    }
}
