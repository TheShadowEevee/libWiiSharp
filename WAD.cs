// Decompiled with JetBrains decompiler
// Type: libWiiSharp.WAD
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace libWiiSharp
{
    public enum LowerTitleID : uint
    {
        SystemTitles = 0x00000001,
        SystemChannels = 0x00010002,
        Channel = 0x00010001,
        GameChannel = 0x00010004,
        DLC = 0x00010005,
        HiddenChannels = 0x00010008,
    }
    public class WAD : IDisposable
  {
    private SHA1 sha = SHA1.Create();
    private DateTime creationTimeUTC = new DateTime(1970, 1, 1);
    private bool hasBanner;
    private bool lz77CompressBannerAndIcon = true;
    private bool lz77DecompressBannerAndIcon;
    private bool keepOriginalFooter;
    private WAD_Header wadHeader;
    private CertificateChain cert = new CertificateChain();
    private Ticket tik = new Ticket();
    private TMD tmd = new TMD();
    private List<byte[]> contents;
    private U8 bannerApp = new U8();
    private byte[] footer = new byte[0];
    private bool isDisposed;

    public Region Region
    {
      get => this.tmd.Region;
      set => this.tmd.Region = value;
    }

    public int NumOfContents => (int) this.tmd.NumOfContents;

    public byte[][] Contents => this.contents.ToArray();

    public bool FakeSign
    {
      get => this.tik.FakeSign && this.tmd.FakeSign;
      set
      {
        this.tik.FakeSign = value;
        this.tmd.FakeSign = value;
      }
    }

    public U8 BannerApp
    {
      get => this.bannerApp;
      set => this.bannerApp = value;
    }

    public ulong StartupIOS
    {
      get => this.tmd.StartupIOS;
      set => this.tmd.StartupIOS = value;
    }

    public ulong TitleID
    {
      get => this.tik.TitleID;
      set
      {
        this.tik.TitleID = value;
        this.tmd.TitleID = value;
      }
    }

    public string UpperTitleID => this.tik.GetUpperTitleID();

    public ushort TitleVersion
    {
      get => this.tmd.TitleVersion;
      set => this.tmd.TitleVersion = value;
    }

    public ushort BootIndex
    {
      get => this.tmd.BootIndex;
      set => this.tmd.BootIndex = value;
    }

    public DateTime CreationTimeUTC => this.creationTimeUTC;

    public bool HasBanner => this.hasBanner;

    public bool Lz77CompressBannerAndIcon
    {
      get => this.lz77CompressBannerAndIcon;
      set
      {
        this.lz77CompressBannerAndIcon = value;
        if (!value)
          return;
        this.lz77DecompressBannerAndIcon = false;
      }
    }

    public bool Lz77DecompressBannerAndIcon
    {
      get => this.lz77DecompressBannerAndIcon;
      set
      {
        this.lz77DecompressBannerAndIcon = value;
        if (!value)
          return;
        this.lz77CompressBannerAndIcon = false;
      }
    }

    public string NandBlocks => this.tmd.GetNandBlocks();

    public string[] ChannelTitles
    {
      get => this.hasBanner ? ((Headers.IMET) this.bannerApp.Header).AllTitles : new string[0];
      set => this.ChangeChannelTitles(value);
    }

    public bool KeepOriginalFooter
    {
      get => this.keepOriginalFooter;
      set => this.keepOriginalFooter = value;
    }

    public TMD_Content[] TmdContents => this.tmd.Contents;

    public CommonKeyType CommonKeyType
    {
      get => this.tik.CommonKeyIndex;
      set => this.tik.CommonKeyIndex = value;
    }

    public bool SortContents
    {
      get => this.tmd.SortContents;
      set => this.tmd.SortContents = value;
    }

    public event EventHandler<ProgressChangedEventArgs> Progress;

    public event EventHandler<MessageEventArgs> Warning;

    public event EventHandler<MessageEventArgs> Debug;

    public WAD()
    {
      this.cert.Debug += new EventHandler<MessageEventArgs>(this.cert_Debug);
      this.tik.Debug += new EventHandler<MessageEventArgs>(this.tik_Debug);
      this.tmd.Debug += new EventHandler<MessageEventArgs>(this.tmd_Debug);
      this.bannerApp.Debug += new EventHandler<MessageEventArgs>(this.bannerApp_Debug);
      this.bannerApp.Warning += new EventHandler<MessageEventArgs>(this.bannerApp_Warning);
    }

    ~WAD() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.sha.Clear();
        this.sha = (SHA1) null;
        this.wadHeader = (WAD_Header) null;
        this.cert.Dispose();
        this.tik.Dispose();
        this.tmd.Dispose();
        this.contents.Clear();
        this.contents = (List<byte[]>) null;
        this.bannerApp.Dispose();
        this.footer = (byte[]) null;
      }
      this.isDisposed = true;
    }

    public static WAD Load(string pathToWad) => WAD.Load(File.ReadAllBytes(pathToWad));

    public static WAD Load(byte[] wadFile)
    {
      WAD wad = new WAD();
      MemoryStream memoryStream = new MemoryStream(wadFile);
      try
      {
        wad.parseWad((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
      return wad;
    }

    public static WAD Load(Stream wad)
    {
      WAD wad1 = new WAD();
      wad1.parseWad(wad);
      return wad1;
    }

    public static WAD Create(string contentDir)
    {
      string[] files1 = Directory.GetFiles(contentDir, "*cert*");
      string[] files2 = Directory.GetFiles(contentDir, "*tik*");
      string[] files3 = Directory.GetFiles(contentDir, "*tmd*");
      CertificateChain cert = CertificateChain.Load(files1[0]);
      Ticket tik = Ticket.Load(files2[0]);
      TMD tmd = TMD.Load(files3[0]);
      bool flag = true;
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].ContentID.ToString("x8") + ".app"))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < tmd.Contents.Length; ++index)
        {
          if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].Index.ToString("x8") + ".app"))
            throw new Exception("Couldn't find all content files!");
        }
      }
      byte[][] contents = new byte[tmd.Contents.Length][];
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
        contents[index] = File.ReadAllBytes(path);
      }
      return WAD.Create(cert, tik, tmd, contents);
    }

    public static WAD Create(
      string pathToCert,
      string pathToTik,
      string pathToTmd,
      string contentDir)
    {
      CertificateChain cert = CertificateChain.Load(pathToCert);
      Ticket tik = Ticket.Load(pathToTik);
      TMD tmd = TMD.Load(pathToTmd);
      bool flag = true;
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].ContentID.ToString("x8") + ".app"))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < tmd.Contents.Length; ++index)
        {
          if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].Index.ToString("x8") + ".app"))
            throw new Exception("Couldn't find all content files!");
        }
      }
      byte[][] contents = new byte[tmd.Contents.Length][];
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
        contents[index] = File.ReadAllBytes(path);
      }
      return WAD.Create(cert, tik, tmd, contents);
    }

    public static WAD Create(byte[] cert, byte[] tik, byte[] tmd, byte[][] contents)
    {
      CertificateChain cert1 = CertificateChain.Load(cert);
      Ticket ticket = Ticket.Load(tik);
      TMD tmd1 = TMD.Load(tmd);
      Ticket tik1 = ticket;
      TMD tmd2 = tmd1;
      byte[][] contents1 = contents;
      return WAD.Create(cert1, tik1, tmd2, contents1);
    }

    public static WAD Create(CertificateChain cert, Ticket tik, TMD tmd, byte[][] contents)
    {
      WAD wad = new WAD()
      {
        cert = cert,
        tik = tik,
        tmd = tmd,
        contents = new List<byte[]>((IEnumerable<byte[]>) contents),
        wadHeader = new WAD_Header()
      };
      wad.wadHeader.TmdSize = (uint) (484 + tmd.Contents.Length * 36);
      int num1 = 0;
      for (int index = 0; index < contents.Length - 1; ++index)
        num1 += Shared.AddPadding(contents[index].Length);
      int num2 = num1 + contents[contents.Length - 1].Length;
      wad.wadHeader.ContentSize = (uint) num2;
      for (int index = 0; index < wad.tmd.Contents.Length; ++index)
      {
        if (wad.tmd.Contents[index].Index == (ushort) 0)
        {
          try
          {
            wad.bannerApp.LoadFile(contents[index]);
            wad.hasBanner = true;
            return wad;
          }
          catch
          {
            wad.hasBanner = false;
            return wad;
          }
        }
      }
      return wad;
    }

    public void LoadFile(string pathToWad) => this.LoadFile(File.ReadAllBytes(pathToWad));

    public void LoadFile(byte[] wadFile)
    {
      MemoryStream memoryStream = new MemoryStream(wadFile);
      try
      {
        this.parseWad((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
    }

    public void LoadFile(Stream wad) => this.parseWad(wad);

    public void CreateNew(string contentDir)
    {
      string[] files1 = Directory.GetFiles(contentDir, "*cert*");
      string[] files2 = Directory.GetFiles(contentDir, "*tik*");
      string[] files3 = Directory.GetFiles(contentDir, "*tmd*");
      CertificateChain cert = CertificateChain.Load(files1[0]);
      Ticket tik = Ticket.Load(files2[0]);
      TMD tmd = TMD.Load(files3[0]);
      bool flag = true;
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].ContentID.ToString("x8") + ".app"))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < tmd.Contents.Length; ++index)
        {
          if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].Index.ToString("x8") + ".app"))
            throw new Exception("Couldn't find all content files!");
        }
      }
      byte[][] contents = new byte[tmd.Contents.Length][];
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
        contents[index] = File.ReadAllBytes(path);
      }
      this.CreateNew(cert, tik, tmd, contents);
    }

    public void CreateNew(
      string pathToCert,
      string pathToTik,
      string pathToTmd,
      string contentDir)
    {
      CertificateChain cert = CertificateChain.Load(pathToCert);
      Ticket tik = Ticket.Load(pathToTik);
      TMD tmd = TMD.Load(pathToTmd);
      bool flag = true;
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].ContentID.ToString("x8") + ".app"))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < tmd.Contents.Length; ++index)
        {
          if (!File.Exists(contentDir + Path.DirectorySeparatorChar.ToString() + tmd.Contents[index].Index.ToString("x8") + ".app"))
            throw new Exception("Couldn't find all content files!");
        }
      }
      byte[][] contents = new byte[tmd.Contents.Length][];
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
        contents[index] = File.ReadAllBytes(path);
      }
      this.CreateNew(cert, tik, tmd, contents);
    }

    public void CreateNew(byte[] cert, byte[] tik, byte[] tmd, byte[][] contents) => this.CreateNew(CertificateChain.Load(cert), Ticket.Load(tik), TMD.Load(tmd), contents);

    public void CreateNew(CertificateChain cert, Ticket tik, TMD tmd, byte[][] contents)
    {
      this.cert = cert;
      this.tik = tik;
      this.tmd = tmd;
      this.contents = new List<byte[]>((IEnumerable<byte[]>) contents);
      this.wadHeader = new WAD_Header();
      this.wadHeader.TmdSize = (uint) (484 + tmd.Contents.Length * 36);
      int num = 0;
      for (int index = 0; index < contents.Length - 1; ++index)
        num += Shared.AddPadding(contents[index].Length);
      this.wadHeader.ContentSize = (uint) (num + contents[contents.Length - 1].Length);
      for (int index = 0; index < this.tmd.Contents.Length; ++index)
      {
        if (this.tmd.Contents[index].Index == (ushort) 0)
        {
          try
          {
            this.bannerApp.LoadFile(contents[index]);
            this.hasBanner = true;
            break;
          }
          catch
          {
            this.hasBanner = false;
            break;
          }
        }
      }
    }

    public void Save(string savePath)
    {
      if (File.Exists(savePath))
        File.Delete(savePath);
      using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
        this.writeToStream((Stream) fileStream);
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

    public byte[] ToByteArray()
    {
      MemoryStream memoryStream = new MemoryStream();
      try
      {
        this.writeToStream((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      byte[] array = memoryStream.ToArray();
      memoryStream.Dispose();
      return array;
    }

    public void ChangeTitleID(LowerTitleID lowerID, string upperID)
    {
      uint num1 = upperID.Length == 4 ? BitConverter.ToUInt32(new byte[4]
      {
        (byte) upperID[3],
        (byte) upperID[2],
        (byte) upperID[1],
        (byte) upperID[0]
      }, 0) : throw new Exception("Upper Title ID must be 4 characters long!");
      ulong num2 = (ulong) lowerID << 32 | (ulong) num1;
      this.tik.TitleID = num2;
      this.tmd.TitleID = num2;
    }

    public void ChangeStartupIOS(int newIos) => this.StartupIOS = 4294967296UL | (ulong) (uint) newIos;

    public void ChangeTitleKey(string newTitleKey) => this.tik.SetTitleKey(newTitleKey);

    public void ChangeTitleKey(char[] newTitleKey) => this.tik.SetTitleKey(newTitleKey);

    public void ChangeTitleKey(byte[] newTitleKey) => this.tik.SetTitleKey(newTitleKey);

    public byte[] GetContentByIndex(int index)
    {
      for (int index1 = 0; index1 < (int) this.tmd.NumOfContents; ++index1)
      {
        if ((int) this.tmd.Contents[index1].Index == index)
          return this.contents[index1];
      }
      throw new Exception(string.Format("Content with index {0} not found!", (object) index));
    }

    public byte[] GetContentByID(int contentID)
    {
      for (int index = 0; index < (int) this.tmd.NumOfContents; ++index)
      {
        if ((int) this.tmd.Contents[index].Index == contentID)
          return this.contents[index];
      }
      throw new Exception(string.Format("Content with content ID {0} not found!", (object) contentID));
    }

    public void ChangeChannelTitles(params string[] newTitles)
    {
      if (!this.hasBanner)
        return;
      ((Headers.IMET) this.bannerApp.Header).ChangeTitles(newTitles);
    }

    public void AddContent(byte[] newContent, int contentID, int index, ContentType type = ContentType.Normal)
    {
      this.tmd.AddContent(new TMD_Content()
      {
        ContentID = (uint) contentID,
        Index = (ushort) index,
        Type = type,
        Size = (ulong) newContent.Length,
        Hash = this.sha.ComputeHash(newContent)
      });
      this.contents.Add(newContent);
      this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
    }

    public void RemoveContent(int index)
    {
      for (int index1 = 0; index1 < this.tmd.Contents.Length; ++index1)
      {
        if ((int) this.tmd.Contents[index1].Index == index)
        {
          this.tmd.RemoveContent(index);
          this.contents.RemoveAt(index1);
          this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
          return;
        }
      }
      throw new Exception(string.Format("Content with index {0} not found!", (object) index));
    }

    public void RemoveContentByID(int contentID)
    {
      for (int index = 0; index < this.tmd.Contents.Length; ++index)
      {
        if ((int) this.tmd.Contents[index].Index == contentID)
        {
          this.tmd.RemoveContentByID(contentID);
          this.contents.RemoveAt(index);
          this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
          return;
        }
      }
      throw new Exception(string.Format("Content with content ID {0} not found!", (object) contentID));
    }

    public void RemoveAllContents()
    {
      if (!this.hasBanner)
      {
        this.tmd.Contents = new TMD_Content[0];
        this.contents = new List<byte[]>();
        this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
      }
      else
      {
        for (int index = 0; index < (int) this.tmd.NumOfContents; ++index)
        {
          if (this.tmd.Contents[index].Index == (ushort) 0)
          {
            byte[] content1 = this.contents[index];
            TMD_Content content2 = this.tmd.Contents[index];
            this.tmd.Contents = new TMD_Content[0];
            this.contents = new List<byte[]>();
            this.tmd.AddContent(content2);
            this.contents.Add(content1);
            this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
            break;
          }
        }
      }
    }

    public void Unpack(string unpackDir, bool nameContentID = false) => this.unpackAll(unpackDir, nameContentID);

    public void RemoveFooter()
    {
      this.footer = new byte[0];
      this.wadHeader.FooterSize = 0U;
      this.keepOriginalFooter = true;
    }

    public void AddFooter(byte[] footer) => this.ChangeFooter(footer);

    public void ChangeFooter(byte[] newFooter)
    {
      if (newFooter.Length % 64 != 0)
        Array.Resize<byte>(ref newFooter, Shared.AddPadding(newFooter.Length));
      this.footer = newFooter;
      this.wadHeader.FooterSize = (uint) newFooter.Length;
      this.keepOriginalFooter = true;
    }

    private void writeToStream(Stream writeStream)
    {
      this.fireDebug("Writing Wad...");
      if (!this.keepOriginalFooter)
      {
        this.fireDebug("   Building Footer Timestamp...");
        this.createFooterTimestamp();
      }
      if (this.hasBanner)
      {
        if (this.lz77CompressBannerAndIcon || this.lz77DecompressBannerAndIcon)
        {
          for (int index = 0; index < this.bannerApp.Nodes.Count; ++index)
          {
            if (this.bannerApp.StringTable[index].ToLower() == "icon.bin" || this.bannerApp.StringTable[index].ToLower() == "banner.bin")
            {
              if (!Lz77.IsLz77Compressed(this.bannerApp.Data[index]) && this.lz77CompressBannerAndIcon)
              {
                this.fireDebug("   Compressing {0}...", (object) this.bannerApp.StringTable[index]);
                byte[] file = new byte[this.bannerApp.Data[index].Length - 32];
                Array.Copy((Array) this.bannerApp.Data[index], 32, (Array) file, 0, file.Length);
                byte[] numArray = Headers.IMD5.AddHeader(new Lz77().Compress(file));
                this.bannerApp.Data[index] = numArray;
                this.bannerApp.Nodes[index].SizeOfData = (uint) numArray.Length;
              }
              else if (Lz77.IsLz77Compressed(this.bannerApp.Data[index]) && this.lz77DecompressBannerAndIcon)
              {
                this.fireDebug("   Decompressing {0}...", (object) this.bannerApp.StringTable[index]);
                byte[] file = new byte[this.bannerApp.Data[index].Length - 32];
                Array.Copy((Array) this.bannerApp.Data[index], 32, (Array) file, 0, file.Length);
                byte[] numArray = Headers.IMD5.AddHeader(new Lz77().Decompress(file));
                this.bannerApp.Data[index] = numArray;
                this.bannerApp.Nodes[index].SizeOfData = (uint) numArray.Length;
              }
            }
          }
        }
        for (int index = 0; index < this.contents.Count; ++index)
        {
          if (this.tmd.Contents[index].Index == (ushort) 0)
          {
            this.fireDebug("   Saving Banner App...");
            this.contents[index] = this.bannerApp.ToByteArray();
            break;
          }
        }
      }
      this.fireDebug("   Updating Header...");
      int num = 0;
      for (int index = 0; index < this.contents.Count - 1; ++index)
        num += Shared.AddPadding(this.contents[index].Length);
      this.wadHeader.ContentSize = (uint) (num + this.contents[this.contents.Count - 1].Length);
      this.wadHeader.TmdSize = (uint) (484 + (int) this.tmd.NumOfContents * 36);
      this.fireDebug("   Updating TMD Contents...");
      this.tmd.UpdateContents(this.contents.ToArray());
      this.fireDebug("   Writing Wad Header... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper());
      writeStream.Seek(0L, SeekOrigin.Begin);
      this.wadHeader.Write(writeStream);
      this.fireDebug("   Writing Certificate Chain... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper());
      writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position), SeekOrigin.Begin);
      byte[] byteArray1 = this.cert.ToByteArray();
      writeStream.Write(byteArray1, 0, byteArray1.Length);
      this.fireDebug("   Writing Ticket... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper());
      writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position), SeekOrigin.Begin);
      byte[] byteArray2 = this.tik.ToByteArray();
      writeStream.Write(byteArray2, 0, byteArray2.Length);
      this.fireDebug("   Writing TMD... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper());
      writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position), SeekOrigin.Begin);
      byte[] byteArray3 = this.tmd.ToByteArray();
      writeStream.Write(byteArray3, 0, byteArray3.Length);
      ContentIndices[] sortedContentList = this.tmd.GetSortedContentList();
      for (int index = 0; index < sortedContentList.Length; ++index)
      {
        writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position), SeekOrigin.Begin);
        this.fireProgress((index + 1) * 100 / this.contents.Count);
        this.fireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper(), (object) (index + 1), (object) this.contents.Count);
        this.fireDebug("    -> Content ID: 0x{0}", (object) this.tmd.Contents[sortedContentList[index].Index].ContentID.ToString("x8"));
        this.fireDebug("    -> Index: 0x{0}", (object) this.tmd.Contents[sortedContentList[index].Index].Index.ToString("x4"));
        this.fireDebug("    -> Type: 0x{0} ({1})", (object) ((ushort) this.tmd.Contents[sortedContentList[index].Index].Type).ToString("x4"), (object) this.tmd.Contents[sortedContentList[index].Index].Type.ToString());
        this.fireDebug("    -> Size: {0} bytes", (object) this.tmd.Contents[sortedContentList[index].Index].Size);
        this.fireDebug("    -> Hash: {0}", (object) Shared.ByteArrayToString(this.tmd.Contents[sortedContentList[index].Index].Hash));
        byte[] buffer = this.encryptContent(this.contents[sortedContentList[index].Index], sortedContentList[index].Index);
        writeStream.Write(buffer, 0, buffer.Length);
      }
      if (this.wadHeader.FooterSize != 0U)
      {
        this.fireDebug("   Writing Footer... (Offset: 0x{0})", (object) writeStream.Position.ToString("x8").ToUpper());
        writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position), SeekOrigin.Begin);
        writeStream.Write(this.footer, 0, this.footer.Length);
      }
      while (writeStream.Position % 64L != 0L)
        writeStream.WriteByte((byte) 0);
      this.fireDebug("Writing Wad Finished... (Written Bytes: {0})", (object) writeStream.Position);
    }

    private void unpackAll(string unpackDir, bool nameContentId)
    {
      this.fireDebug("Unpacking Wad to: {0}", (object) unpackDir);
      if (!Directory.Exists(unpackDir))
        Directory.CreateDirectory(unpackDir);
      string str1 = this.tik.TitleID.ToString("x16");
      this.fireDebug("   Saving Certificate Chain: {0}.cert", (object) str1);
      CertificateChain cert = this.cert;
      string str2 = unpackDir;
      char directorySeparatorChar = Path.DirectorySeparatorChar;
      string str3 = directorySeparatorChar.ToString();
      string str4 = str1;
      string savePath1 = str2 + str3 + str4 + ".cert";
      cert.Save(savePath1);
      this.fireDebug("   Saving Ticket: {0}.tik", (object) str1);
      Ticket tik = this.tik;
      string str5 = unpackDir;
      directorySeparatorChar = Path.DirectorySeparatorChar;
      string str6 = directorySeparatorChar.ToString();
      string str7 = str1;
      string savePath2 = str5 + str6 + str7 + ".tik";
      tik.Save(savePath2);
      this.fireDebug("   Saving TMD: {0}.tmd", (object) str1);
      TMD tmd = this.tmd;
      string str8 = unpackDir;
      directorySeparatorChar = Path.DirectorySeparatorChar;
      string str9 = directorySeparatorChar.ToString();
      string str10 = str1;
      string savePath3 = str8 + str9 + str10 + ".tmd";
      tmd.Save(savePath3);
      for (int index = 0; index < (int) this.tmd.NumOfContents; ++index)
      {
        this.fireProgress((index + 1) * 100 / (int) this.tmd.NumOfContents);
        this.fireDebug("   Saving Content #{0} of {1}: {2}.app", (object) (index + 1), (object) this.tmd.NumOfContents, nameContentId ? (object) this.tmd.Contents[index].ContentID.ToString("x8") : (object) this.tmd.Contents[index].Index.ToString("x8"));
        this.fireDebug("    -> Content ID: 0x{0}", (object) this.tmd.Contents[index].ContentID.ToString("x8"));
        this.fireDebug("    -> Index: 0x{0}", (object) this.tmd.Contents[index].Index.ToString("x4"));
        object[] objArray = new object[2];
        ushort num = (ushort) this.tmd.Contents[index].Type;
        objArray[0] = (object) num.ToString("x4");
        objArray[1] = (object) this.tmd.Contents[index].Type.ToString();
        this.fireDebug("    -> Type: 0x{0} ({1})", objArray);
        this.fireDebug("    -> Size: {0} bytes", (object) this.tmd.Contents[index].Size);
        this.fireDebug("    -> Hash: {0}", (object) Shared.ByteArrayToString(this.tmd.Contents[index].Hash));
        string str11 = unpackDir;
        directorySeparatorChar = Path.DirectorySeparatorChar;
        string str12 = directorySeparatorChar.ToString();
        string str13;
        if (!nameContentId)
        {
          num = this.tmd.Contents[index].Index;
          str13 = num.ToString("x8");
        }
        else
          str13 = this.tmd.Contents[index].ContentID.ToString("x8");
        using (FileStream fileStream = new FileStream(str11 + str12 + str13 + ".app", FileMode.Create))
          fileStream.Write(this.contents[index], 0, this.contents[index].Length);
      }
      this.fireDebug("   Saving Footer: {0}.footer", (object) str1);
      string str14 = unpackDir;
      directorySeparatorChar = Path.DirectorySeparatorChar;
      string str15 = directorySeparatorChar.ToString();
      string str16 = str1;
      using (FileStream fileStream = new FileStream(str14 + str15 + str16 + ".footer", FileMode.Create))
        fileStream.Write(this.footer, 0, this.footer.Length);
      this.fireDebug("Unpacking Wad Finished...");
    }

    private void parseWad(Stream wadFile)
    {
      this.fireDebug("Parsing Wad...");
      wadFile.Seek(0L, SeekOrigin.Begin);
      byte[] buffer = new byte[4];
      this.wadHeader = new WAD_Header();
      this.contents = new List<byte[]>();
      this.fireDebug("   Parsing Header... (Offset: 0x{0})", (object) wadFile.Position.ToString("x8").ToUpper());
      wadFile.Read(buffer, 0, 4);
      if ((int) Shared.Swap(BitConverter.ToUInt32(buffer, 0)) != (int) this.wadHeader.HeaderSize)
        throw new Exception("Invalid Headersize!");
      wadFile.Read(buffer, 0, 4);
      this.wadHeader.WadType = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      wadFile.Seek(12L, SeekOrigin.Current);
      wadFile.Read(buffer, 0, 4);
      this.wadHeader.TmdSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      wadFile.Read(buffer, 0, 4);
      this.wadHeader.ContentSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      wadFile.Read(buffer, 0, 4);
      this.wadHeader.FooterSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Parsing Certificate Chain... (Offset: 0x{0})", (object) wadFile.Position.ToString("x8").ToUpper());
      wadFile.Seek((long) Shared.AddPadding((int) wadFile.Position), SeekOrigin.Begin);
      byte[] numArray1 = new byte[(int) this.wadHeader.CertSize];
      wadFile.Read(numArray1, 0, numArray1.Length);
      this.cert.LoadFile(numArray1);
      this.fireDebug("   Parsing Ticket... (Offset: 0x{0})", (object) wadFile.Position.ToString("x8").ToUpper());
      wadFile.Seek((long) Shared.AddPadding((int) wadFile.Position), SeekOrigin.Begin);
      byte[] numArray2 = new byte[(int) this.wadHeader.TicketSize];
      wadFile.Read(numArray2, 0, numArray2.Length);
      this.tik.LoadFile(numArray2);
      this.fireDebug("   Parsing TMD... (Offset: 0x{0})", (object) wadFile.Position.ToString("x8").ToUpper());
      wadFile.Seek((long) Shared.AddPadding((int) wadFile.Position), SeekOrigin.Begin);
      byte[] numArray3 = new byte[(int) this.wadHeader.TmdSize];
      wadFile.Read(numArray3, 0, numArray3.Length);
      this.tmd.LoadFile(numArray3);
      if ((long) this.tmd.TitleID != (long) this.tik.TitleID)
        this.fireWarning("The Title ID in the Ticket doesn't match the one in the TMD!");
      long position;
      for (int contentIndex = 0; contentIndex < (int) this.tmd.NumOfContents; ++contentIndex)
      {
        this.fireProgress((contentIndex + 1) * 100 / (int) this.tmd.NumOfContents);
        object[] objArray1 = new object[3]
        {
          (object) (contentIndex + 1),
          (object) this.tmd.NumOfContents,
          null
        };
        position = wadFile.Position;
        objArray1[2] = (object) position.ToString("x8").ToUpper();
        this.fireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", objArray1);
        this.fireDebug("    -> Content ID: 0x{0}", (object) this.tmd.Contents[contentIndex].ContentID.ToString("x8"));
        object[] objArray2 = new object[1];
        ushort num = this.tmd.Contents[contentIndex].Index;
        objArray2[0] = (object) num.ToString("x4");
        this.fireDebug("    -> Index: 0x{0}", objArray2);
        object[] objArray3 = new object[2];
        num = (ushort) this.tmd.Contents[contentIndex].Type;
        objArray3[0] = (object) num.ToString("x4");
        objArray3[1] = (object) this.tmd.Contents[contentIndex].Type.ToString();
        this.fireDebug("    -> Type: 0x{0} ({1})", objArray3);
        this.fireDebug("    -> Size: {0} bytes", (object) this.tmd.Contents[contentIndex].Size);
        this.fireDebug("    -> Hash: {0}", (object) Shared.ByteArrayToString(this.tmd.Contents[contentIndex].Hash));
        wadFile.Seek((long) Shared.AddPadding((int) wadFile.Position), SeekOrigin.Begin);
        byte[] numArray4 = new byte[Shared.AddPadding((int) this.tmd.Contents[contentIndex].Size, 16)];
        wadFile.Read(numArray4, 0, numArray4.Length);
        byte[] array = this.decryptContent(numArray4, contentIndex);
        Array.Resize<byte>(ref array, (int) this.tmd.Contents[contentIndex].Size);
        if (!Shared.CompareByteArrays(this.tmd.Contents[contentIndex].Hash, this.sha.ComputeHash(array, 0, (int) this.tmd.Contents[contentIndex].Size)))
        {
          this.fireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
          // ISSUE: variable of a boxed type
          int local = contentIndex + 1;
          string str1 = this.tmd.Contents[contentIndex].ContentID.ToString("x8");
          num = this.tmd.Contents[contentIndex].Index;
          string str2 = num.ToString("x4");
          this.fireWarning(string.Format("Content #{0} (Content ID: 0x{1}; Index: 0x{2}): Hashes do not match! The content might be corrupted!", (object) local, (object) str1, (object) str2));
        }
        this.contents.Add(array);
        if (this.tmd.Contents[contentIndex].Index == (ushort) 0)
        {
          try
          {
            this.bannerApp.LoadFile(array);
            this.hasBanner = true;
          }
          catch
          {
            this.hasBanner = false;
          }
        }
      }
      if (this.wadHeader.FooterSize != 0U)
      {
        object[] objArray = new object[1];
        position = wadFile.Position;
        objArray[0] = (object) position.ToString("x8").ToUpper();
        this.fireDebug("   Reading Footer... (Offset: 0x{0})", objArray);
        this.footer = new byte[(int) this.wadHeader.FooterSize];
        wadFile.Seek((long) Shared.AddPadding((int) wadFile.Position), SeekOrigin.Begin);
        wadFile.Read(this.footer, 0, this.footer.Length);
        this.parseFooterTimestamp();
      }
      this.fireDebug("Parsing Wad Finished...");
    }

    private byte[] decryptContent(byte[] content, int contentIndex)
    {
      int length = content.Length;
      Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
      byte[] titleKey = this.tik.TitleKey;
      byte[] numArray = new byte[16];
      byte[] bytes = BitConverter.GetBytes(this.tmd.Contents[contentIndex].Index);
      numArray[0] = bytes[1];
      numArray[1] = bytes[0];
      RijndaelManaged rijndaelManaged = new RijndaelManaged();
      rijndaelManaged.Mode = CipherMode.CBC;
      rijndaelManaged.Padding = PaddingMode.None;
      rijndaelManaged.KeySize = 128;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Key = titleKey;
      rijndaelManaged.IV = numArray;
      ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor();
      MemoryStream memoryStream = new MemoryStream(content);
      CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read);
      byte[] buffer = new byte[length];
      cryptoStream.Read(buffer, 0, buffer.Length);
      cryptoStream.Dispose();
      memoryStream.Dispose();
      return buffer;
    }

    private byte[] encryptContent(byte[] content, int contentIndex)
    {
      Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
      byte[] titleKey = this.tik.TitleKey;
      byte[] numArray = new byte[16];
      byte[] bytes = BitConverter.GetBytes(this.tmd.Contents[contentIndex].Index);
      numArray[0] = bytes[1];
      numArray[1] = bytes[0];
      RijndaelManaged rijndaelManaged = new RijndaelManaged();
      rijndaelManaged.Mode = CipherMode.CBC;
      rijndaelManaged.Padding = PaddingMode.None;
      rijndaelManaged.KeySize = 128;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Key = titleKey;
      rijndaelManaged.IV = numArray;
      ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
      MemoryStream memoryStream = new MemoryStream(content);
      CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Read);
      byte[] buffer = new byte[content.Length];
      cryptoStream.Read(buffer, 0, buffer.Length);
      cryptoStream.Dispose();
      memoryStream.Dispose();
      return buffer;
    }

    private void createFooterTimestamp()
    {
      byte[] bytes = new ASCIIEncoding().GetBytes("TmStmp" + ((int) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString());
      Array.Resize<byte>(ref bytes, 64);
      this.wadHeader.FooterSize = (uint) bytes.Length;
      this.footer = bytes;
    }

    private void parseFooterTimestamp()
    {
      this.creationTimeUTC = new DateTime(1970, 1, 1);
      if ((this.footer[0] != (byte) 67 || this.footer[1] != (byte) 77 || (this.footer[2] != (byte) 105 || this.footer[3] != (byte) 105) || (this.footer[4] != (byte) 85 || this.footer[5] != (byte) 84)) && (this.footer[0] != (byte) 84 || this.footer[1] != (byte) 109 || (this.footer[2] != (byte) 83 || this.footer[3] != (byte) 116) || (this.footer[4] != (byte) 109 || this.footer[5] != (byte) 112)))
        return;
      string s = new ASCIIEncoding().GetString(this.footer, 6, 10);
      int num = 0;
      ref int local = ref num;
      if (!int.TryParse(s, out local))
        return;
      this.creationTimeUTC = this.creationTimeUTC.AddSeconds((double) num);
    }

    private void fireDebug(string debugMessage, params object[] args)
    {
      EventHandler<MessageEventArgs> debug = this.Debug;
      if (debug == null)
        return;
      debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
    }

    private void fireWarning(string warningMessage)
    {
      EventHandler<MessageEventArgs> warning = this.Warning;
      if (warning == null)
        return;
      warning(new object(), new MessageEventArgs(warningMessage));
    }

    private void fireProgress(int progressPercentage)
    {
      EventHandler<ProgressChangedEventArgs> progress = this.Progress;
      if (progress == null)
        return;
      progress(new object(), new ProgressChangedEventArgs(progressPercentage, (object) string.Empty));
    }

    private void cert_Debug(object sender, MessageEventArgs e) => this.fireDebug("      Certificate Chain: {0}", (object) e.Message);

    private void tik_Debug(object sender, MessageEventArgs e) => this.fireDebug("      Ticket: {0}", (object) e.Message);

    private void tmd_Debug(object sender, MessageEventArgs e) => this.fireDebug("      TMD: {0}", (object) e.Message);

    private void bannerApp_Debug(object sender, MessageEventArgs e) => this.fireDebug("      BannerApp: {0}", (object) e.Message);

    private void bannerApp_Warning(object sender, MessageEventArgs e) => this.fireWarning(e.Message);
  }

    public class WAD_Header
    {
        private uint headerSize = 0x20;
        private uint wadType = 0x49730000;
        private uint certSize = 0xA00;
        private uint reserved = 0x00;
        private uint tikSize = 0x2A4;
        private uint tmdSize;
        private uint contentSize;
        private uint footerSize = 0x00;

        public uint HeaderSize { get { return headerSize; } }
        public uint WadType { get { return wadType; } set { wadType = value; } }
        public uint CertSize { get { return certSize; } }
        public uint Reserved { get { return reserved; } }
        public uint TicketSize { get { return tikSize; } }
        public uint TmdSize { get { return tmdSize; } set { tmdSize = value; } }
        public uint ContentSize { get { return contentSize; } set { contentSize = value; } }
        public uint FooterSize { get { return footerSize; } set { footerSize = value; } }

        public void Write(Stream writeStream)
        {
            writeStream.Seek(0, SeekOrigin.Begin);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(wadType)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(certSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(reserved)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tikSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tmdSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(contentSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(footerSize)), 0, 4);
        }
    }
}
