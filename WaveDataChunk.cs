// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveDataChunk
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  internal class WaveDataChunk
  {
    private uint dataId = 1684108385;
    private uint dataSize = 8;
    private byte[] data;

    public uint DataSize => this.dataSize;

    public byte[] Data
    {
      get => this.data;
      set
      {
        this.data = value;
        this.dataSize = (uint) this.data.Length;
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Shared.Swap(this.dataId));
      writer.Write(this.dataSize);
      writer.Write(this.data, 0, this.data.Length);
    }

    public void Read(BinaryReader reader)
    {
      this.dataSize = (int) Shared.Swap(reader.ReadUInt32()) == (int) this.dataId ? reader.ReadUInt32() : throw new Exception("Wrong chunk ID!");
      this.data = reader.ReadBytes((int) this.dataSize);
    }
  }
}
