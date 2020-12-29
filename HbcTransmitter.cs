// Decompiled with JetBrains decompiler
// Type: libWiiSharp.HbcTransmitter
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;

namespace libWiiSharp
{
  public class HbcTransmitter : IDisposable
  {
    private int blocksize = 4096;
    private int wiiloadMayor;
    private int wiiloadMinor = 5;
    private bool compress;
    private string ipAddress;
    private int port = 4299;
    private string lastErrorMessage = string.Empty;
    private Protocol protocol;
    private TcpClient tcpClient;
    private NetworkStream nwStream;
    private string lastError = string.Empty;
    private int transmittedLength;
    private int compressionRatio;
    private bool isDisposed;

    public int Blocksize
    {
      get => this.blocksize;
      set => this.blocksize = value;
    }

    public int WiiloadVersionMayor
    {
      get => this.wiiloadMayor;
      set => this.wiiloadMayor = value;
    }

    public int WiiloadVersionMinor
    {
      get => this.wiiloadMinor;
      set => this.wiiloadMinor = value;
    }

    public bool Compress
    {
      get => this.compress;
      set
      {
        if (this.protocol == Protocol.HAXX)
          return;
        this.compress = value;
      }
    }

    public string IpAddress
    {
      get => this.ipAddress;
      set => this.ipAddress = value;
    }

    public int Port
    {
      get => this.port;
      set => this.port = value;
    }

    public int TransmittedLength => this.transmittedLength;

    public int CompressionRatio => this.compressionRatio;

    public string LastError => this.lastError;

    public event EventHandler<ProgressChangedEventArgs> Progress;

    public event EventHandler<MessageEventArgs> Debug;

    public HbcTransmitter(Protocol protocol, string ipAddress)
    {
      this.protocol = protocol;
      this.ipAddress = ipAddress;
      this.wiiloadMinor = protocol == Protocol.HAXX ? 4 : 5;
      this.compress = protocol == Protocol.JODI;
    }

