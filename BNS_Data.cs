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
    internal class BNS_Data
    {
        private byte[] magic = new byte[4]
        {
            (byte) 68,
            (byte) 65,
            (byte) 84,
            (byte) 65
        };
        private uint size = 315392;
        private byte[] data;

        public uint Size
        {
            get => this.size;
            set => this.size = value;
        }

        public byte[] Data
        {
            get => this.data;
            set => this.data = value;
        }

        public void Write(Stream outStream)
        {
            byte[] bytes = BitConverter.GetBytes(Shared.Swap(this.size));
            outStream.Write(this.magic, 0, this.magic.Length);
            outStream.Write(bytes, 0, bytes.Length);
            outStream.Write(this.data, 0, this.data.Length);
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            this.size = Shared.CompareByteArrays(this.magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
            this.data = binaryReader.ReadBytes((int) this.size - 8);
        }
    }
}
