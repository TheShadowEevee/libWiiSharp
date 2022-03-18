/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2022 TheShadowEevee, Github Contributors
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
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public class CertificateChain : IDisposable
    {
        //private const string certCaHash = "5B7D3EE28706AD8DA2CBD5A6B75C15D0F9B6F318";
        //private const string certCpHash = "6824D6DA4C25184F0D6DAF6EDB9C0FC57522A41C";
        //private const string certXsHash = "09787045037121477824BC6A3E5E076156573F8A";
        private SHA1 sha = SHA1.Create();
        private bool[] certsComplete = new bool[3];
        private byte[] certCa = new byte[1024];
        private byte[] certCp = new byte[768];
        private byte[] certXs = new byte[768];
        private bool isDisposed;

        /// <summary>
        /// If false, the Certificate Chain is not complete (i.e. at least one certificate is missing).
        /// </summary>
        public bool CertsComplete => certsComplete[0] && certsComplete[1] && certsComplete[2];

        #region IDisposable Members
        ~CertificateChain() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                sha.Clear();
                sha = null;
                certsComplete = null;
                certCa = null;
                certCp = null;
                certXs = null;
            }
            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="pathToCert"></param>
        /// <returns></returns>
        public static CertificateChain Load(string pathToCert)
        {
            return Load(File.ReadAllBytes(pathToCert));
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="certFile"></param>
        /// <returns></returns>
        public static CertificateChain Load(byte[] certFile)
        {
            CertificateChain certificateChain = new CertificateChain();
            MemoryStream memoryStream = new MemoryStream(certFile);
            try
            {
                certificateChain.ParseCert(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
            return certificateChain;
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static CertificateChain Load(Stream cert)
        {
            CertificateChain certificateChain = new CertificateChain();
            certificateChain.ParseCert(cert);
            return certificateChain;
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(string pathToTik, string pathToTmd)
        {
            return FromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tikFile"></param>
        /// <param name="tmdFile"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(byte[] tikFile, byte[] tmdFile)
        {
            CertificateChain certificateChain = new CertificateChain();
            MemoryStream memoryStream1 = new MemoryStream(tikFile);
            try
            {
                certificateChain.GrabFromTik(memoryStream1);
            }
            catch
            {
                memoryStream1.Dispose();
                throw;
            }
            MemoryStream memoryStream2 = new MemoryStream(tmdFile);
            try
            {
                certificateChain.GrabFromTmd(memoryStream2);
            }
            catch
            {
                memoryStream2.Dispose();
                throw;
            }
            memoryStream2.Dispose();
            return certificateChain.CertsComplete ? certificateChain : throw new Exception("Couldn't locate all certs!");
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(Stream tik, Stream tmd)
        {
            CertificateChain certificateChain = new CertificateChain();
            certificateChain.GrabFromTik(tik);
            certificateChain.GrabFromTmd(tmd);
            return certificateChain;
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="pathToCert"></param>
        public void LoadFile(string pathToCert)
        {
            LoadFile(File.ReadAllBytes(pathToCert));
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="certFile"></param>
        public void LoadFile(byte[] certFile)
        {
            MemoryStream memoryStream = new MemoryStream(certFile);
            try
            {
                ParseCert(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="cert"></param>
        public void LoadFile(Stream cert)
        {
            ParseCert(cert);
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public void LoadFromTikTmd(string pathToTik, string pathToTmd)
        {
            LoadFromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tikFile"></param>
        /// <param name="tmdFile"></param>
        public void LoadFromTikTmd(byte[] tikFile, byte[] tmdFile)
        {
            MemoryStream memoryStream1 = new MemoryStream(tikFile);
            try
            {
                GrabFromTik(memoryStream1);
            }
            catch
            {
                memoryStream1.Dispose();
                throw;
            }
            MemoryStream memoryStream2 = new MemoryStream(tmdFile);
            try
            {
                GrabFromTmd(memoryStream2);
            }
            catch
            {
                memoryStream2.Dispose();
                throw;
            }
            memoryStream2.Dispose();
            if (!CertsComplete)
            {
                throw new Exception("Couldn't locate all certs!");
            }
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        public void LoadFromTikTmd(Stream tik, Stream tmd)
        {
            GrabFromTik(tik);
            GrabFromTmd(tmd);
        }

        /// <summary>
        /// Saves the Certificate Chain.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using FileStream fileStream = new FileStream(savePath, FileMode.Create);
            WriteToStream(fileStream);
        }

        /// <summary>
        /// Returns the Certificate Chain as a memory stream.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToMemoryStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                WriteToStream(memoryStream);
                return memoryStream;
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Returns the Certificate Chain as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                WriteToStream(memoryStream);
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
        #endregion

        #region Private Functions
        private void WriteToStream(Stream writeStream)
        {
            FireDebug("Writing Certificate Chain...");
            if (!CertsComplete)
            {
                FireDebug("   Certificate Chain incomplete...");
                throw new Exception("At least one certificate is missing!");
            }
            writeStream.Seek(0L, SeekOrigin.Begin);
            object[] objArray1 = new object[1];
            long position = writeStream.Position;
            objArray1[0] = position.ToString("x8");
            FireDebug("   Writing Certificate CA... (Offset: 0x{0})", objArray1);
            writeStream.Write(certCa, 0, certCa.Length);
            object[] objArray2 = new object[1];
            position = writeStream.Position;
            objArray2[0] = position.ToString("x8");
            FireDebug("   Writing Certificate CP... (Offset: 0x{0})", objArray2);
            writeStream.Write(certCp, 0, certCp.Length);
            object[] objArray3 = new object[1];
            position = writeStream.Position;
            objArray3[0] = position.ToString("x8");
            FireDebug("   Writing Certificate XS... (Offset: 0x{0})", objArray3);
            writeStream.Write(certXs, 0, certXs.Length);
            FireDebug("Writing Certificate Chain Finished...");
        }

        private void ParseCert(Stream certFile)
        {
            FireDebug("Parsing Certificate Chain...");
            int num = 0;
            for (int index = 0; index < 3; ++index)
            {
                FireDebug("   Scanning at Offset 0x{0}:", (object)num.ToString("x8"));
                try
                {
                    certFile.Seek(num, SeekOrigin.Begin);
                    byte[] array = new byte[1024];
                    certFile.Read(array, 0, array.Length);
                    FireDebug("   Checking for Certificate CA...");
                    if (IsCertCa(array) && !certsComplete[1])
                    {
                        FireDebug("   Certificate CA detected...");
                        certCa = array;
                        certsComplete[1] = true;
                        num += 1024;
                        continue;
                    }
                    FireDebug("   Checking for Certificate CP...");
                    if (IsCertCp(array) && !certsComplete[2])
                    {
                        FireDebug("   Certificate CP detected...");
                        Array.Resize<byte>(ref array, 768);
                        certCp = array;
                        certsComplete[2] = true;
                        num += 768;
                        continue;
                    }
                    FireDebug("   Checking for Certificate XS...");
                    if (IsCertXs(array))
                    {
                        if (!certsComplete[0])
                        {
                            FireDebug("   Certificate XS detected...");
                            Array.Resize<byte>(ref array, 768);
                            certXs = array;
                            certsComplete[0] = true;
                            num += 768;
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    FireDebug("Error: {0}", (object)ex.Message);
                }
                num += 768;
            }
            if (!CertsComplete)
            {
                FireDebug("   Couldn't locate all Certificates...");
                throw new Exception("Couldn't locate all certs!");
            }
            FireDebug("Parsing Certificate Chain Finished...");
        }

        private void GrabFromTik(Stream tik)
        {
            FireDebug("Scanning Ticket for Certificates...");
            int num = 676;
            for (int index = 0; index < 3; ++index)
            {
                FireDebug("   Scanning at Offset 0x{0}:", (object)num.ToString("x8"));
                try
                {
                    tik.Seek(num, SeekOrigin.Begin);
                    byte[] array = new byte[1024];
                    tik.Read(array, 0, array.Length);
                    FireDebug("   Checking for Certificate CA...");
                    if (IsCertCa(array) && !certsComplete[1])
                    {
                        FireDebug("   Certificate CA detected...");
                        certCa = array;
                        certsComplete[1] = true;
                        num += 1024;
                        continue;
                    }
                    FireDebug("   Checking for Certificate CP...");
                    if (IsCertCp(array) && !certsComplete[2])
                    {
                        FireDebug("   Certificate CP detected...");
                        Array.Resize<byte>(ref array, 768);
                        certCp = array;
                        certsComplete[2] = true;
                        num += 768;
                        continue;
                    }
                    FireDebug("   Checking for Certificate XS...");
                    if (IsCertXs(array))
                    {
                        if (!certsComplete[0])
                        {
                            FireDebug("   Certificate XS detected...");
                            Array.Resize<byte>(ref array, 768);
                            certXs = array;
                            certsComplete[0] = true;
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
            FireDebug("Scanning Ticket for Certificates Finished...");
        }

        private void GrabFromTmd(Stream tmd)
        {
            FireDebug("Scanning TMD for Certificates...");
            byte[] buffer = new byte[2];
            tmd.Seek(478L, SeekOrigin.Begin);
            tmd.Read(buffer, 0, 2);
            int num = 484 + Shared.Swap(BitConverter.ToUInt16(buffer, 0)) * 36;
            for (int index = 0; index < 3; ++index)
            {
                FireDebug("   Scanning at Offset 0x{0}:", (object)num.ToString("x8"));
                try
                {
                    tmd.Seek(num, SeekOrigin.Begin);
                    byte[] array = new byte[1024];
                    tmd.Read(array, 0, array.Length);
                    FireDebug("   Checking for Certificate CA...");
                    if (IsCertCa(array) && !certsComplete[1])
                    {
                        FireDebug("   Certificate CA detected...");
                        certCa = array;
                        certsComplete[1] = true;
                        num += 1024;
                        continue;
                    }
                    FireDebug("   Checking for Certificate CP...");
                    if (IsCertCp(array) && !certsComplete[2])
                    {
                        FireDebug("   Certificate CP detected...");
                        Array.Resize<byte>(ref array, 768);
                        certCp = array;
                        certsComplete[2] = true;
                        num += 768;
                        continue;
                    }
                    FireDebug("   Checking for Certificate XS...");
                    if (IsCertXs(array))
                    {
                        if (!certsComplete[0])
                        {
                            FireDebug("   Certificate XS detected...");
                            Array.Resize<byte>(ref array, 768);
                            certXs = array;
                            certsComplete[0] = true;
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
            FireDebug("Scanning TMD for Certificates Finished...");
        }

        private bool IsCertXs(byte[] part)
        {
            if (part.Length < 768)
            {
                return false;
            }

            if (part.Length > 768)
            {
                Array.Resize<byte>(ref part, 768);
            }

            return part[388] == 88 && part[389] == 83 && Shared.CompareByteArrays(sha.ComputeHash(part), Shared.HexStringToByteArray("09787045037121477824BC6A3E5E076156573F8A"));
        }

        private bool IsCertCa(byte[] part)
        {
            if (part.Length < 1024)
            {
                return false;
            }

            if (part.Length > 1024)
            {
                Array.Resize<byte>(ref part, 1024);
            }

            return part[644] == 67 && part[645] == 65 && Shared.CompareByteArrays(sha.ComputeHash(part), Shared.HexStringToByteArray("5B7D3EE28706AD8DA2CBD5A6B75C15D0F9B6F318"));
        }

        private bool IsCertCp(byte[] part)
        {
            if (part.Length < 768)
            {
                return false;
            }

            if (part.Length > 768)
            {
                Array.Resize<byte>(ref part, 768);
            }

            return part[388] == 67 && part[389] == 80 && Shared.CompareByteArrays(sha.ComputeHash(part), Shared.HexStringToByteArray("6824D6DA4C25184F0D6DAF6EDB9C0FC57522A41C"));
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires debugging messages. You may write them into a log file or log textbox.
        /// </summary>
        public event EventHandler<MessageEventArgs> Debug;

        private void FireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = Debug;
            if (debug == null)
            {
                return;
            }

            debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }
        #endregion
    }
}
