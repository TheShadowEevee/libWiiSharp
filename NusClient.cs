// Decompiled with JetBrains decompiler
// Type: libWiiSharp.NusClient
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace libWiiSharp
{
  public class NusClient : IDisposable
  {
    private const string nusUrl = "http://nus.cdn.shop.wii.com/ccs/download/";
    private WebClient wcNus = new WebClient();
    private bool useLocalFiles;
    private bool continueWithoutTicket;
    private bool isDisposed;

    public bool UseLocalFiles
    {
      get => this.useLocalFiles;
      set => this.useLocalFiles = value;
    }

    public bool ContinueWithoutTicket
    {
      get => this.continueWithoutTicket;
      set => this.continueWithoutTicket = value;
    }

    public event EventHandler<ProgressChangedEventArgs> Progress;

    public event EventHandler<MessageEventArgs> Debug;

    ~NusClient() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
        this.wcNus.Dispose();
      this.isDisposed = true;
    }

    public void DownloadTitle(
      string titleId,
      string titleVersion,
      string outputDir,
      params StoreType[] storeTypes)
    {
      if (titleId.Length != 16)
        throw new Exception("Title ID must be 16 characters long!");
      this.downloadTitle(titleId, titleVersion, outputDir, storeTypes);
    }

    public TMD DownloadTMD(string titleId, string titleVersion) => titleId.Length == 16 ? this.downloadTmd(titleId, titleVersion) : throw new Exception("Title ID must be 16 characters long!");

    public Ticket DownloadTicket(string titleId) => titleId.Length == 16 ? this.downloadTicket(titleId) : throw new Exception("Title ID must be 16 characters long!");

    public byte[] DownloadSingleContent(string titleId, string titleVersion, string contentId)
    {
      if (titleId.Length != 16)
        throw new Exception("Title ID must be 16 characters long!");
      return this.downloadSingleContent(titleId, titleVersion, contentId);
    }

    public void DownloadSingleContent(
      string titleId,
      string titleVersion,
      string contentId,
      string savePath)
    {
      if (titleId.Length != 16)
        throw new Exception("Title ID must be 16 characters long!");
      if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
      if (System.IO.File.Exists(savePath))
        System.IO.File.Delete(savePath);
      byte[] bytes = this.downloadSingleContent(titleId, titleVersion, contentId);
      System.IO.File.WriteAllBytes(savePath, bytes);
    }

    private byte[] downloadSingleContent(string titleId, string titleVersion, string contentId)
    {
      uint num = uint.Parse(contentId, NumberStyles.HexNumber);
      contentId = num.ToString("x8");
      this.fireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2}...", (object) contentId, (object) titleId, string.IsNullOrEmpty(titleVersion) ? (object) "[Latest]" : (object) titleVersion);
      this.fireDebug("   Checking for Internet connection...");
      if (!this.CheckInet())
      {
        this.fireDebug("   Connection not found...");
        throw new Exception("You're not connected to the internet!");
      }
      this.fireProgress(0);
      string str1 = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion);
      string str2 = string.Format("{0}{1}/", (object) "http://nus.cdn.shop.wii.com/ccs/download/", (object) titleId);
      string empty = string.Empty;
      int contentIndex = 0;
      this.fireDebug("   Downloading TMD...");
      byte[] tmdFile = this.wcNus.DownloadData(str2 + str1);
      this.fireDebug("   Parsing TMD...");
      TMD tmd = TMD.Load(tmdFile);
      this.fireProgress(20);
      this.fireDebug("   Looking for Content ID {0} in TMD...", (object) contentId);
      bool flag = false;
      for (int index = 0; index < tmd.Contents.Length; ++index)
      {
        if ((int) tmd.Contents[index].ContentID == (int) num)
        {
          this.fireDebug("   Content ID {0} found in TMD...", (object) contentId);
          flag = true;
          empty = tmd.Contents[index].ContentID.ToString("x8");
          contentIndex = index;
          break;
        }
      }
      if (!flag)
      {
        this.fireDebug("   Content ID {0} wasn't found in TMD...", (object) contentId);
        throw new Exception("Content ID wasn't found in the TMD!");
      }
      if (!File.Exists("cetk"))
      {
        fireDebug("   Downloading Ticket...");
        byte[] tikArray = wcNus.DownloadData(str2 + "cetk");
        Console.WriteLine("Downloading");
      }
      Console.WriteLine("Continuing");
      this.fireDebug("Parsing Ticket...");
      Ticket tik = Ticket.Load("cetk");
      this.fireProgress(40);
      this.fireDebug("   Downloading Content... ({0} bytes)", (object) tmd.Contents[contentIndex].Size);
      byte[] content = this.wcNus.DownloadData(str2 + empty);
      this.fireProgress(80);
      this.fireDebug("   Decrypting Content...");
      byte[] array = this.decryptContent(content, contentIndex, tik, tmd);
      Array.Resize<byte>(ref array, (int) tmd.Contents[contentIndex].Size);
      if (!Shared.CompareByteArrays(SHA1.Create().ComputeHash(array), tmd.Contents[contentIndex].Hash))
      {
        this.fireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
        throw new Exception("Hashes do not match!");
      }
      this.fireProgress(100);
      this.fireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2} Finished...", (object) contentId, (object) titleId, string.IsNullOrEmpty(titleVersion) ? (object) "[Latest]" : (object) titleVersion);
      return array;
    }

    private Ticket downloadTicket(string titleId)
    {
        if (!CheckInet())
            throw new Exception("You're not connected to the internet!");

        string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
        byte[] tikArray = wcNus.DownloadData(titleUrl + "cetk");

        return Ticket.Load(tikArray);
    }

    private TMD downloadTmd(string titleId, string titleVersion)
    {
      if (!this.CheckInet())
        throw new Exception("You're not connected to the internet!");
      return TMD.Load(this.wcNus.DownloadData(string.Format("{0}{1}/", (object) "http://nus.cdn.shop.wii.com/ccs/download/", (object) titleId) + ("tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion))));
    }

    private void downloadTitle(
      string titleId,
      string titleVersion,
      string outputDir,
      StoreType[] storeTypes)
    {
      this.fireDebug("Downloading Title {0} v{1}...", (object) titleId, string.IsNullOrEmpty(titleVersion) ? (object) "[Latest]" : (object) titleVersion);
      if (storeTypes.Length < 1)
      {
        this.fireDebug("  No store types were defined...");
        throw new Exception("You must at least define one store type!");
      }
      string str1 = string.Format("{0}{1}/", (object) "http://nus.cdn.shop.wii.com/ccs/download/", (object) titleId);
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      this.fireProgress(0);
      for (int index = 0; index < storeTypes.Length; ++index)
      {
        switch (storeTypes[index])
        {
          case StoreType.EncryptedContent:
            this.fireDebug("    -> Storing Encrypted Content...");
            flag1 = true;
            break;
          case StoreType.DecryptedContent:
            this.fireDebug("    -> Storing Decrypted Content...");
            flag2 = true;
            break;
          case StoreType.WAD:
            this.fireDebug("    -> Storing WAD...");
            flag3 = true;
            break;
          case StoreType.All:
            this.fireDebug("    -> Storing Decrypted Content...");
            this.fireDebug("    -> Storing Encrypted Content...");
            this.fireDebug("    -> Storing WAD...");
            flag2 = true;
            flag1 = true;
            flag3 = true;
            break;
        }
      }
      this.fireDebug("   Checking for Internet connection...");
      if (!this.CheckInet())
      {
        this.fireDebug("   Connection not found...");
        throw new Exception("You're not connected to the internet!");
      }
      if ((int) outputDir[outputDir.Length - 1] != (int) Path.DirectorySeparatorChar)
        outputDir += Path.DirectorySeparatorChar.ToString();
      if (!Directory.Exists(outputDir))
        Directory.CreateDirectory(outputDir);
      string str2 = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion);
      this.fireDebug("   Downloading TMD...");
      try
      {
        this.wcNus.DownloadFile(str1 + str2, outputDir + str2);
      }
      catch (Exception ex)
      {
        this.fireDebug("   Downloading TMD Failed...");
        throw new Exception("Downloading TMD Failed:\n" + ex.Message);
      }

      if (!File.Exists(outputDir + "cetk"))
      {
        //Download cetk
        fireDebug("   Downloading Ticket...");
        try
        {
                    wcNus.DownloadFile(string.Format("{0}{1}/", nusUrl, titleId) + "cetk", outputDir + "cetk");
        }
        catch (Exception ex)
        {
            if (!continueWithoutTicket || !flag1)
            {
                fireDebug("   Downloading Ticket Failed...");
                throw new Exception("CETK Doesn't Exist and Downloading Ticket Failed:\n" + ex.Message);
            }

            flag2 = false;
            flag3 = false;
        }
      }


      this.fireProgress(10);
      this.fireDebug("   Parsing TMD...");
      TMD tmd = TMD.Load(outputDir + str2);
      if (string.IsNullOrEmpty(titleVersion))
        this.fireDebug("    -> Title Version: {0}", (object) tmd.TitleVersion);
      this.fireDebug("    -> {0} Contents", (object) tmd.NumOfContents);
      this.fireDebug("   Parsing Ticket...");
      Ticket tik = Ticket.Load(outputDir + "cetk");
      string[] strArray1 = new string[(int) tmd.NumOfContents];
      uint contentId;
      for (int index1 = 0; index1 < (int) tmd.NumOfContents; ++index1)
      {
        this.fireDebug("   Downloading Content #{0} of {1}... ({2} bytes)", (object) (index1 + 1), (object) tmd.NumOfContents, (object) tmd.Contents[index1].Size);
        this.fireProgress((index1 + 1) * 60 / (int) tmd.NumOfContents + 10);
        if (this.useLocalFiles)
        {
          string str3 = outputDir;
          contentId = tmd.Contents[index1].ContentID;
          string str4 = contentId.ToString("x8");
          if (System.IO.File.Exists(str3 + str4))
          {
            this.fireDebug("   Using Local File, Skipping...");
            continue;
          }
        }
        try
        {
          WebClient wcNus = this.wcNus;
          string str3 = str1;
          contentId = tmd.Contents[index1].ContentID;
          string str4 = contentId.ToString("x8");
          string address = str3 + str4;
          string str5 = outputDir;
          contentId = tmd.Contents[index1].ContentID;
          string str6 = contentId.ToString("x8");
          string fileName = str5 + str6;
          wcNus.DownloadFile(address, fileName);
          string[] strArray2 = strArray1;
          int index2 = index1;
          contentId = tmd.Contents[index1].ContentID;
          string str7 = contentId.ToString("x8");
          strArray2[index2] = str7;
        }
        catch (Exception ex)
        {
          this.fireDebug("   Downloading Content #{0} of {1} failed...", (object) (index1 + 1), (object) tmd.NumOfContents);
          throw new Exception("Downloading Content Failed:\n" + ex.Message);
        }
      }
      if (flag2 | flag3)
      {
        SHA1 shA1 = SHA1.Create();
        for (int contentIndex = 0; contentIndex < (int) tmd.NumOfContents; ++contentIndex)
        {
          this.fireDebug("   Decrypting Content #{0} of {1}...", (object) (contentIndex + 1), (object) tmd.NumOfContents);
          this.fireProgress((contentIndex + 1) * 20 / (int) tmd.NumOfContents + 75);
          string str3 = outputDir;
          contentId = tmd.Contents[contentIndex].ContentID;
          string str4 = contentId.ToString("x8");
          byte[] array = this.decryptContent(System.IO.File.ReadAllBytes(str3 + str4), contentIndex, tik, tmd);
          Array.Resize<byte>(ref array, (int) tmd.Contents[contentIndex].Size);
          if (!Shared.CompareByteArrays(shA1.ComputeHash(array), tmd.Contents[contentIndex].Hash))
          {
            this.fireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
            throw new Exception(string.Format("Content #{0}: Hashes do not match!", (object) contentIndex));
          }
          string str5 = outputDir;
          contentId = tmd.Contents[contentIndex].ContentID;
          string str6 = contentId.ToString("x8");
          System.IO.File.WriteAllBytes(str5 + str6 + ".app", array);
        }
        shA1.Clear();
      }
      if (flag3)
      {
        this.fireDebug("   Building Certificate Chain...");
        CertificateChain cert = CertificateChain.FromTikTmd(outputDir + "cetk", outputDir + str2);
        byte[][] contents = new byte[(int) tmd.NumOfContents][];
        for (int index1 = 0; index1 < (int) tmd.NumOfContents; ++index1)
        {
          byte[][] numArray1 = contents;
          int index2 = index1;
          string str3 = outputDir;
          contentId = tmd.Contents[index1].ContentID;
          string str4 = contentId.ToString("x8");
          byte[] numArray2 = System.IO.File.ReadAllBytes(str3 + str4 + ".app");
          numArray1[index2] = numArray2;
        }
        this.fireDebug("   Creating WAD...");
        WAD.Create(cert, tik, tmd, contents).Save(outputDir + tmd.TitleID.ToString("x16") + "v" + tmd.TitleVersion.ToString() + ".wad");
      }
      if (!flag1)
      {
        this.fireDebug("   Deleting Encrypted Contents...");
        for (int index = 0; index < strArray1.Length; ++index)
        {
          if (System.IO.File.Exists(outputDir + strArray1[index]))
            System.IO.File.Delete(outputDir + strArray1[index]);
        }
      }
      if (flag3 && !flag2)
      {
        this.fireDebug("   Deleting Decrypted Contents...");
        for (int index = 0; index < strArray1.Length; ++index)
        {
          if (System.IO.File.Exists(outputDir + strArray1[index] + ".app"))
            System.IO.File.Delete(outputDir + strArray1[index] + ".app");
        }
      }
      if (!flag2 && !flag1)
      {
        this.fireDebug("   Deleting TMD and Ticket...");
        System.IO.File.Delete(outputDir + str2);
        System.IO.File.Delete(outputDir + "cetk");
      }
      this.fireDebug("Downloading Title {0} v{1} Finished...", (object) titleId, string.IsNullOrEmpty(titleVersion) ? (object) "[Latest]" : (object) titleVersion);
      this.fireProgress(100);
    }

    private byte[] decryptContent(byte[] content, int contentIndex, Ticket tik, TMD tmd)
    {
      Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
      byte[] titleKey = tik.TitleKey;
      byte[] numArray = new byte[16];
      byte[] bytes = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
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
      byte[] buffer = new byte[content.Length];
      cryptoStream.Read(buffer, 0, buffer.Length);
      cryptoStream.Dispose();
      memoryStream.Dispose();
      return buffer;
    }

    private bool CheckInet()
    {
      try
      {
        Dns.GetHostEntry("www.google.com");
        return true;
      }
      catch
      {
        return false;
      }
    }

    private void fireDebug(string debugMessage, params object[] args)
    {
      EventHandler<MessageEventArgs> debug = this.Debug;
      if (debug == null)
        return;
      debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
    }

    private void fireProgress(int progressPercentage)
    {
      EventHandler<ProgressChangedEventArgs> progress = this.Progress;
      if (progress == null)
        return;
      progress(new object(), new ProgressChangedEventArgs(progressPercentage, (object) string.Empty));
    }
  }
}
