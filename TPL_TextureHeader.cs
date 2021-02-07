// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_TextureHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class TPL_TextureHeader
    {
        private ushort textureHeight;
        private ushort textureWidth;
        private uint textureFormat;
        private uint textureDataOffset;
        private uint wrapS;
        private uint wrapT;
        private uint minFilter = 1;
        private uint magFilter = 1;
        private uint lodBias;
        private byte edgeLod;
        private byte minLod;
        private byte maxLod;
        private byte unpacked;

        public ushort TextureHeight
        {
            get => textureHeight;
            set => textureHeight = value;
        }

        public ushort TextureWidth
        {
            get => textureWidth;
            set => textureWidth = value;
        }

        public uint TextureFormat
        {
            get => textureFormat;
            set => textureFormat = value;
        }

        public uint TextureDataOffset
        {
            get => textureDataOffset;
            set => textureDataOffset = value;
        }

        public uint WrapS
        {
            get => wrapS;
            set => wrapS = value;
        }

        public uint WrapT
        {
            get => wrapT;
            set => wrapT = value;
        }

        public uint MinFilter
        {
            get => minFilter;
            set => minFilter = value;
        }

        public uint MagFilter
        {
            get => magFilter;
            set => magFilter = value;
        }

        public uint LodBias
        {
            get => lodBias;
            set => lodBias = value;
        }

        public byte EdgeLod
        {
            get => edgeLod;
            set => edgeLod = value;
        }

        public byte MinLod
        {
            get => minLod;
            set => minLod = value;
        }

        public byte MaxLod
        {
            get => maxLod;
            set => maxLod = value;
        }

        public byte Unpacked
        {
            get => unpacked;
            set => unpacked = value;
        }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(textureHeight)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(textureWidth)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(textureFormat)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(textureDataOffset)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(wrapS)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(wrapT)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(minFilter)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(magFilter)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(lodBias)), 0, 4);
            writeStream.WriteByte(edgeLod);
            writeStream.WriteByte(minLod);
            writeStream.WriteByte(maxLod);
            writeStream.WriteByte(unpacked);
        }
    }
}
