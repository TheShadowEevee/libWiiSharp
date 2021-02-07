// Decompiled with JetBrains decompiler
// Type: libWiiSharp.Ticket
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{

    public enum CommonKeyType : byte
    {
        Standard = 0x00,
        Korean = 0x01,
    }

    public class Ticket : IDisposable
    {
    private byte newKeyIndex;
    private byte[] decryptedTitleKey = new byte[16];
    private bool fakeSign;
    private bool titleKeyChanged;
    private byte[] newEncryptedTitleKey = new byte[0];
    private bool reDecrypt;
    private uint signatureExponent = 65537;
    private byte[] signature = new byte[256];
    private byte[] padding = new byte[60];
    private byte[] issuer = new byte[64];
    private byte[] unknown = new byte[63];
    private byte[] encryptedTitleKey = new byte[16];
    private byte unknown2;
    private ulong ticketId;
    private uint consoleId;
    private ulong titleId;
    private ushort unknown3 = ushort.MaxValue;
    private ushort numOfDlc;
    private ulong unknown4;
    private byte padding2;
    private byte commonKeyIndex;
    private byte[] unknown5 = new byte[48];
    private byte[] unknown6 = new byte[32];
    private ushort padding3;
    private uint enableTimeLimit;
    private uint timeLimit;
    private byte[] padding4 = new byte[88];
    private bool isDisposed;

    public byte[] TitleKey
    {
      get => this.decryptedTitleKey;
      set
      {
        this.decryptedTitleKey = value;
        this.titleKeyChanged = true;
        this.reDecrypt = false;
      }
    }

    public CommonKeyType CommonKeyIndex
    {
      get => (CommonKeyType) this.newKeyIndex;
      set => this.newKeyIndex = (byte) value;
    }

    public ulong TicketID
    {
      get => this.ticketId;
      set => this.ticketId = value;
    }

    public uint ConsoleID
    {
      get => this.consoleId;
      set => this.consoleId = value;
    }

    public ulong TitleID
    {
      get => this.titleId;
      set
      {
        this.titleId = value;
        if (!this.reDecrypt)
          return;
        this.reDecryptTitleKey();
      }
    }

    public ushort NumOfDLC
    {
      get => this.numOfDlc;
      set => this.numOfDlc = value;
    }

    public bool FakeSign
    {
      get => this.fakeSign;
      set => this.fakeSign = value;
    }

    public bool TitleKeyChanged => this.titleKeyChanged;

    public event EventHandler<MessageEventArgs> Debug;

    ~Ticket() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.decryptedTitleKey = (byte[]) null;
        this.newEncryptedTitleKey = (byte[]) null;
        this.signature = (byte[]) null;
        this.padding = (byte[]) null;
        this.issuer = (byte[]) null;
        this.unknown = (byte[]) null;
        this.encryptedTitleKey = (byte[]) null;
        this.unknown5 = (byte[]) null;
        this.unknown6 = (byte[]) null;
        this.padding4 = (byte[]) null;
      }
      this.isDisposed = true;
    }

    public static Ticket Load(string pathToTicket) => Ticket.Load(File.ReadAllBytes(pathToTicket));

    public static Ticket Load(byte[] ticket)
    {
      Ticket ticket1 = new Ticket();
      MemoryStream memoryStream = new MemoryStream(ticket);
      try
      {
        ticket1.parseTicket((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
      return ticket1;
    }

    public static Ticket Load(Stream ticket)
    {
      Ticket ticket1 = new Ticket();
      ticket1.parseTicket(ticket);
      return ticket1;
    }

    public void LoadFile(string pathToTicket) => this.LoadFile(File.ReadAllBytes(pathToTicket));

    public void LoadFile(byte[] ticket)
    {
      MemoryStream memoryStream = new MemoryStream(ticket);
      try
      {
        this.parseTicket((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
    }

    public void LoadFile(Stream ticket) => this.parseTicket(ticket);

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

    public void SetTitleKey(string newTitleKey) => this.SetTitleKey(newTitleKey.ToCharArray());

    public void SetTitleKey(char[] newTitleKey)
    {
      if (newTitleKey.Length != 16)
        throw new Exception("The title key must be 16 characters long!");
      for (int index = 0; index < 16; ++index)
        this.encryptedTitleKey[index] = (byte) newTitleKey[index];
      this.decryptTitleKey();
      this.titleKeyChanged = true;
      this.reDecrypt = true;
      this.newEncryptedTitleKey = this.encryptedTitleKey;
    }

    public void SetTitleKey(byte[] newTitleKey)
    {
      this.encryptedTitleKey = newTitleKey.Length == 16 ? newTitleKey : throw new Exception("The title key must be 16 characters long!");
      this.decryptTitleKey();
      this.titleKeyChanged = true;
      this.reDecrypt = true;
      this.newEncryptedTitleKey = newTitleKey;
    }

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

    private void writeToStream(Stream writeStream)
    {
      this.fireDebug("Writing Ticket...");
      this.fireDebug("   Encrypting Title Key...");
      this.encryptTitleKey();
      this.fireDebug("    -> Decrypted Title Key: {0}", (object) Shared.ByteArrayToString(this.decryptedTitleKey));
      this.fireDebug("    -> Encrypted Title Key: {0}", (object) Shared.ByteArrayToString(this.encryptedTitleKey));
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
      this.fireDebug("   Writing Unknown... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.unknown, 0, this.unknown.Length);
      this.fireDebug("   Writing Title Key... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(this.encryptedTitleKey, 0, this.encryptedTitleKey.Length);
      this.fireDebug("   Writing Unknown2... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.unknown2);
      this.fireDebug("   Writing Ticket ID... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.ticketId)), 0, 8);
      this.fireDebug("   Writing Console ID... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.consoleId)), 0, 4);
      this.fireDebug("   Writing Title ID... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.titleId)), 0, 8);
      this.fireDebug("   Writing Unknwon3... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.unknown3)), 0, 2);
      this.fireDebug("   Writing NumOfDLC... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.numOfDlc)), 0, 2);
      this.fireDebug("   Writing Unknwon4... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.unknown4)), 0, 8);
      this.fireDebug("   Writing Padding2... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.padding2);
      this.fireDebug("   Writing Common Key Index... (Offset: 0x{0})", (object) memoryStream.Position.ToString("x8").ToUpper());
      memoryStream.WriteByte(this.commonKeyIndex);
      object[] objArray1 = new object[1];
      long position = memoryStream.Position;
      objArray1[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Unknown5... (Offset: 0x{0})", objArray1);
      memoryStream.Write(this.unknown5, 0, this.unknown5.Length);
      object[] objArray2 = new object[1];
      position = memoryStream.Position;
      objArray2[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Unknown6... (Offset: 0x{0})", objArray2);
      memoryStream.Write(this.unknown6, 0, this.unknown6.Length);
      object[] objArray3 = new object[1];
      position = memoryStream.Position;
      objArray3[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Padding3... (Offset: 0x{0})", objArray3);
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.padding3)), 0, 2);
      object[] objArray4 = new object[1];
      position = memoryStream.Position;
      objArray4[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Enable Time Limit... (Offset: 0x{0})", objArray4);
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.enableTimeLimit)), 0, 4);
      object[] objArray5 = new object[1];
      position = memoryStream.Position;
      objArray5[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Time Limit... (Offset: 0x{0})", objArray5);
      memoryStream.Write(BitConverter.GetBytes(Shared.Swap(this.timeLimit)), 0, 4);
      object[] objArray6 = new object[1];
      position = memoryStream.Position;
      objArray6[0] = (object) position.ToString("x8").ToUpper();
      this.fireDebug("   Writing Padding4... (Offset: 0x{0})", objArray6);
      memoryStream.Write(this.padding4, 0, this.padding4.Length);
      byte[] array = memoryStream.ToArray();
      memoryStream.Dispose();
      if (this.fakeSign)
      {
        this.fireDebug("   Fakesigning Ticket...");
        byte[] numArray = new byte[20];
        SHA1 shA1 = SHA1.Create();
        for (ushort index = 0; index < ushort.MaxValue; ++index)
        {
          byte[] bytes = BitConverter.GetBytes(index);
          array[498] = bytes[1];
          array[499] = bytes[0];
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
      this.fireDebug("Writing Ticket Finished...");
    }

    private void parseTicket(Stream ticketFile)
    {
      this.fireDebug("Parsing Ticket...");
      ticketFile.Seek(0L, SeekOrigin.Begin);
      byte[] buffer = new byte[8];
      this.fireDebug("   Reading Signature Exponent... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 4);
      this.signatureExponent = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Reading Signature... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.signature, 0, this.signature.Length);
      this.fireDebug("   Reading Padding... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.padding, 0, this.padding.Length);
      this.fireDebug("   Reading Issuer... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.issuer, 0, this.issuer.Length);
      this.fireDebug("   Reading Unknown... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.unknown, 0, this.unknown.Length);
      this.fireDebug("   Reading Title Key... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.encryptedTitleKey, 0, this.encryptedTitleKey.Length);
      this.fireDebug("   Reading Unknown2... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      this.unknown2 = (byte) ticketFile.ReadByte();
      this.fireDebug("   Reading Ticket ID.. (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 8);
      this.ticketId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
      this.fireDebug("   Reading Console ID... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 4);
      this.consoleId = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.fireDebug("   Reading Title ID... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 8);
      this.titleId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
      this.fireDebug("   Reading Unknown3... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading NumOfDLC... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 4);
      this.unknown3 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.numOfDlc = Shared.Swap(BitConverter.ToUInt16(buffer, 2));
      this.fireDebug("   Reading Unknown4... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 8);
      this.unknown4 = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
      this.fireDebug("   Reading Padding2... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      this.padding2 = (byte) ticketFile.ReadByte();
      this.fireDebug("   Reading Common Key Index... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      this.commonKeyIndex = (byte) ticketFile.ReadByte();
      this.newKeyIndex = this.commonKeyIndex;
      this.fireDebug("   Reading Unknown5... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.unknown5, 0, this.unknown5.Length);
      this.fireDebug("   Reading Unknown6... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.unknown6, 0, this.unknown6.Length);
      this.fireDebug("   Reading Padding3... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 2);
      this.padding3 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
      this.fireDebug("   Reading Enable Time Limit... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      this.fireDebug("   Reading Time Limit... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(buffer, 0, 8);
      this.enableTimeLimit = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
      this.timeLimit = Shared.Swap(BitConverter.ToUInt32(buffer, 4));
      this.fireDebug("   Reading Padding4... (Offset: 0x{0})", (object) ticketFile.Position.ToString("x8").ToUpper());
      ticketFile.Read(this.padding4, 0, this.padding4.Length);
      this.fireDebug("   Decrypting Title Key...");
      this.decryptTitleKey();
      this.fireDebug("    -> Encrypted Title Key: {0}", (object) Shared.ByteArrayToString(this.encryptedTitleKey));
      this.fireDebug("    -> Decrypted Title Key: {0}", (object) Shared.ByteArrayToString(this.decryptedTitleKey));
      this.fireDebug("Parsing Ticket Finished...");
    }

    private void decryptTitleKey()
    {
      byte[] numArray = this.commonKeyIndex == (byte) 1 ? CommonKey.GetKoreanKey() : CommonKey.GetStandardKey();
      byte[] bytes = BitConverter.GetBytes(Shared.Swap(this.titleId));
      Array.Resize<byte>(ref bytes, 16);
      RijndaelManaged rijndaelManaged = new RijndaelManaged();
      rijndaelManaged.Mode = CipherMode.CBC;
      rijndaelManaged.Padding = PaddingMode.None;
      rijndaelManaged.KeySize = 128;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Key = numArray;
      rijndaelManaged.IV = bytes;
      ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor();
      MemoryStream memoryStream = new MemoryStream(this.encryptedTitleKey);
      CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read);
      cryptoStream.Read(this.decryptedTitleKey, 0, this.decryptedTitleKey.Length);
      cryptoStream.Dispose();
      memoryStream.Dispose();
      decryptor.Dispose();
      rijndaelManaged.Clear();
    }

    private void encryptTitleKey()
    {
      this.commonKeyIndex = this.newKeyIndex;
      byte[] numArray = this.commonKeyIndex == (byte) 1 ? CommonKey.GetKoreanKey() : CommonKey.GetStandardKey();
      byte[] bytes = BitConverter.GetBytes(Shared.Swap(this.titleId));
      Array.Resize<byte>(ref bytes, 16);
      RijndaelManaged rijndaelManaged = new RijndaelManaged();
      rijndaelManaged.Mode = CipherMode.CBC;
      rijndaelManaged.Padding = PaddingMode.None;
      rijndaelManaged.KeySize = 128;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Key = numArray;
      rijndaelManaged.IV = bytes;
      ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
      MemoryStream memoryStream = new MemoryStream(this.decryptedTitleKey);
      CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Read);
      cryptoStream.Read(this.encryptedTitleKey, 0, this.encryptedTitleKey.Length);
      cryptoStream.Dispose();
      memoryStream.Dispose();
      encryptor.Dispose();
      rijndaelManaged.Clear();
    }

    private void reDecryptTitleKey()
    {
      this.encryptedTitleKey = this.newEncryptedTitleKey;
      this.decryptTitleKey();
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
