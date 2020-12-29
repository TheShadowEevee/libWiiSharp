// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WaveHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  internal class WaveHeader
  {
    private uint headerId = 1380533830;
    private uint fileSize = 12;
    private uint format = 1463899717;

    public uint FileSize
    {
      get => this.fileSize;
      set => this.fileSize = value;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Shared.Swap(this.headerId));
      writer.Write(this.fileSize);
      writer.Write(Shared.Swap(this.format));
    }

    public void Read(BinaryReader reader)
    {
      this.fileSize = (int) Shared.Swap(reader.ReadUInt32()) == (int) this.headerId ? reader.ReadUInt32() : throw new Exception("Not a valid RIFF Wave file!");
      if ((int) Shared.Swap(reader.ReadUInt32()) != (int) this.format)
        throw new Exception("Not a valid RIFF Wave file!");
    }
  }
}
