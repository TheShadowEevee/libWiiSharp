// Decompiled with JetBrains decompiler
// Type: libWiiSharp.ContentIndices
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;

namespace libWiiSharp
{
  public struct ContentIndices : IComparable
  {
    private int index;
    private int contentIndex;

    public int Index => this.index;

    public int ContentIndex => this.contentIndex;

    public ContentIndices(int index, int contentIndex)
    {
      this.index = index;
      this.contentIndex = contentIndex;
    }

    public int CompareTo(object obj)
    {
      if (obj is ContentIndices contentIndices)
        return this.contentIndex.CompareTo(contentIndices.contentIndex);
      throw new ArgumentException();
    }
  }
}
