/* This file is part of libWiiSharp
*Copyright(C) 2009 Leathl
* Copyright(C) 2020 - 2021 Github Contributors
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
    public class TPL_Header
    {
        private readonly uint tplMagic = 2142000;
        private uint numOfTextures;
        private readonly uint headerSize = 12;

        public uint TplMagic => tplMagic;

        public uint NumOfTextures
        {
            get => numOfTextures;
            set => numOfTextures = value;
        }

        public uint HeaderSize => headerSize;

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tplMagic)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(numOfTextures)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
        }
    }
}
