// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_PaletteHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class TPL_PaletteHeader
  {
    private ushort numberOfItems;
    private byte unpacked;
    private byte pad;
    private uint paletteFormat = (uint) byte.MaxValue;
    private uint paletteDataOffset;

    public ushort NumberOfItems
    {
      get => this.numberOfItems;
      set => this.numberOfItems = value;
    }

    public byte Unpacked
    {
      get => this.unpacked;
      set => this.unpacked = value;
    }

    public byte Pad
    {
      get => this.pad;
      set => this.pad = value;
    }

    public uint PaletteFormat
    {
      get => this.paletteFormat;
      set => this.paletteFormat = value;
    }

    public uint PaletteDataOffset
    {
      get => this.paletteDataOffset;
      set => this.paletteDataOffset = value;
    }

    public void Write(Stream writeStream)
    {
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.numberOfItems)), 0, 2);
      writeStream.WriteByte(this.unpacked);
      writeStream.WriteByte(this.pad);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.paletteFormat)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.paletteDataOffset)), 0, 4);
    }
  }
}
