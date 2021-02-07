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
