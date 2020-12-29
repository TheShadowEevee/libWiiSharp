// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveSmplLoop
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

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
      get => this.start;
      set => this.start = value;
    }

    public uint LoopEnd
    {
      get => this.end;
      set => this.end = value;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(this.cuePointId);
      writer.Write(this.type);
      writer.Write(this.start);
      writer.Write(this.end);
      writer.Write(this.fraction);
      writer.Write(this.playCount);
    }

    public void Read(BinaryReader reader)
    {
      this.cuePointId = reader.ReadUInt32();
      this.type = reader.ReadUInt32();
      this.start = reader.ReadUInt32();
      this.end = reader.ReadUInt32();
      this.fraction = reader.ReadUInt32();
      this.playCount = reader.ReadUInt32();
    }
  }
}
