// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TMD
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
  public class TMD : IDisposable
  {
    private bool fakeSign;
    private bool sortContents;
    private uint signatureExponent = 65537;
    private byte[] signature = new byte[256];
    private byte[] padding = new byte[60];
    private byte[] issuer = new byte[64];
    private byte version;
    private byte caCrlVersion;
    private byte signerCrlVersion;
    private byte paddingByte;
    private ulong startupIos;
    private ulong titleId;
    private uint titleType;
    private ushort groupId;
    private ushort padding2;
    private ushort region;
    private byte[] reserved = new byte[58];
    private uint accessRights;
    private ushort titleVersion;
    private ushort numOfContents;
    private ushort bootIndex;
    private ushort padding3;
    private List<TMD_Content> contents;
    private bool isDisposed;

    public Region Region
    {
      get => (Region) this.region;
      set => this.region = (ushort) value;
    }

    public ulong StartupIOS
    {
      get => this.startupIos;
      set => this.startupIos = value;
    }

    public ulong TitleID
    {
      get => this.titleId;
      set => this.titleId = value;
    }

    public ushort TitleVersion
    {
      get => this.titleVersion;
      set => this.titleVersion = value;
    }

    public ushort NumOfContents => this.numOfContents;

    public ushort BootIndex
    {
      get => this.bootIndex;
      set
      {
        if ((int) value > (int) this.numOfContents)
          return;
        this.bootIndex = value;
      }
    }

    public TMD_Content[] Contents
    {
      get => this.contents.ToArray();
      set
      {
        this.contents = new List<TMD_Content>((IEnumerable<TMD_Content>) value);
        this.numOfContents = (ushort) value.Length;
      }
    }

    public bool FakeSign
    {
      get => this.fakeSign;
      set => this.fakeSign = value;
    }

    public bool SortContents
    {
      get => this.sortContents;
      set => this.sortContents = true;
    }

    public event EventHandler<MessageEventArgs> Debug;

    ~TMD() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.signature = (byte[]) null;
        this.padding = (byte[]) null;
        this.issuer = (byte[]) null;
        this.reserved = (byte[]) null;
        this.contents.Clear();
        this.contents = (List<TMD_Content>) null;
      }
      this.isDisposed = true;
    }

    public static TMD Load(string pathToTmd) => TMD.Load(File.ReadAllBytes(pathToTmd));

    public static TMD Load(byte[] tmdFile)
    {
      TMD tmd = new TMD();
      MemoryStream memoryStream = new MemoryStream(tmdFile);
      try
      {
        tmd.parseTmd((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
      return tmd;
    }

    public static TMD Load(Stream tmd)
    {
      TMD tmd1 = new TMD();
      tmd1.parseTmd(tmd);
      return tmd1;
    }

    public void LoadFile(string pathToTmd) => this.LoadFile(File.ReadAllBytes(pathToTmd));

    public void LoadFile(byte[] tmdFile)
    {
      MemoryStream memoryStream = new MemoryStream(tmdFile);
      try
      {
        this.parseTmd((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
    }

    public void LoadFile(Stream tmd) => this.parseTmd(tmd);

    public void Save(string savePath) => this.Save(savePath, false);

    public void Save(string savePath, bool fakeSign)
    {
      if (fakeSign)
        this.fakeSign = true;
      if (File.Exists(savePath))
        File.Delete(savePath);
      using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
        this.writeToStream((Stream) fileStream);
    }

    public MemoryStream ToMemoryStream() => this.ToMemoryStream(false);

    public MemoryStream ToMemoryStream(bool fakeSign)
    {
      if (fakeSign)
        this.fakeSign = true;
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

    public byte[] ToByteArray() => this.ToByteArray(false);

    public byte[] ToByteArray(bool fakeSign)
    {
      if (fakeSign)
        this.fakeSign = true;
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

    public void UpdateContents(string contentDir)
    {
      bool flag = true;
      char directorySeparatorChar;
      for (int index = 0; index < this.contents.Count; ++index)
      {
        string str1 = contentDir;
        directorySeparatorChar = Path.DirectorySeparatorChar;
        string str2 = directorySeparatorChar.ToString();
        string str3 = this.contents[index].ContentID.ToString("x8");
        if (!File.Exists(str1 + str2 + str3 + ".app"))
        {
          flag = false;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < this.contents.Count; ++index)
        {
          string str1 = contentDir;
          directorySeparatorChar = Path.DirectorySeparatorChar;
          string str2 = directorySeparatorChar.ToString();
          string str3 = this.contents[index].ContentID.ToString("x8");
          if (!File.Exists(str1 + str2 + str3 + ".app"))
            throw new Exception("Couldn't find all content files!");
        }
      }
      byte[][] conts = new byte[this.contents.Count][];
      for (int index = 0; index < this.contents.Count; ++index)
      {
        string str1 = contentDir;
        directorySeparatorChar = Path.DirectorySeparatorChar;
        string str2 = directorySeparatorChar.ToString();
        string str3 = flag ? this.contents[index].ContentID.ToString("x8") : this.contents[index].Index.ToString("x8");
        string path = str1 + str2 + str3 + ".app";
        conts[index] = File.ReadAllBytes(path);
      }
      this.updateContents(conts);
    }

    public void UpdateContents(byte[][] contents) => this.updateContents(contents);

    public string GetUpperTitleID()
    {
      byte[] bytes = BitConverter.GetBytes(Shared.Swap((uint) this.titleId));
      return new string(new char[4]
      {
        (char) bytes[0],
        (char) bytes[1],
        (char) bytes[2],
        (char) bytes[3]
      });
    }

    public string GetNandBlocks() => this.calculateNandBlocks();

    public void AddContent(TMD_Content content)
    {
      this.contents.Add(content);
      this.numOfContents = (ushort) this.contents.Count;
    }

    public void RemoveContent(int contentIndex)
    {
      for (int index = 0; index < (int) this.numOfContents; ++index)
      {
        if ((int) this.contents[index].Index == contentIndex)
        {
          this.contents.RemoveAt(index);
          break;
        }
      }
      this.numOfContents = (ushort) this.contents.Count;
    }

    public void RemoveContentByID(int contentId)
    {
      for (int index = 0; index < (int) this.numOfContents; ++index)
      {
        if ((long) this.contents[index].ContentID == (long) contentId)
        {
          this.contents.RemoveAt(index);
          break;
        }
      }
      this.numOfContents = (ushort) this.contents.Count;
    }

    public ContentIndices[] GetSortedContentList()
    {
      List<ContentIndices> contentIndicesList = new List<ContentIndices>();
      for (int index = 0; index < this.contents.Count; ++index)
        contentIndicesList.Add(new ContentIndices(index, (int) this.contents[index].Index));
      if (this.sortContents)
        contentIndicesList.Sort();
      return contentIndicesList.ToArray();
    }

    private void writeToStream(Stream writeStream)
    {
      this.fireDebug("Writing TMD...");
      if (this.fakeSign)
      {
        this.fireDebug("   Clearing Signature...");
        this.signature = new byte[256];
      }
      MemoryStream memoryStream = new MemoryStream();
      memoryStream.Seek(0L, SeekOrigin.Begin);
      this.fireDebug("   Writing Signature Exponent... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.signatureExponent)), 0, 4);
      this.fireDebug("   Writing Signature... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.signature, 0, this.signature.Length);
      this.fireDebug("   Writing Padding... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.padding, 0, this.padding.Length);
      this.fireDebug("   Writing Issuer... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.issuer, 0, this.issuer.Length);
      this.fireDebug("   Writing Version... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.version);
      this.fireDebug("   Writing CA Crl Version... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.caCrlVersion);
      this.fireDebug("   Writing Signer Crl Version... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.signerCrlVersion);
      this.fireDebug("   Writing Padding Byte... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.paddingByte);
      this.fireDebug("   Writing Startup IOS... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.startupIos)), 0, 8);
      this.fireDebug("   Writing Title ID... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.titleId)), 0, 8);
      this.fireDebug("   Writing Title Type... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.titleType)), 0, 4);
      this.fireDebug("   Writing Group ID... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.groupId)), 0, 2);
      this.fireDebug("   Writing Padding2... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.padding2)), 0, 2);
      this.fireDebug("   Writing Region... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.region)), 0, 2);
      this.fireDebug("   Writing Reserved... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.reserved, 0, this.reserved.Length);
      this.fireDebug("   Writing Access Rights... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.accessRights)), 0, 4);
      this.fireDebug("   Writing Title Version... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.titleVersion)), 0, 2);
      this.fireDebug("   Writing NumOfContents... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.numOfContents)), 0, 2);
      this.fireDebug("   Writing Boot Index... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.bootIndex)), 0, 2);
      this.fireDebug("   Writing Padding3... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.padding3)), 0, 2);
      List<ContentIndices> contentIndicesList = new List<ContentIndices>();
      for (int index = 0; index < this.contents.Count; ++index)
        contentIndicesList.Add(new ContentIndices(index, (int) this.contents[index].Index));
      if (this.sortContents)
        contentIndicesList.Sort();
      for (int index = 0; index < contentIndicesList.Count; ++index)
      {
        this.fireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper().ToUpper(), (object) (index + 1), (object) this.numOfContents);
        memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.contents[contentIndicesList[index].Index].ContentID)), 0, 4);
        memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.contents[contentIndicesList[index].Index].Index)), 0, 2);
        memoryStream.Write(BitConverter.GetBytes(Shared.Swap((ushort) this.contents[contentIndicesList[index].Index].Type)), 0, 2);
        memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.contents[contentIndicesList[index].Index].Size)), 0, 8);
        memoryStream.Write(this.contents[contentIndicesList[index].Index].Hash, 0, this.contents[contentIndicesList[index].Index].Hash.Length);
      }
      byte[] array = memoryStream.ToArray();
      memoryStream.Dispose();
      if (this.fakeSign)
      {
        this.fireDebug("   Fakesigning TMD...");
        byte[] numArray = new byte[20];
        SHA1 shA1 = SHA1.Create();
        for (ushort index = 0; index < ushort.MaxValue; ++index)
        {
          byte[] bytes = BitConverter.GetBytes(index);
          array[482] = bytes[1];
          array[483] = bytes[0];
          if (shA1.ComputeHash(array)[0] == (byte) 0)
          {
            this.fireDebug("   -> Signed ({0})", (object) index);
            break;
          }
          if (index == (ushort) 65534)
          {
            this.fireDebug("    -> Signing Failed...");
            throw new Exception("Fakesigning failed...");
          }
        }
        shA1.Clear();
      }
      writeStream.Seek(0L, SeekOrigin.Begin);
      writeStream.Write(array, 0, array.Length);
      this.fireDebug("Writing TMD Finished...");
    }

    private void updateContents(byte[][] conts)
    {
      SHA1 shA1 = SHA1.Create();
      for (int index = 0; index < this.contents.Count; ++index)
      {
        this.contents[index].Size = (ulong) conts[index].Length;
        this.contents[index].Hash = shA1.ComputeHash(conts[index]);
      }
      shA1.Clear();
    }

    private void parseTmd(Stream tmdFile)
    {
      this.fireDebug("Pasing TMD...");
      tmdFile.Seek(0L, SeekOrigin.Begin);
      byte[] buffer = new byte[8];
      this.fireDebug("   Reading Signature Exponent... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 4);
      this.signatureExponent = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Reading Signature... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(this.signature, 0, this.signature.Length);
      this.fireDebug("   Reading Padding... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(this.padding, 0, this.padding.Length);
      this.fireDebug("   Reading Issuer... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(this.issuer, 0, this.issuer.Length);
      this.fireDebug("   Reading Version... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading CA Crl Version... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading Signer Crl Version... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading Padding Byte... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 4);
      this.version = buffer[0];
      this.caCrlVersion = buffer[1];
      this.signerCrlVersion = buffer[2];
      this.paddingByte = buffer[3];
      this.fireDebug("   Reading Startup IOS... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 8);
      this.startupIos = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
      this.fireDebug("   Reading Title ID... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 8);
      this.titleId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
      this.fireDebug("   Reading Title Type... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 4);
      this.titleType = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Reading Group ID... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 2);
      this.groupId = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.fireDebug("   Reading Padding2... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 2);
      this.padding2 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.fireDebug("   Reading Region... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 2);
      this.region = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.fireDebug("   Reading Reserved... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(this.reserved, 0, this.reserved.Length);
      this.fireDebug("   Reading Access Rights... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 4);
      this.accessRights = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Reading Title Version... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading NumOfContents... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading Boot Index... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading Padding3... (Offset: 0x{0})", (object) tmdFile.Position.ToString("x8").ToUpper());
      tmdFile.Read(buffer, 0, 8);
      this.titleVersion = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.numOfContents = Shared.Swap(BitConverter.ToUInt16(buffer, 2));
      this.bootIndex = Shared.Swap(BitConverter.ToUInt16(buffer, 4));
      this.padding3 = Shared.Swap(BitConverter.ToUInt16(buffer, 6));
      this.contents = new List<TMD_Content>();
      for (int index = 0; index < (int) this.numOfContents; ++index)
      {
        this.fireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", (object) (index + 1), (object) this.numOfContents, (object) tmdFile.Position.ToString("x8").ToUpper().ToUpper());
        TMD_Content tmdContent = new TMD_Content();
        tmdContent.Hash = new byte[20];
        tmdFile.Read(buffer, 0, 8);
        tmdContent.ContentID = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
        tmdContent.Index = Shared.Swap(BitConverter.ToUInt16(buffer, 4));
        tmdContent.Type = (ContentType) Shared.Swap(BitConverter.ToUInt16(buffer, 6));
        tmdFile.Read(buffer, 0, 8);
        tmdContent.Size = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
        tmdFile.Read(tmdContent.Hash, 0, tmdContent.Hash.Length);
        this.contents.Add(tmdContent);
      }
      this.fireDebug("Pasing TMD Finished...");
    }

    private string calculateNandBlocks()
    {
      int num1 = 0;
      int num2 = 0;
      for (int index = 0; index < (int) this.numOfContents; ++index)
      {
        num2 += (int) this.contents[index].Size;
        if (this.contents[index].Type == ContentType.Normal)
          num1 += (int) this.contents[index].Size;
      }
      int num3 = (int) Math.Ceiling((double) num1 / 131072.0);
      int num4 = (int) Math.Ceiling((double) num2 / 131072.0);
      return num3 == num4 ? num4.ToString() : string.Format("{0} - {1}", (object) num3, (object) num4);
    }

    private void fireDebug(string debugMessage, params object[] args)
    {
      EventHandler<MessageEventArgs> debug = this.Debug;
      if (debug == null)
        return;
      debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
    }
  }
}
