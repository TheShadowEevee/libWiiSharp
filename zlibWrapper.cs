// Decompiled with JetBrains decompiler
// Type: libWiiSharp.zlibWrapper
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Runtime.InteropServices;

namespace libWiiSharp
{
  internal class zlibWrapper
  {
    [DllImport("zlib1.dll")]
    private static extern zlibWrapper.ZLibError compress2(
      byte[] dest,
      ref int destLength,
      byte[] source,
      int sourceLength,
      int level);

    public static byte[] Compress(byte[] inFile)
    {
      byte[] array = new byte[inFile.Length + 64];
      int destLength = -1;
      zlibWrapper.ZLibError zlibError = zlibWrapper.compress2(array, ref destLength, inFile, inFile.Length, 6);
      if (zlibError != zlibWrapper.ZLibError.Z_OK || destLength <= -1 || destLength >= inFile.Length)
        throw new Exception("An error occured while compressing! Code: " + zlibError.ToString());
      Array.Resize<byte>(ref array, destLength);
      return array;
    }

    public enum ZLibError
    {
      Z_VERSION_ERROR = -6, // 0xFFFFFFFA
      Z_BUF_ERROR = -5, // 0xFFFFFFFB
      Z_MEM_ERROR = -4, // 0xFFFFFFFC
      Z_DATA_ERROR = -3, // 0xFFFFFFFD
      Z_STREAM_ERROR = -2, // 0xFFFFFFFE
      Z_ERRNO = -1, // 0xFFFFFFFF
      Z_OK = 0,
      Z_STREAM_END = 1,
      Z_NEED_DICT = 2,
    }
  }
}
