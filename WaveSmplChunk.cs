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
using System.Collections.Generic;
using System.IO;

namespace libWiiSharp
{
    internal class WaveSmplChunk
    {
        private readonly uint smplId = 1936552044;
        private uint smplSize = 36;
        private uint manufacturer;
        private uint product;
        private uint samplePeriod;
        private uint unityNote = 60;
        private uint pitchFraction;
        private uint smpteFormat;
        private uint smpteOffset;
        private uint numLoops;
        private uint samplerData;
        private readonly List<WaveSmplLoop> smplLoops = new List<WaveSmplLoop>();

        public uint SmplSize => smplSize;

        public uint NumLoops => numLoops;

        public WaveSmplLoop[] Loops => smplLoops.ToArray();

        public void AddLoop(int loopStartSample, int loopEndSample)
        {
            RemoveAllLoops();
            ++numLoops;
            smplLoops.Add(new WaveSmplLoop()
            {
                LoopStart = (uint)loopStartSample,
                LoopEnd = (uint)loopEndSample
            });
        }

        public void RemoveAllLoops()
        {
            smplLoops.Clear();
            numLoops = 0U;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shared.Swap(smplId));
            writer.Write(smplSize);
            writer.Write(manufacturer);
            writer.Write(product);
            writer.Write(samplePeriod);
            writer.Write(unityNote);
            writer.Write(pitchFraction);
            writer.Write(smpteFormat);
            writer.Write(smpteOffset);
            writer.Write(numLoops);
            writer.Write(samplerData);
            for (int index = 0; index < numLoops; ++index)
            {
                smplLoops[index].Write(writer);
            }
        }

        public void Read(BinaryReader reader)
        {
            smplSize = (int)Shared.Swap(reader.ReadUInt32()) == (int)smplId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
            manufacturer = reader.ReadUInt32();
            product = reader.ReadUInt32();
            samplePeriod = reader.ReadUInt32();
            unityNote = reader.ReadUInt32();
            pitchFraction = reader.ReadUInt32();
            smpteFormat = reader.ReadUInt32();
            smpteOffset = reader.ReadUInt32();
            numLoops = reader.ReadUInt32();
            samplerData = reader.ReadUInt32();
            for (int index = 0; index < numLoops; ++index)
            {
                WaveSmplLoop waveSmplLoop = new WaveSmplLoop();
                waveSmplLoop.Read(reader);
                smplLoops.Add(waveSmplLoop);
            }
        }
    }
}
