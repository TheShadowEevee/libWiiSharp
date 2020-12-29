// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WAD_Header
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class WAD_Header
  {
    private uint headerSize = 32;
    private uint wadType = 1232273408;
    private uint certSize = 2560;
    private uint reserved;
    private uint tikSize = 676;
    private uint tmdSize;
    private uint contentSize;
    private uint footerSize;

    public uint HeaderSize => this.headerSize;

    public uint WadType
    {
      get => this.wadType;
      set => this.wadType = value;
    }

    public uint CertSize => this.certSize;

    public uint Reserved => this.reserved;

    public uint TicketSize => this.tikSize;

    public uint TmdSize
    {
      get => this.tmdSize;
      set => this.tmdSize = value;
    }

    public uint ContentSize
    {
      get => this.contentSize;
      set => this.contentSize = value;
    }

    public uint FooterSize
    {
      get => this.footerSize;
      set => this.footerSize = value;
    }

    public void Write(Stream writeStream)
    {
      writeStream.Seek(0L, SeekOrigin.Begin);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.headerSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.wadType)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.certSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.reserved)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.tikSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.tmdSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.contentSize)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.footerSize)), 0, 4);
    }
  }
}
