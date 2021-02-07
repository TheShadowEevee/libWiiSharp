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

namespace libWiiSharp
{
    public class TMD_Content
    {
        private uint contentId;
        private ushort index;
        private ushort type;
        private ulong size;
        private byte[] hash = new byte[20];

        public uint ContentID
        {
            get => contentId;
            set => contentId = value;
        }

        public ushort Index
        {
            get => index;
            set => index = value;
        }

        public ContentType Type
        {
            get => (ContentType)type;
            set => type = (ushort)value;
        }

        public ulong Size
        {
            get => size;
            set => size = value;
        }

        public byte[] Hash
        {
            get => hash;
            set => hash = value;
        }
    }
}
