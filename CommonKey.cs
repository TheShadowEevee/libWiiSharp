// Decompiled with JetBrains decompiler
// Type: libWiiSharp.CommonKey
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

namespace libWiiSharp
{
  public class CommonKey
  {
    private static string standardKey = "ebe42a225e8593e448d9c5457381aaf7";
    private static string koreanKey = "63b82bb4f4614e2e13f2fefbba4c9b7e";

    public static byte[] GetStandardKey() => Shared.HexStringToByteArray(CommonKey.standardKey);

    public static byte[] GetKoreanKey() => Shared.HexStringToByteArray(CommonKey.koreanKey);
  }
}
