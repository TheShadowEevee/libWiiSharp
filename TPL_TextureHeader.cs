/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2021 Github Contributors
 * 
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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
