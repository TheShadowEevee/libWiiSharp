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
    internal class BNS_Data
    {
        private readonly byte[] magic = new byte[4]
        {
             68,
             65,
             84,
             65
        };
        private uint size = 315392;
        private byte[] data;

        public uint Size
        {
            get => size;
            set => size = value;
        }

        public byte[] Data
        {
            get => data;
            set => data = value;
        }

        public void Write(Stream outStream)
        {
            byte[] bytes = BitConverter.GetBytes(Shared.Swap(size));
            outStream.Write(magic, 0, magic.Length);
            outStream.Write(bytes, 0, bytes.Length);
            outStream.Write(data, 0, data.Length);
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            size = Shared.CompareByteArrays(magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
            data = binaryReader.ReadBytes((int)size - 8);
        }
    }
}
