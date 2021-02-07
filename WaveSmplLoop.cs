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

using System.IO;

namespace libWiiSharp
{
    internal class WaveSmplLoop
    {
        private uint cuePointId;
        private uint type;
        private uint start;
        private uint end;
        private uint fraction;
        private uint playCount;

        public uint LoopStart
        {
            get => start;
            set => start = value;
        }

        public uint LoopEnd
        {
            get => end;
            set => end = value;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(cuePointId);
            writer.Write(type);
            writer.Write(start);
            writer.Write(end);
            writer.Write(fraction);
            writer.Write(playCount);
        }

        public void Read(BinaryReader reader)
        {
            cuePointId = reader.ReadUInt32();
            type = reader.ReadUInt32();
            start = reader.ReadUInt32();
            end = reader.ReadUInt32();
            fraction = reader.ReadUInt32();
            playCount = reader.ReadUInt32();
        }
    }
}
