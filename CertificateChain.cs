// Decompiled with JetBrains decompiler
// Type: libWiiSharp.CertificateChain
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
  public class CertificateChain : IDisposable
  {
    private const string certCaHash = "5B7D3EE28706AD8DA2CBD5A6B75C15D0F9B6F318";
    private const string certCpHash = "6824D6DA4C25184F0D6DAF6EDB9C0FC57522A41C";
    private const string certXsHash = "09787045037121477824BC6A3E5E076156573F8A";
    private SHA1 sha = SHA1.Create();
    private bool[] certsComplete = new bool[3];
    private byte[] certCa = new byte[1024];
    private byte[] certCp = new byte[768];
    private byte[] certXs = new byte[768];
    private bool isDisposed;

    public bool CertsComplete => this.certsComplete[0] && this.certsComplete[1] && this.certsComplete[2];

    public event EventHandler<MessageEventArgs> Debug;

    ~CertificateChain() => this.Dispose(false);

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
        this.certsComplete = (bool[]) null;
        this.certCa = (byte[]) null;
        this.certCp = (byte[]) null;
        this.certXs = (byte[]) null;
      }
      this.isDisposed = true;
    }

    public static CertificateChain Load(string pathToCert) => CertificateChain.Load(File.ReadAllBytes(pathToCert));

    public static CertificateChain Load(byte[] certFile)
    {
      CertificateChain certificateChain = new CertificateChain();
      MemoryStream memoryStream = new MemoryStream(certFile);
      try
      {
        certificateChain.parseCert((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
      return certificateChain;
    }

    public static CertificateChain Load(Stream cert)
    {
      CertificateChain certificateChain = new CertificateChain();
      certificateChain.parseCert(cert);
      return certificateChain;
    }

    public static CertificateChain FromTikTmd(string pathToTik, string pathToTmd) => CertificateChain.FromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));

    public static CertificateChain FromTikTmd(byte[] tikFile, byte[] tmdFile)
    {
      CertificateChain certificateChain = new CertificateChain();
      MemoryStream memoryStream1 = new MemoryStream(tikFile);
      try
      {
        certificateChain.grabFromTik((Stream) memoryStream1);
      }
      catch
      {
        memoryStream1.Dispose();
        throw;
      }
      MemoryStream memoryStream2 = new MemoryStream(tmdFile);
      try
      {
        certificateChain.grabFromTmd((Stream) memoryStream2);
      }
      catch
      {
        memoryStream2.Dispose();
        throw;
      }
      memoryStream2.Dispose();
      return certificateChain.CertsComplete ? certificateChain : throw new Exception("Couldn't locate all certs!");
    }

    public static CertificateChain FromTikTmd(Stream tik, Stream tmd)
    {
      CertificateChain certificateChain = new CertificateChain();
      certificateChain.grabFromTik(tik);
      certificateChain.grabFromTmd(tmd);
      return certificateChain;
    }

    public void LoadFile(string pathToCert) => this.LoadFile(File.ReadAllBytes(pathToCert));

    public void LoadFile(byte[] certFile)
    {
      MemoryStream memoryStream = new MemoryStream(certFile);
      try
      {
        this.parseCert((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
    }

    public void LoadFile(Stream cert) => this.parseCert(cert);

    public void LoadFromTikTmd(string pathToTik, string pathToTmd) => this.LoadFromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));

    public void LoadFromTikTmd(byte[] tikFile, byte[] tmdFile)
    {
      MemoryStream memoryStream1 = new MemoryStream(tikFile);
      try
      {
        this.grabFromTik((Stream) memoryStream1);
      }
      catch
      {
        memoryStream1.Dispose();
        throw;
      }
      MemoryStream memoryStream2 = new MemoryStream(tmdFile);
      try
      {
        this.grabFromTmd((Stream) memoryStream2);
      }
      catch
      {
        memoryStream2.Dispose();
        throw;
      }
      memoryStream2.Dispose();
      if (!this.CertsComplete)
        throw new Exception("Couldn't locate all certs!");
    }

    public void LoadFromTikTmd(Stream tik, Stream tmd)
    {
      this.grabFromTik(tik);
      this.grabFromTmd(tmd);
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

    private void writeToStream(Stream writeStream)
    {
      this.fireDebug("Writing Certificate Chain...");
      if (!this.CertsComplete)
      {
        this.fireDebug("   Certificate Chain incomplete...");
        throw new Exception("At least one certificate is missing!");
      }
      writeStream.Seek(0L, SeekOrigin.Begin);
      object[] objArray1 = new object[1];
      long position = writeStream.Position;
      objArray1[0] = (object) position.ToString("x8");
      this.fireDebug("   Writing Certificate CA... (Offset: 0x{0})", objArray1);
      writeStream.Write(this.certCa, 0, this.certCa.Length);
      object[] objArray2 = new object[1];
      position = writeStream.Position;
      objArray2[0] = (object) position.ToString("x8");
      this.fireDebug("   Writing Certificate CP... (Offset: 0x{0})", objArray2);
      writeStream.Write(this.certCp, 0, this.certCp.Length);
      object[] objArray3 = new object[1];
      position = writeStream.Position;
      objArray3[0] = (object) position.ToString("x8");
      this.fireDebug("   Writing Certificate XS... (Offset: 0x{0})", objArray3);
      writeStream.Write(this.certXs, 0, this.certXs.Length);
      this.fireDebug("Writing Certificate Chain Finished...");
    }

    private void parseCert(Stream certFile)
    {
      this.fireDebug("Parsing Certificate Chain...");
      int num = 0;
      for (int index = 0; index < 3; ++index)
      {
        this.fireDebug("   Scanning at Offset 0x{0}:", (object) num.ToString("x8"));
        try
        {
          certFile.Seek((long) num, SeekOrigin.Begin);
          byte[] array = new byte[1024];
          certFile.Read(array, 0, array.Length);
          this.fireDebug("   Checking for Certificate CA...");
          if (this.isCertCa(array) && !this.certsComplete[1])
          {
            this.fireDebug("   Certificate CA detected...");
            this.certCa = array;
            this.certsComplete[1] = true;
            num += 1024;
            continue;
          }
          this.fireDebug("   Checking for Certificate CP...");
          if (this.isCertCp(array) && !this.certsComplete[2])
          {
            this.fireDebug("   Certificate CP detected...");
            Array.Resize<byte>(ref array, 768);
            this.certCp = array;
            this.certsComplete[2] = true;
            num += 768;
            continue;
          }
          this.fireDebug("   Checking for Certificate XS...");
          if (this.isCertXs(array))
          {
            if (!this.certsComplete[0])
            {
              this.fireDebug("   Certificate XS detected...");
              Array.Resize<byte>(ref array, 768);
              this.certXs = array;
              this.certsComplete[0] = true;
              num += 768;
              continue;
            }
          }
        }
        catch (Exception ex)
        {
          this.fireDebug("Error: {0}", (object) ex.Message);
        }
        num += 768;
      }
      if (!this.CertsComplete)
      {
        this.fireDebug("   Couldn't locate all Certificates...");
        throw new Exception("Couldn't locate all certs!");
      }
      this.fireDebug("Parsing Certificate Chain Finished...");
    }

    private void grabFromTik(Stream tik)
    {
      this.fireDebug("Scanning Ticket for Certificates...");
      int num = 676;
      for (int index = 0; index < 3; ++index)
      {
        this.fireDebug("   Scanning at Offset 0x{0}:", (object) num.ToString("x8"));
        try
        {
          tik.Seek((long) num, SeekOrigin.Begin);
          byte[] array = new byte[1024];
          tik.Read(array, 0, array.Length);
          this.fireDebug("   Checking for Certificate CA...");
          if (this.isCertCa(array) && !this.certsComplete[1])
          {
            this.fireDebug("   Certificate CA detected...");
            this.certCa = array;
            this.certsComplete[1] = true;
            num += 1024;
            continue;
          }
          this.fireDebug("   Checking for Certificate CP...");
          if (this.isCertCp(array) && !this.certsComplete[2])
          {
            this.fireDebug("   Certificate CP detected...");
            Array.Resize<byte>(ref array, 768);
            this.certCp = array;
            this.certsComplete[2] = true;
            num += 768;
            continue;
          }
          this.fireDebug("   Checking for Certificate XS...");
          if (this.isCertXs(array))
          {
            if (!this.certsComplete[0])
            {
              this.fireDebug("   Certificate XS detected...");
              Array.Resize<byte>(ref array, 768);
              this.certXs = array;
              this.certsComplete[0] = true;
              num += 768;
              continue;
            }
          }
        }
        catch
        {
        }
        num += 768;
      }
      this.fireDebug("Scanning Ticket for Certificates Finished...");
    }

    private void grabFromTmd(Stream tmd)
    {
      this.fireDebug("Scanning TMD for Certificates...");
      byte[] buffer = new byte[2];
      tmd.Seek(478L, SeekOrigin.Begin);
      tmd.Read(buffer, 0, 2);
      int num = 484 + (int) Shared.Swap(BitConverter.ToUInt16(buffer, 0)) * 36;
      for (int index = 0; index < 3; ++index)
      {
        this.fireDebug("   Scanning at Offset 0x{0}:", (object) num.ToString("x8"));
        try
        {
          tmd.Seek((long) num, SeekOrigin.Begin);
          byte[] array = new byte[1024];
          tmd.Read(array, 0, array.Length);
          this.fireDebug("   Checking for Certificate CA...");
          if (this.isCertCa(array) && !this.certsComplete[1])
          {
            this.fireDebug("   Certificate CA detected...");
            this.certCa = array;
            this.certsComplete[1] = true;
            num += 1024;
            continue;
          }
          this.fireDebug("   Checking for Certificate CP...");
          if (this.isCertCp(array) && !this.certsComplete[2])
          {
            this.fireDebug("   Certificate CP detected...");
            Array.Resize<byte>(ref array, 768);
            this.certCp = array;
            this.certsComplete[2] = true;
            num += 768;
            continue;
          }
          this.fireDebug("   Checking for Certificate XS...");
          if (this.isCertXs(array))
          {
            if (!this.certsComplete[0])
            {
              this.fireDebug("   Certificate XS detected...");
              Array.Resize<byte>(ref array, 768);
              this.certXs = array;
              this.certsComplete[0] = true;
              num += 768;
              continue;
            }
          }
        }
        catch
        {
        }
        num += 768;
      }
      this.fireDebug("Scanning TMD for Certificates Finished...");
    }

    private bool isCertXs(byte[] part)
    {
      if (part.Length < 768)
        return false;
      if (part.Length > 768)
        Array.Resize<byte>(ref part, 768);
      return part[388] == (byte) 88 && part[389] == (byte) 83 && Shared.CompareByteArrays(this.sha.ComputeHash(part), Shared.HexStringToByteArray("09787045037121477824BC6A3E5E076156573F8A"));
    }

    private bool isCertCa(byte[] part)
    {
      if (part.Length < 1024)
        return false;
      if (part.Length > 1024)
        Array.Resize<byte>(ref part, 1024);
      return part[644] == (byte) 67 && part[645] == (byte) 65 && Shared.CompareByteArrays(this.sha.ComputeHash(part), Shared.HexStringToByteArray("5B7D3EE28706AD8DA2CBD5A6B75C15D0F9B6F318"));
    }

    private bool isCertCp(byte[] part)
    {
      if (part.Length < 768)
        return false;
      if (part.Length > 768)
        Array.Resize<byte>(ref part, 768);
      return part[388] == (byte) 67 && part[389] == (byte) 80 && Shared.CompareByteArrays(this.sha.ComputeHash(part), Shared.HexStringToByteArray("6824D6DA4C25184F0D6DAF6EDB9C0FC57522A41C"));
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
