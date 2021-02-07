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
    internal class WaveHeader
    {
        private readonly uint headerId = 1380533830;
        private uint fileSize = 12;
        private readonly uint format = 1463899717;

        public uint FileSize
        {
            get => fileSize;
            set => fileSize = value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shared.Swap(headerId));
            writer.Write(fileSize);
            writer.Write(Shared.Swap(format));
        }

        public void Read(BinaryReader reader)
        {
            fileSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)headerId ? reader.ReadUInt32() : throw new Exception("Not a valid RIFF Wave file!");
            if ((int)Shared.Swap(reader.ReadUInt32()) != (int)format)
            {
                throw new Exception("Not a valid RIFF Wave file!");
            }
        }
    }
}
