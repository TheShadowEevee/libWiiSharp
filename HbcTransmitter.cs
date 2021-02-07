/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 Github Contributors
 * 
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace libWiiSharp
{
    public enum Protocol
    {
        /// <summary>
        /// Will preconfigure all settings for HBC to 1.0.5 (HAXX).
        /// </summary>
        HAXX = 0,
        /// <summary>
        /// Will preconfigure all settings for HBC from 1.0.5 (JODI).
        /// </summary>
        JODI = 1,
        /// <summary>
        /// Remember to define your custom settings.
        /// </summary>
        Custom = 2,
    }

    /// <summary>
    /// The HbcTransmitter can easily transmit files to the Homebrew Channel.
    /// In order to use compression, you need zlib1.dll in the application directory.
    /// </summary>
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

        /// <summary>
        /// The size of the buffer that is used to transmit the data.
        /// Default is 4 * 1024. If you're facing problems (freezes while transmitting), try a higher size.
        /// </summary>
        public int Blocksize
        {
            get => this.blocksize;
            set => this.blocksize = value;
        }

        /// <summary>
        /// The mayor version of wiiload. You might need to change it for upcoming releases of the HBC.
        /// </summary>
        public int WiiloadVersionMayor
        {
              get => this.wiiloadMayor;
              set => this.wiiloadMayor = value;
        }

        /// <summary>
        /// The minor version of wiiload. You might need to change it for upcoming releases of the HBC.
        /// </summary>
        public int WiiloadVersionMinor
        {
            get => this.wiiloadMinor;
            set => this.wiiloadMinor = value;
        }

        /// <summary>
        /// If true, the data will be compressed before being transmitted. NOT available for Protocol.HAXX!
        /// Also, compression will only work if zlib1.dll is in the application folder.
        /// </summary>
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

        /// <summary>
        /// The IP address of the Wii.
        /// </summary>
        public string IpAddress
        {
            get => this.ipAddress;
            set => this.ipAddress = value;
        }

        /// The port used for the transmission.
        /// You don't need to touch this unless the port changes in future releases of the HBC.
        /// </summary>
        public int Port
        {
            get => this.port;
            set => this.port = value;
        }

        /// <summary>
        /// After a successfully completed transmission, this value holds the number of transmitted bytes.
        /// </summary>
        public int TransmittedLength => this.transmittedLength;

        /// <summary>
        /// After a successfully completed transmission, this value holds the compression ratio.
        /// Will be 0 if the data wasn't compressed.
        /// </summary>
        public int CompressionRatio => this.compressionRatio;

        /// <summary>
        /// Holds the last occured error message.
        /// </summary>
        public string LastError => this.lastError;

        public HbcTransmitter(Protocol protocol, string ipAddress)
        {
            this.protocol = protocol;
            this.ipAddress = ipAddress;
            this.wiiloadMinor = protocol == Protocol.HAXX ? 4 : 5;
            this.compress = protocol == Protocol.JODI;
        }

        #region IDisposable Members

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
        #endregion

        #region Public Functions
        public bool TransmitFile(string pathToFile) => this.transmit(Path.GetFileName(pathToFile), File.ReadAllBytes(pathToFile));

        public bool TransmitFile(string fileName, byte[] fileData) => this.transmit(fileName, fileData);
        #endregion

        #region Private Functions
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
        #endregion

        #region Events
        /// <summary>
        /// Fires the Progress of various operations
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> Progress;

        /// <summary>
        /// Fires debugging messages. You may write them into a log file or log textbox.
        /// </summary>
        public event EventHandler<MessageEventArgs> Debug;

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
        #endregion
    }

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
