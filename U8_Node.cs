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
    public class U8_Node
    {
        private ushort type;
        private ushort offsetToName;
        private uint offsetToData;
        private uint sizeOfData;

        public U8_NodeType Type
        {
            get => (U8_NodeType)type;
            set => type = (ushort)value;
        }

        public ushort OffsetToName
        {
            get => offsetToName;
            set => offsetToName = value;
        }

        public uint OffsetToData
        {
            get => offsetToData;
            set => offsetToData = value;
        }

        public uint SizeOfData
        {
            get => sizeOfData;
            set => sizeOfData = value;
        }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(type)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToName)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToData)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(sizeOfData)), 0, 4);
        }
    }
}
