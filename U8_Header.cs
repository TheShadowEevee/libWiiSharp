// Decompiled with JetBrains decompiler
// Type: libWiiSharp.U8_Header
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class U8_Header
  {
    private uint u8Magic = 1437218861;
    private uint offsetToRootNode = 32;
    private uint headerSize;
    private uint offsetToData;
    private byte[] padding = new byte[16];

    public uint U8Magic => this.u8Magic;

    public uint OffsetToRootNode => this.offsetToRootNode;

    public uint HeaderSize
    {
      get => this.headerSize;
      set => this.headerSize = value;
    }

    public uint OffsetToData
    {
      get => this.offsetToData;
      set => this.offsetToData = value;
    }

    public byte[] Padding => this.padding;

    public void Write(Stream writeStream)
    {
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.u8Magic)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.offsetToRootNode)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.headerSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.offsetToData)), 0, 4);
      writeStream.Write(this.padding, 0, 16);
    }
  }
}
