// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveFmtChunk
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  internal class WaveFmtChunk
  {
    private uint fmtId = 1718449184;
    private uint fmtSize = 16;
    private ushort audioFormat = 1;
    private ushort numChannels = 2;
    private uint sampleRate = 44100;
    private uint byteRate;
    private ushort blockAlign;
    private ushort bitsPerSample = 16;

    public uint FmtSize => this.fmtSize;

    public ushort NumChannels
    {
      get => this.numChannels;
      set => this.numChannels = value;
    }

    public uint SampleRate
    {
      get => this.sampleRate;
      set => this.sampleRate = value;
    }

    public ushort BitsPerSample
    {
      get => this.bitsPerSample;
      set => this.bitsPerSample = value;
    }

    public uint AudioFormat => (uint) this.audioFormat;

    public void Write(BinaryWriter writer)
    {
      this.byteRate = this.sampleRate * (uint) this.numChannels * (uint) this.bitsPerSample / 8U;
      this.blockAlign = (ushort) ((int) this.numChannels * (int) this.bitsPerSample / 8);
      writer.Write(Shared.Swap(this.fmtId));
      writer.Write(this.fmtSize);
      writer.Write(this.audioFormat);
      writer.Write(this.numChannels);
      writer.Write(this.sampleRate);
      writer.Write(this.byteRate);
      writer.Write(this.blockAlign);
      writer.Write(this.bitsPerSample);
    }

    public void Read(BinaryReader reader)
    {
      this.fmtSize = (int) Shared.Swap(reader.ReadUInt32()) == (int) this.fmtId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
      this.audioFormat = reader.ReadUInt16();
      this.numChannels = reader.ReadUInt16();
      this.sampleRate = reader.ReadUInt32();
      this.byteRate = reader.ReadUInt32();
      this.blockAlign = reader.ReadUInt16();
      this.bitsPerSample = reader.ReadUInt16();
    }
  }
}
