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
    private uint smplId = 1936552044;
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
    private List<WaveSmplLoop> smplLoops = new List<WaveSmplLoop>();

    public uint SmplSize => this.smplSize;

    public uint NumLoops => this.numLoops;

    public WaveSmplLoop[] Loops => this.smplLoops.ToArray();

    public void AddLoop(int loopStartSample, int loopEndSample)
    {
      this.RemoveAllLoops();
      ++this.numLoops;
      this.smplLoops.Add(new WaveSmplLoop()
      {
        LoopStart = (uint) loopStartSample,
        LoopEnd = (uint) loopEndSample
      });
    }

    public void RemoveAllLoops()
    {
      this.smplLoops.Clear();
      this.numLoops = 0U;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Shared.Swap(this.smplId));
      writer.Write(this.smplSize);
      writer.Write(this.manufacturer);
      writer.Write(this.product);
      writer.Write(this.samplePeriod);
      writer.Write(this.unityNote);
      writer.Write(this.pitchFraction);
      writer.Write(this.smpteFormat);
      writer.Write(this.smpteOffset);
      writer.Write(this.numLoops);
      writer.Write(this.samplerData);
      for (int index = 0; (long) index < (long) this.numLoops; ++index)
        this.smplLoops[index].Write(writer);
    }

    public void Read(BinaryReader reader)
    {
      this.smplSize = (int) Shared.Swap(reader.ReadUInt32()) == (int) this.smplId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
      this.manufacturer = reader.ReadUInt32();
      this.product = reader.ReadUInt32();
      this.samplePeriod = reader.ReadUInt32();
      this.unityNote = reader.ReadUInt32();
      this.pitchFraction = reader.ReadUInt32();
      this.smpteFormat = reader.ReadUInt32();
      this.smpteOffset = reader.ReadUInt32();
      this.numLoops = reader.ReadUInt32();
      this.samplerData = reader.ReadUInt32();
      for (int index = 0; (long) index < (long) this.numLoops; ++index)
      {
        WaveSmplLoop waveSmplLoop = new WaveSmplLoop();
        waveSmplLoop.Read(reader);
        this.smplLoops.Add(waveSmplLoop);
      }
    }
  }
}
