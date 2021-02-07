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
    internal class WaveFmtChunk
    {
        private readonly uint fmtId = 1718449184;
        private uint fmtSize = 16;
        private ushort audioFormat = 1;
        private ushort numChannels = 2;
        private uint sampleRate = 44100;
        private uint byteRate;
        private ushort blockAlign;
        private ushort bitsPerSample = 16;

        public uint FmtSize => fmtSize;

        public ushort NumChannels
        {
            get => numChannels;
            set => numChannels = value;
        }

        public uint SampleRate
        {
            get => sampleRate;
            set => sampleRate = value;
        }

        public ushort BitsPerSample
        {
            get => bitsPerSample;
            set => bitsPerSample = value;
        }

        public uint AudioFormat => audioFormat;

        public void Write(BinaryWriter writer)
        {
            byteRate = sampleRate * numChannels * bitsPerSample / 8U;
            blockAlign = (ushort)(numChannels * bitsPerSample / 8);
            writer.Write(Shared.Swap(fmtId));
            writer.Write(fmtSize);
            writer.Write(audioFormat);
            writer.Write(numChannels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
        }

        public void Read(BinaryReader reader)
        {
            fmtSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)fmtId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
            audioFormat = reader.ReadUInt16();
            numChannels = reader.ReadUInt16();
            sampleRate = reader.ReadUInt32();
            byteRate = reader.ReadUInt32();
            blockAlign = reader.ReadUInt16();
            bitsPerSample = reader.ReadUInt16();
        }
    }
}
