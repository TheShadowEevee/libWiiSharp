// Decompiled with JetBrains decompiler
// Type: libWiiSharp.BNS_Data
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  internal class BNS_Data
  {
    private byte[] magic = new byte[4]
    {
      (byte) 68,
      (byte) 65,
      (byte) 84,
      (byte) 65
    };
    private uint size = 315392;
    private byte[] data;

    public uint Size
    {
      get => this.size;
      set => this.size = value;
    }

    public byte[] Data
    {
      get => this.data;
      set => this.data = value;
    }

    public void Write(Stream outStream)
    {
      byte[] bytes = BitConverter.GetBytes(Shared.Swap(this.size));
      outStream.Write(this.magic, 0, this.magic.Length);
      outStream.Write(bytes, 0, bytes.Length);
      outStream.Write(this.data, 0, this.data.Length);
    }

    public void Read(Stream input)
    {
      BinaryReader binaryReader = new BinaryReader(input);
      this.size = Shared.CompareByteArrays(this.magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
      this.data = binaryReader.ReadBytes((int) this.size - 8);
    }
  }
}
