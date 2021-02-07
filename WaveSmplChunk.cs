// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveSmplChunk
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

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
