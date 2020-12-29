// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_Header
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class TPL_Header
  {
    private uint tplMagic = 2142000;
    private uint numOfTextures;
    private uint headerSize = 12;

    public uint TplMagic => this.tplMagic;

    public uint NumOfTextures
    {
      get => this.numOfTextures;
      set => this.numOfTextures = value;
    }

    public uint HeaderSize => this.headerSize;

    public void Write(Stream writeStream)
    {
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.tplMagic)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.numOfTextures)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.headerSize)), 0, 4);
    }
  }
}
