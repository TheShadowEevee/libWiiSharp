// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL_TextureHeader
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class TPL_TextureHeader
  {
    private ushort textureHeight;
    private ushort textureWidth;
    private uint textureFormat;
    private uint textureDataOffset;
    private uint wrapS;
    private uint wrapT;
    private uint minFilter = 1;
    private uint magFilter = 1;
    private uint lodBias;
    private byte edgeLod;
    private byte minLod;
    private byte maxLod;
    private byte unpacked;

    public ushort TextureHeight
    {
      get => this.textureHeight;
      set => this.textureHeight = value;
    }

    public ushort TextureWidth
    {
      get => this.textureWidth;
      set => this.textureWidth = value;
    }

    public uint TextureFormat
    {
      get => this.textureFormat;
      set => this.textureFormat = value;
    }

    public uint TextureDataOffset
    {
      get => this.textureDataOffset;
      set => this.textureDataOffset = value;
    }

    public uint WrapS
    {
      get => this.wrapS;
      set => this.wrapS = value;
    }

    public uint WrapT
    {
      get => this.wrapT;
      set => this.wrapT = value;
    }

    public uint MinFilter
    {
      get => this.minFilter;
      set => this.minFilter = value;
    }

    public uint MagFilter
    {
      get => this.magFilter;
      set => this.magFilter = value;
    }

    public uint LodBias
    {
      get => this.lodBias;
      set => this.lodBias = value;
    }

    public byte EdgeLod
    {
      get => this.edgeLod;
      set => this.edgeLod = value;
    }

    public byte MinLod
    {
      get => this.minLod;
      set => this.minLod = value;
    }

    public byte MaxLod
    {
      get => this.maxLod;
      set => this.maxLod = value;
    }

    public byte Unpacked
    {
      get => this.unpacked;
      set => this.unpacked = value;
    }

    public void Write(Stream writeStream)
    {
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.textureHeight)), 0, 2);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.textureWidth)), 0, 2);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.textureFormat)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.textureDataOffset)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.wrapS)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.wrapT)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.minFilter)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.magFilter)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.lodBias)), 0, 4);
      writeStream.WriteByte(this.edgeLod);
      writeStream.WriteByte(this.minLod);
      writeStream.WriteByte(this.maxLod);
      writeStream.WriteByte(this.unpacked);
    }
  }
}
