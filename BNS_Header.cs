/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 Github Contributors
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
    internal class BNS_Header
    {
        private byte[] magic = new byte[4]
        {
            (byte) 66,
            (byte) 78,
            (byte) 83,
            (byte) 32
        };
        private uint flags = 4278124800;
        private uint fileSize = 315584;
        private ushort size = 32;
        private ushort chunkCount = 2;
        private uint infoOffset = 32;
        private uint infoLength = 160;
        private uint dataOffset = 192;
        private uint dataLength = 315392;

        public uint DataOffset
        {
            get => this.dataOffset;
            set => this.dataOffset = value;
        }

        public uint InfoLength
        {
            get => this.infoLength;
            set => this.infoLength = value;
        }

        public ushort Size
        {
            get => this.size;
            set => this.size = value;
        }

        public uint DataLength
        {
            get => this.dataLength;
            set => this.dataLength = value;
        }

        public uint FileSize
        {
            get => this.fileSize;
            set => this.fileSize = value;
        }

        public void Write(Stream outStream)
        {
          outStream.Write(this.magic, 0, this.magic.Length);
          byte[] bytes1 = BitConverter.GetBytes(this.flags);
          Array.Reverse((Array) bytes1);
          outStream.Write(bytes1, 0, bytes1.Length);
          byte[] bytes2 = BitConverter.GetBytes(this.fileSize);
          Array.Reverse((Array) bytes2);
          outStream.Write(bytes2, 0, bytes2.Length);
          byte[] bytes3 = BitConverter.GetBytes(this.size);
          Array.Reverse((Array) bytes3);
          outStream.Write(bytes3, 0, bytes3.Length);
          byte[] bytes4 = BitConverter.GetBytes(this.chunkCount);
          Array.Reverse((Array) bytes4);
          outStream.Write(bytes4, 0, bytes4.Length);
          byte[] bytes5 = BitConverter.GetBytes(this.infoOffset);
          Array.Reverse((Array) bytes5);
          outStream.Write(bytes5, 0, bytes5.Length);
          byte[] bytes6 = BitConverter.GetBytes(this.infoLength);
          Array.Reverse((Array) bytes6);
          outStream.Write(bytes6, 0, bytes6.Length);
          byte[] bytes7 = BitConverter.GetBytes(this.dataOffset);
          Array.Reverse((Array) bytes7);
          outStream.Write(bytes7, 0, bytes7.Length);
          byte[] bytes8 = BitConverter.GetBytes(this.dataLength);
          Array.Reverse((Array) bytes8);
          outStream.Write(bytes8, 0, bytes8.Length);
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            if (!Shared.CompareByteArrays(this.magic, binaryReader.ReadBytes(4)))
            {
                binaryReader.BaseStream.Seek(28L, SeekOrigin.Current);
                if (!Shared.CompareByteArrays(this.magic, binaryReader.ReadBytes(4)))
                    throw new Exception("This is not a valid BNS audio file!");
            }
        this.flags = Shared.Swap(binaryReader.ReadUInt32());
        this.fileSize = Shared.Swap(binaryReader.ReadUInt32());
        this.size = Shared.Swap(binaryReader.ReadUInt16());
        this.chunkCount = Shared.Swap(binaryReader.ReadUInt16());
        this.infoOffset = Shared.Swap(binaryReader.ReadUInt32());
        this.infoLength = Shared.Swap(binaryReader.ReadUInt32());
        this.dataOffset = Shared.Swap(binaryReader.ReadUInt32());
        this.dataLength = Shared.Swap(binaryReader.ReadUInt32());
        }
    }
}
