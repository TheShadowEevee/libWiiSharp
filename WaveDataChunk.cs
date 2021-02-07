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
    internal class WaveDataChunk
    {
        private readonly uint dataId = 1684108385;
        private uint dataSize = 8;
        private byte[] data;

        public uint DataSize => dataSize;

        public byte[] Data
        {
            get => data;
            set
            {
                data = value;
                dataSize = (uint)data.Length;
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shared.Swap(dataId));
            writer.Write(dataSize);
            writer.Write(data, 0, data.Length);
        }

        public void Read(BinaryReader reader)
        {
            dataSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)dataId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
            data = reader.ReadBytes((int)dataSize);
        }
    }
}