    ~HbcTransmitter() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.ipAddress = (string) null;
        this.lastErrorMessage = (string) null;
        this.lastError = (string) null;
        if (this.nwStream != null)
        {
          this.nwStream.Close();
          this.nwStream = (NetworkStream) null;
        }
        if (this.tcpClient != null)
        {
          this.tcpClient.Close();
          this.tcpClient = (TcpClient) null;
        }
      }
      this.isDisposed = true;
    }

    public bool TransmitFile(string pathToFile) => this.transmit(Path.GetFileName(pathToFile), File.ReadAllBytes(pathToFile));

    public bool TransmitFile(string fileName, byte[] fileData) => this.transmit(fileName, fileData);

    private bool transmit(string fileName, byte[] fileData)
    {
      this.fireDebug("Transmitting {0} to {1}:{2}...", (object) fileName, (object) this.ipAddress, (object) this.port);
      if (!Environment.OSVersion.ToString().ToLower().Contains("windows"))
        this.compress = false;
      if (fileName.ToLower().EndsWith(".zip"))
        this.compress = false;
      this.tcpClient = new TcpClient();
      byte[] buffer1 = new byte[4];
      this.fireDebug("   Connecting...");
      try
      {
        this.tcpClient.Connect(this.ipAddress, 4299);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Connection Failed:\n" + ex.Message);
        this.lastError = "Connection Failed:\n" + ex.Message;
        this.tcpClient.Close();
        return false;
      }
      this.nwStream = this.tcpClient.GetStream();
      this.fireDebug("   Sending Magic...");
      buffer1[0] = (byte) 72;
      buffer1[1] = (byte) 65;
      buffer1[2] = (byte) 88;
      buffer1[3] = (byte) 88;
      try
      {
        this.nwStream.Write(buffer1, 0, 4);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Error sending Magic:\n" + ex.Message);
        this.lastError = "Error sending Magic:\n" + ex.Message;
        this.nwStream.Close();
        this.tcpClient.Close();
        return false;
      }
      this.fireDebug("   Sending Version Info...");
      buffer1[0] = (byte) this.wiiloadMayor;
      buffer1[1] = (byte) this.wiiloadMinor;
      buffer1[2] = (byte) (fileName.Length + 2 >> 8 & (int) byte.MaxValue);
      buffer1[3] = (byte) (fileName.Length + 2 & (int) byte.MaxValue);
      try
      {
        this.nwStream.Write(buffer1, 0, 4);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Error sending Version Info:\n" + ex.Message);
        this.lastError = "Error sending Version Info:\n" + ex.Message;
        this.nwStream.Close();
        this.tcpClient.Close();
        return false;
      }
      byte[] buffer2;
      if (this.compress)
      {
        this.fireDebug("   Compressing File...");
        try
        {
          buffer2 = zlibWrapper.Compress(fileData);
        }
        catch
        {
          this.fireDebug("    -> Compression failed, continuing without compression...");
          this.compress = false;
          buffer2 = fileData;
          fileData = new byte[0];
        }
      }
      else
      {
        buffer2 = fileData;
        fileData = new byte[0];
      }
      this.fireDebug("   Sending Filesize...");
      buffer1[0] = (byte) (buffer2.Length >> 24 & (int) byte.MaxValue);
      buffer1[1] = (byte) (buffer2.Length >> 16 & (int) byte.MaxValue);
      buffer1[2] = (byte) (buffer2.Length >> 8 & (int) byte.MaxValue);
      buffer1[3] = (byte) (buffer2.Length & (int) byte.MaxValue);
      try
      {
        this.nwStream.Write(buffer1, 0, 4);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Error sending Filesize:\n" + ex.Message);
        this.lastError = "Error sending Filesize:\n" + ex.Message;
        this.nwStream.Close();
        this.tcpClient.Close();
        return false;
      }
      if (this.protocol != Protocol.HAXX)
      {
        buffer1[0] = (byte) (fileData.Length >> 24 & (int) byte.MaxValue);
        buffer1[1] = (byte) (fileData.Length >> 16 & (int) byte.MaxValue);
        buffer1[2] = (byte) (fileData.Length >> 8 & (int) byte.MaxValue);
        buffer1[3] = (byte) (fileData.Length & (int) byte.MaxValue);
        try
        {
          this.nwStream.Write(buffer1, 0, 4);
        }
        catch (Exception ex)
        {
          this.fireDebug("    -> Error sending Filesize:\n" + ex.Message);
          this.lastError = "Error sending Filesize:\n" + ex.Message;
          this.nwStream.Close();
          this.tcpClient.Close();
          return false;
        }
      }
      this.fireDebug("   Sending File...");
      int offset = 0;
      int num1 = 0;
      int num2 = buffer2.Length / this.Blocksize;
      int num3 = buffer2.Length % this.Blocksize;
      try
      {
        do
        {
          this.fireProgress(++num1 * 100 / num2);
          this.nwStream.Write(buffer2, offset, this.Blocksize);
          offset += this.Blocksize;
        }
        while (num1 < num2);
        if (num3 > 0)
          this.nwStream.Write(buffer2, offset, buffer2.Length - offset);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Error sending File:\n" + ex.Message);
        this.lastError = "Error sending File:\n" + ex.Message;
        this.nwStream.Close();
        this.tcpClient.Close();
        return false;
      }
      this.fireDebug("   Sending Arguments...");
      byte[] buffer3 = new byte[fileName.Length + 2];
      for (int index = 0; index < fileName.Length; ++index)
        buffer3[index] = (byte) fileName.ToCharArray()[index];
      try
      {
        this.nwStream.Write(buffer3, 0, buffer3.Length);
      }
      catch (Exception ex)
      {
        this.fireDebug("    -> Error sending Arguments:\n" + ex.Message);
        this.lastError = "Error sending Arguments:\n" + ex.Message;
        this.nwStream.Close();
        this.tcpClient.Close();
        return false;
      }
      this.nwStream.Close();
      this.tcpClient.Close();
      this.transmittedLength = buffer2.Length;
      this.compressionRatio = !this.compress || fileData.Length == 0 ? 0 : buffer2.Length * 100 / fileData.Length;
      this.fireDebug("Transmitting {0} to {1}:{2} Finished...", (object) fileName, (object) this.ipAddress, (object) this.port);
      return true;
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
