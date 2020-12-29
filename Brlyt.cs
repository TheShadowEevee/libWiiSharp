// Decompiled with JetBrains decompiler
// Type: libWiiSharp.Brlyt
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace libWiiSharp
{
  public class Brlyt
  {
    public static string[] GetBrlytTpls(string pathToBrlyt) => Brlyt.getBrlytTpls(File.ReadAllBytes(pathToBrlyt));

    public static string[] GetBrlytTpls(byte[] brlytFile) => Brlyt.getBrlytTpls(brlytFile);

    public static string[] GetBrlytTpls(WAD wad, bool banner)
    {
      if (!wad.HasBanner)
        return new string[0];
      string str = nameof (banner);
      if (!banner)
        str = "icon";
      for (int index1 = 0; index1 < wad.BannerApp.Nodes.Count; ++index1)
      {
        if (wad.BannerApp.StringTable[index1].ToLower() == str + ".bin")
        {
          U8 u8 = U8.Load(wad.BannerApp.Data[index1]);
          string[] a = new string[0];
          for (int index2 = 0; index2 < u8.Nodes.Count; ++index2)
          {
            if (u8.StringTable[index2].ToLower() == str + ".brlyt")
              a = Shared.MergeStringArrays(a, Brlyt.getBrlytTpls(u8.Data[index2]));
          }
          return a;
        }
      }
      return new string[0];
    }

    private static string[] getBrlytTpls(byte[] brlytFile)
    {
      List<string> stringList = new List<string>();
      int numOfTpls = Brlyt.getNumOfTpls(brlytFile);
      int index1 = 48 + numOfTpls * 8;
      for (int index2 = 0; index2 < numOfTpls; ++index2)
      {
        string empty = string.Empty;
        while (brlytFile[index1] != (byte) 0)
          empty += Convert.ToChar(brlytFile[index1++]).ToString();
        stringList.Add(empty);
        ++index1;
      }
      for (int index2 = stringList.Count - 1; index2 >= 0; --index2)
      {
        if (!stringList[index2].ToLower().EndsWith(".tpl"))
          stringList.RemoveAt(index2);
      }
      return stringList.ToArray();
    }

    private static int getNumOfTpls(byte[] brlytFile) => (int) Shared.Swap(BitConverter.ToUInt16(brlytFile, 44));
  }
}
