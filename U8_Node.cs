// Decompiled with JetBrains decompiler
// Type: libWiiSharp.U8_Node
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class U8_Node
  {
    private ushort type;
    private ushort offsetToName;
    private uint offsetToData;
    private uint sizeOfData;

    public U8_NodeType Type
    {
      get => (U8_NodeType) this.type;
      set => this.type = (ushort) value;
    }

    public ushort OffsetToName
    {
      get => this.offsetToName;
      set => this.offsetToName = value;
    }

    public uint OffsetToData
    {
      get => this.offsetToData;
      set => this.offsetToData = value;
    }

    public uint SizeOfData
    {
      get => this.sizeOfData;
      set => this.sizeOfData = value;
    }

    public void Write(Stream writeStream)
    {
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.type)), 0, 2);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.offsetToName)), 0, 2);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.offsetToData)), 0, 4);
      writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.sizeOfData)), 0, 4);
    }
  }
}
