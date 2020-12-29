// Decompiled with JetBrains decompiler
// Type: libWiiSharp.Headers
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
  public class Headers
  {
    private static uint imd5Magic = 1229800501;
    private static uint imetMagic = 1229800788;

    public static Headers.HeaderType DetectHeader(string pathToFile) => Headers.DetectHeader(File.ReadAllBytes(pathToFile));

    public static Headers.HeaderType DetectHeader(byte[] file)
    {
      if (file.Length > 68 && (int) Shared.Swap(BitConverter.ToUInt32(file, 64)) == (int) Headers.imetMagic)
        return Headers.HeaderType.ShortIMET;
      if (file.Length > 132 && (int) Shared.Swap(BitConverter.ToUInt32(file, 128)) == (int) Headers.imetMagic)
        return Headers.HeaderType.IMET;
      return file.Length > 4 && (int) Shared.Swap(BitConverter.ToUInt32(file, 0)) == (int) Headers.imd5Magic ? Headers.HeaderType.IMD5 : Headers.HeaderType.None;
    }

    public static Headers.HeaderType DetectHeader(Stream file)
    {
      byte[] buffer = new byte[4];
      if (file.Length > 68L)
      {
        file.Seek(64L, SeekOrigin.Begin);
        file.Read(buffer, 0, buffer.Length);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int) Headers.imetMagic)
          return Headers.HeaderType.ShortIMET;
      }
      if (file.Length > 132L)
      {
        file.Seek(128L, SeekOrigin.Begin);
        file.Read(buffer, 0, buffer.Length);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int) Headers.imetMagic)
          return Headers.HeaderType.IMET;
      }
      if (file.Length > 4L)
      {
        file.Seek(0L, SeekOrigin.Begin);
        file.Read(buffer, 0, buffer.Length);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int) Headers.imd5Magic)
          return Headers.HeaderType.IMD5;
      }
      return Headers.HeaderType.None;
    }

    public enum HeaderType
    {
      None = 0,
      IMD5 = 32, // 0x00000020
      ShortIMET = 1536, // 0x00000600
      IMET = 1600, // 0x00000640
    }

    public class IMET
    {
      private bool hashesMatch = true;
      private bool isShortImet;
      private byte[] additionalPadding = new byte[64];
      private byte[] padding = new byte[64];
      private uint imetMagic = 1229800788;
      private uint sizeOfHeader = 1536;
      private uint unknown = 3;
      private uint iconSize;
      private uint bannerSize;
      private uint soundSize;
      private uint flags;
      private byte[] japaneseTitle = new byte[84];
      private byte[] englishTitle = new byte[84];
      private byte[] germanTitle = new byte[84];
      private byte[] frenchTitle = new byte[84];
      private byte[] spanishTitle = new byte[84];
      private byte[] italianTitle = new byte[84];
      private byte[] dutchTitle = new byte[84];
      private byte[] unknownTitle1 = new byte[84];
      private byte[] unknownTitle2 = new byte[84];
      private byte[] koreanTitle = new byte[84];
      private byte[] padding2 = new byte[588];
      private byte[] hash = new byte[16];

      public bool IsShortIMET
      {
        get => this.isShortImet;
        set => this.isShortImet = value;
      }

      public uint IconSize
      {
        get => this.iconSize;
        set => this.iconSize = value;
      }

      public uint BannerSize
      {
        get => this.bannerSize;
        set => this.bannerSize = value;
      }

      public uint SoundSize
      {
        get => this.soundSize;
        set => this.soundSize = value;
      }

      public string JapaneseTitle
      {
        get => this.returnTitleAsString(this.japaneseTitle);
        set => this.setTitleFromString(value, 0);
      }

      public string EnglishTitle
      {
        get => this.returnTitleAsString(this.englishTitle);
        set => this.setTitleFromString(value, 1);
      }

      public string GermanTitle
      {
        get => this.returnTitleAsString(this.germanTitle);
        set => this.setTitleFromString(value, 2);
      }

      public string FrenchTitle
      {
        get => this.returnTitleAsString(this.frenchTitle);
        set => this.setTitleFromString(value, 3);
      }

      public string SpanishTitle
      {
        get => this.returnTitleAsString(this.spanishTitle);
        set => this.setTitleFromString(value, 4);
      }

      public string ItalianTitle
      {
        get => this.returnTitleAsString(this.italianTitle);
        set => this.setTitleFromString(value, 5);
      }

      public string DutchTitle
      {
        get => this.returnTitleAsString(this.dutchTitle);
        set => this.setTitleFromString(value, 6);
      }

      public string KoreanTitle
      {
        get => this.returnTitleAsString(this.koreanTitle);
        set => this.setTitleFromString(value, 7);
      }

      public string[] AllTitles => new string[8]
      {
        this.JapaneseTitle,
        this.EnglishTitle,
        this.GermanTitle,
        this.FrenchTitle,
        this.SpanishTitle,
        this.ItalianTitle,
        this.DutchTitle,
        this.KoreanTitle
      };

      public bool HashesMatch => this.hashesMatch;

      public static Headers.IMET Load(string pathToFile) => Headers.IMET.Load(File.ReadAllBytes(pathToFile));

      public static Headers.IMET Load(byte[] fileOrHeader)
      {
        Headers.HeaderType headerType = Headers.DetectHeader(fileOrHeader);
        switch (headerType)
        {
          case Headers.HeaderType.ShortIMET:
          case Headers.HeaderType.IMET:
            Headers.IMET imet = new Headers.IMET();
            if (headerType == Headers.HeaderType.ShortIMET)
              imet.isShortImet = true;
            MemoryStream memoryStream = new MemoryStream(fileOrHeader);
            try
            {
              imet.parseHeader((Stream) memoryStream);
            }
            catch
            {
              memoryStream.Dispose();
              throw;
            }
            memoryStream.Dispose();
            return imet;
          default:
            throw new Exception("No IMET Header found!");
        }
      }

      public static Headers.IMET Load(Stream fileOrHeader)
      {
        Headers.HeaderType headerType = Headers.DetectHeader(fileOrHeader);
        switch (headerType)
        {
          case Headers.HeaderType.ShortIMET:
          case Headers.HeaderType.IMET:
            Headers.IMET imet = new Headers.IMET();
            if (headerType == Headers.HeaderType.ShortIMET)
              imet.isShortImet = true;
            imet.parseHeader(fileOrHeader);
            return imet;
          default:
            throw new Exception("No IMET Header found!");
        }
      }

      public static Headers.IMET Create(
        bool isShortImet,
        int iconSize,
        int bannerSize,
        int soundSize,
        params string[] titles)
      {
        Headers.IMET imet = new Headers.IMET();
        imet.isShortImet = isShortImet;
        for (int titleIndex = 0; titleIndex < titles.Length; ++titleIndex)
          imet.setTitleFromString(titles[titleIndex], titleIndex);
        for (int length = titles.Length; length < 8; ++length)
          imet.setTitleFromString(titles.Length > 1 ? titles[1] : titles[0], length);
        imet.iconSize = (uint) iconSize;
        imet.bannerSize = (uint) bannerSize;
        imet.soundSize = (uint) soundSize;
        return imet;
      }

      public static void RemoveHeader(string pathToFile)
      {
        byte[] bytes = Headers.IMET.RemoveHeader(File.ReadAllBytes(pathToFile));
        File.Delete(pathToFile);
        File.WriteAllBytes(pathToFile, bytes);
      }

      public static byte[] RemoveHeader(byte[] file)
      {
        Headers.HeaderType headerType = Headers.DetectHeader(file);
        switch (headerType)
        {
          case Headers.HeaderType.ShortIMET:
          case Headers.HeaderType.IMET:
            byte[] numArray = new byte[(int) (file.Length - headerType)];
            Array.Copy((Array) file, (int) headerType, (Array) numArray, 0, numArray.Length);
            return numArray;
          default:
            throw new Exception("No IMET Header found!");
        }
      }

      public void SetAllTitles(string newTitle)
      {
        for (int titleIndex = 0; titleIndex < 10; ++titleIndex)
          this.setTitleFromString(newTitle, titleIndex);
      }

      public MemoryStream ToMemoryStream()
      {
        MemoryStream memoryStream = new MemoryStream();
        try
        {
          this.writeToStream((Stream) memoryStream);
          return memoryStream;
        }
        catch
        {
          memoryStream.Dispose();
          throw;
        }
      }

      public byte[] ToByteArray() => this.ToMemoryStream().ToArray();

      public void Write(Stream writeStream) => this.writeToStream(writeStream);

      public void ChangeTitles(params string[] newTitles)
      {
        for (int titleIndex = 0; titleIndex < newTitles.Length; ++titleIndex)
          this.setTitleFromString(newTitles[titleIndex], titleIndex);
        for (int length = newTitles.Length; length < 8; ++length)
          this.setTitleFromString(newTitles.Length > 1 ? newTitles[1] : newTitles[0], length);
      }

      public string[] GetTitles() => new string[8]
      {
        this.JapaneseTitle,
        this.EnglishTitle,
        this.GermanTitle,
        this.FrenchTitle,
        this.SpanishTitle,
        this.ItalianTitle,
        this.DutchTitle,
        this.KoreanTitle
      };

      private void writeToStream(Stream writeStream)
      {
        writeStream.Seek(0L, SeekOrigin.Begin);
        if (!this.isShortImet)
          writeStream.Write(this.additionalPadding, 0, this.additionalPadding.Length);
        writeStream.Write(this.padding, 0, this.padding.Length);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.imetMagic)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.sizeOfHeader)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.unknown)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.iconSize)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.bannerSize)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.soundSize)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.flags)), 0, 4);
        writeStream.Write(this.japaneseTitle, 0, this.japaneseTitle.Length);
        writeStream.Write(this.englishTitle, 0, this.englishTitle.Length);
        writeStream.Write(this.germanTitle, 0, this.germanTitle.Length);
        writeStream.Write(this.frenchTitle, 0, this.frenchTitle.Length);
        writeStream.Write(this.spanishTitle, 0, this.spanishTitle.Length);
        writeStream.Write(this.italianTitle, 0, this.italianTitle.Length);
        writeStream.Write(this.dutchTitle, 0, this.dutchTitle.Length);
        writeStream.Write(this.unknownTitle1, 0, this.unknownTitle1.Length);
        writeStream.Write(this.unknownTitle2, 0, this.unknownTitle2.Length);
        writeStream.Write(this.koreanTitle, 0, this.koreanTitle.Length);
        writeStream.Write(this.padding2, 0, this.padding2.Length);
        int position = (int) writeStream.Position;
        this.hash = new byte[16];
        writeStream.Write(this.hash, 0, this.hash.Length);
        byte[] numArray = new byte[writeStream.Position];
        writeStream.Seek(0L, SeekOrigin.Begin);
        writeStream.Read(numArray, 0, numArray.Length);
        this.computeHash(numArray, !this.isShortImet ? 64 : 0);
        writeStream.Seek((long) position, SeekOrigin.Begin);
        writeStream.Write(this.hash, 0, this.hash.Length);
      }

      private void computeHash(byte[] headerBytes, int hashPos)
      {
        MD5 md5 = MD5.Create();
        this.hash = md5.ComputeHash(headerBytes, hashPos, 1536);
        md5.Clear();
      }

      private void parseHeader(Stream headerStream)
      {
        headerStream.Seek(0L, SeekOrigin.Begin);
        byte[] buffer1 = new byte[4];
        if (!this.isShortImet)
          headerStream.Read(this.additionalPadding, 0, this.additionalPadding.Length);
        headerStream.Read(this.padding, 0, this.padding.Length);
        headerStream.Read(buffer1, 0, 4);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int) this.imetMagic)
          throw new Exception("Invalid Magic!");
        headerStream.Read(buffer1, 0, 4);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int) this.sizeOfHeader)
          throw new Exception("Invalid Header Size!");
        headerStream.Read(buffer1, 0, 4);
        this.unknown = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        headerStream.Read(buffer1, 0, 4);
        this.iconSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        headerStream.Read(buffer1, 0, 4);
        this.bannerSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        headerStream.Read(buffer1, 0, 4);
        this.soundSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        headerStream.Read(buffer1, 0, 4);
        this.flags = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        headerStream.Read(this.japaneseTitle, 0, this.japaneseTitle.Length);
        headerStream.Read(this.englishTitle, 0, this.englishTitle.Length);
        headerStream.Read(this.germanTitle, 0, this.germanTitle.Length);
        headerStream.Read(this.frenchTitle, 0, this.frenchTitle.Length);
        headerStream.Read(this.spanishTitle, 0, this.spanishTitle.Length);
        headerStream.Read(this.italianTitle, 0, this.italianTitle.Length);
        headerStream.Read(this.dutchTitle, 0, this.dutchTitle.Length);
        headerStream.Read(this.unknownTitle1, 0, this.unknownTitle1.Length);
        headerStream.Read(this.unknownTitle2, 0, this.unknownTitle2.Length);
        headerStream.Read(this.koreanTitle, 0, this.koreanTitle.Length);
        headerStream.Read(this.padding2, 0, this.padding2.Length);
        headerStream.Read(this.hash, 0, this.hash.Length);
        headerStream.Seek(-16L, SeekOrigin.Current);
        headerStream.Write(new byte[16], 0, 16);
        byte[] buffer2 = new byte[headerStream.Length];
        headerStream.Seek(0L, SeekOrigin.Begin);
        headerStream.Read(buffer2, 0, buffer2.Length);
        MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(buffer2, !this.isShortImet ? 64 : 0, 1536);
        md5.Clear();
        this.hashesMatch = Shared.CompareByteArrays(hash, this.hash);
      }

      private string returnTitleAsString(byte[] title)
      {
        string empty = string.Empty;
        for (int index = 0; index < 84; index += 2)
        {
          char ch = BitConverter.ToChar(new byte[2]
          {
            title[index + 1],
            title[index]
          }, 0);
          if (ch != char.MinValue)
            empty += ch.ToString();
        }
        return empty;
      }

      private void setTitleFromString(string title, int titleIndex)
      {
        byte[] numArray = new byte[84];
        for (int index = 0; index < title.Length; ++index)
        {
          byte[] bytes = BitConverter.GetBytes(title[index]);
          numArray[index * 2 + 1] = bytes[0];
          numArray[index * 2] = bytes[1];
        }
        switch (titleIndex)
        {
          case 0:
            this.japaneseTitle = numArray;
            break;
          case 1:
            this.englishTitle = numArray;
            break;
          case 2:
            this.germanTitle = numArray;
            break;
          case 3:
            this.frenchTitle = numArray;
            break;
          case 4:
            this.spanishTitle = numArray;
            break;
          case 5:
            this.italianTitle = numArray;
            break;
          case 6:
            this.dutchTitle = numArray;
            break;
          case 7:
            this.koreanTitle = numArray;
            break;
        }
      }
    }

    public class IMD5
    {
      private uint imd5Magic = 1229800501;
      private uint fileSize;
      private byte[] padding = new byte[8];
      private byte[] hash = new byte[16];

      public uint FileSize => this.fileSize;

      public byte[] Hash => this.hash;

      private IMD5()
      {
      }

      public static Headers.IMD5 Load(string pathToFile) => Headers.IMD5.Load(File.ReadAllBytes(pathToFile));

      public static Headers.IMD5 Load(byte[] fileOrHeader)
      {
        if (Headers.DetectHeader(fileOrHeader) != Headers.HeaderType.IMD5)
          throw new Exception("No IMD5 Header found!");
        Headers.IMD5 imD5 = new Headers.IMD5();
        MemoryStream memoryStream = new MemoryStream(fileOrHeader);
        try
        {
          imD5.parseHeader((Stream) memoryStream);
        }
        catch
        {
          memoryStream.Dispose();
          throw;
        }
        memoryStream.Dispose();
        return imD5;
      }

      public static Headers.IMD5 Load(Stream fileOrHeader)
      {
        if (Headers.DetectHeader(fileOrHeader) != Headers.HeaderType.IMD5)
          throw new Exception("No IMD5 Header found!");
        Headers.IMD5 imD5 = new Headers.IMD5();
        imD5.parseHeader(fileOrHeader);
        return imD5;
      }

      public static Headers.IMD5 Create(byte[] file)
      {
        Headers.IMD5 imD5 = new Headers.IMD5();
        imD5.fileSize = (uint) file.Length;
        imD5.computeHash(file);
        return imD5;
      }

      public static void AddHeader(string pathToFile)
      {
        byte[] buffer = Headers.IMD5.AddHeader(File.ReadAllBytes(pathToFile));
        File.Delete(pathToFile);
        using (FileStream fileStream = new FileStream(pathToFile, FileMode.Create))
          fileStream.Write(buffer, 0, buffer.Length);
      }

      public static byte[] AddHeader(byte[] file)
      {
        Headers.IMD5 imD5 = Headers.IMD5.Create(file);
        MemoryStream memoryStream1 = new MemoryStream();
        MemoryStream memoryStream2 = memoryStream1;
        imD5.writeToStream((Stream) memoryStream2);
        memoryStream1.Write(file, 0, file.Length);
        byte[] array = memoryStream1.ToArray();
        memoryStream1.Dispose();
        return array;
      }

      public static void RemoveHeader(string pathToFile)
      {
        byte[] buffer = Headers.IMD5.RemoveHeader(File.ReadAllBytes(pathToFile));
        File.Delete(pathToFile);
        using (FileStream fileStream = new FileStream(pathToFile, FileMode.Create))
          fileStream.Write(buffer, 0, buffer.Length);
      }

      public static byte[] RemoveHeader(byte[] file)
      {
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(file, 32, file.Length - 32);
        byte[] array = memoryStream.ToArray();
        memoryStream.Dispose();
        return array;
      }

      public MemoryStream ToMemoryStream()
      {
        MemoryStream memoryStream = new MemoryStream();
        try
        {
          this.writeToStream((Stream) memoryStream);
          return memoryStream;
        }
        catch
        {
          memoryStream.Dispose();
          throw;
        }
      }

      public byte[] ToByteArray() => this.ToMemoryStream().ToArray();

      public void Write(Stream writeStream) => this.writeToStream(writeStream);

      private void writeToStream(Stream writeStream)
      {
        writeStream.Seek(0L, SeekOrigin.Begin);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.imd5Magic)), 0, 4);
        writeStream.Write(BitConverter.GetBytes(Shared.Swap(this.fileSize)), 0, 4);
        writeStream.Write(this.padding, 0, this.padding.Length);
        writeStream.Write(this.hash, 0, this.hash.Length);
      }

      private void computeHash(byte[] bytesToHash)
      {
        MD5 md5 = MD5.Create();
        this.hash = md5.ComputeHash(bytesToHash);
        md5.Clear();
      }

      private void parseHeader(Stream headerStream)
      {
        headerStream.Seek(0L, SeekOrigin.Begin);
        byte[] buffer = new byte[4];
        headerStream.Read(buffer, 0, 4);
        if ((int) Shared.Swap(BitConverter.ToUInt32(buffer, 0)) != (int) this.imd5Magic)
          throw new Exception("Invalid Magic!");
        headerStream.Read(buffer, 0, 4);
        this.fileSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
        headerStream.Read(this.padding, 0, this.padding.Length);
        headerStream.Read(this.hash, 0, this.hash.Length);
      }
    }
  }
}
