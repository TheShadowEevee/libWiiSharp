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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public enum StoreType
    {
        EncryptedContent,
        DecryptedContent,
        WAD,
        All,
    }

    public class NusClient : IDisposable
    {
        private const string nusUrl = "http://ccs.cdn.wup.shop.nintendo.net/ccs/download/";
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        private readonly WebClient wcNus = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        private bool useLocalFiles;
        private bool continueWithoutTicket;
        private bool isDisposed;

        /// <summary>
        /// If true, existing local files will be used.
        /// </summary>
        public bool UseLocalFiles
        {
            get => useLocalFiles;
            set => useLocalFiles = value;
        }

        /// <summary>
        /// If true, the download will be continued even if no ticket for the title is avaiable (WAD packaging and decryption are disabled).
        /// </summary>
        public bool ContinueWithoutTicket
        {
            get => continueWithoutTicket;
            set => continueWithoutTicket = value;
        }

        public event EventHandler<ProgressChangedEventArgs> Progress;

        public event EventHandler<MessageEventArgs> Debug;

        ~NusClient() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                wcNus.Dispose();
            }

            isDisposed = true;
        }

        public void DownloadTitle(
          string titleId,
          string titleVersion,
          string outputDir,
          params StoreType[] storeTypes)
        {
            if (titleId.Length != 16)
            {
                throw new Exception("Title ID must be 16 characters long!");
            }

            PrivDownloadTitle(titleId, titleVersion, outputDir, storeTypes);
        }

        public TMD DownloadTMD(string titleId, string titleVersion)
        {
            return titleId.Length == 16 ? PrivDownloadTmd(titleId, titleVersion) : throw new Exception("Title ID must be 16 characters long!");
        }

        public Ticket DownloadTicket(string titleId)
        {
            return titleId.Length == 16 ? PrivDownloadTicket(titleId) : throw new Exception("Title ID must be 16 characters long!");
        }

        public byte[] DownloadSingleContent(string titleId, string titleVersion, string contentId)
        {
            if (titleId.Length != 16)
            {
                throw new Exception("Title ID must be 16 characters long!");
            }

            return PrivDownloadSingleContent(titleId, titleVersion, contentId);
        }

        public void DownloadSingleContent(
          string titleId,
          string titleVersion,
          string contentId,
          string savePath)
        {
            if (titleId.Length != 16)
            {
                throw new Exception("Title ID must be 16 characters long!");
            }

            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            byte[] bytes = PrivDownloadSingleContent(titleId, titleVersion, contentId);
            File.WriteAllBytes(savePath, bytes);
        }

        private byte[] PrivDownloadSingleContent(string titleId, string titleVersion, string contentId)
        {
            uint num = uint.Parse(contentId, NumberStyles.HexNumber);
            contentId = num.ToString("x8");
            FireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2}...", contentId, titleId, string.IsNullOrEmpty(titleVersion) ? "[Latest]" : titleVersion);
            FireDebug("   Checking for Internet connection...");
            if (!PrivCheckInet())
            {
                FireDebug("   Connection not found...");
                throw new Exception("You're not connected to the internet!");
            }
            FireProgress(0);
            string str1 = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion);
            string str2 = string.Format("{0}{1}/", "http://nus.cdn.shop.wii.com/ccs/download/", titleId);
            string empty = string.Empty;
            int contentIndex = 0;
            FireDebug("   Downloading TMD...");
            byte[] tmdFile = wcNus.DownloadData(str2 + str1);
            FireDebug("   Parsing TMD...");
            TMD tmd = TMD.Load(tmdFile);
            FireProgress(20);
            FireDebug("   Looking for Content ID {0} in TMD...", (object)contentId);
            bool flag = false;
            for (int index = 0; index < tmd.Contents.Length; ++index)
            {
                if ((int)tmd.Contents[index].ContentID == (int)num)
                {
                    FireDebug("   Content ID {0} found in TMD...", (object)contentId);
                    flag = true;
                    empty = tmd.Contents[index].ContentID.ToString("x8");
                    contentIndex = index;
                    break;
                }
            }
            if (!flag)
            {
                FireDebug("   Content ID {0} wasn't found in TMD...", (object)contentId);
                throw new Exception("Content ID wasn't found in the TMD!");
            }
            if (!File.Exists("cetk") && !continueWithoutTicket)
            {
                FireDebug("   Downloading Ticket...");
                try
                {
                    byte[] tikArray = wcNus.DownloadData(str2 + "cetk");
                }
                catch(Exception ex)
                {
                    FireDebug("   Downloading Ticket Failed...");
                    throw new Exception("CETK Doesn't Exist and Downloading Ticket Failed:\n" + ex.Message);
                }
            }
            FireDebug("Parsing Ticket...");
            Ticket tik = Ticket.Load("cetk");
            FireProgress(40);
            FireDebug("   Downloading Content... ({0} bytes)", (object)tmd.Contents[contentIndex].Size);
            byte[] content = wcNus.DownloadData(str2 + empty);
            FireProgress(80);
            FireDebug("   Decrypting Content...");
            byte[] array = PrivDecryptContent(content, contentIndex, tik, tmd);
            Array.Resize<byte>(ref array, (int)tmd.Contents[contentIndex].Size);
            if (!Shared.CompareByteArrays(SHA1.Create().ComputeHash(array), tmd.Contents[contentIndex].Hash))
            {
                FireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
                throw new Exception("Hashes do not match!");
            }
            FireProgress(100);
            FireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2} Finished...", contentId, titleId, string.IsNullOrEmpty(titleVersion) ? "[Latest]" : titleVersion);
            return array;
        }

        private Ticket PrivDownloadTicket(string titleId)
        {
            if (!PrivCheckInet())
            {
                throw new Exception("You're not connected to the internet!");
            }

            string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
            byte[] tikArray = wcNus.DownloadData(titleUrl + "cetk");

            return Ticket.Load(tikArray);
        }

        private TMD PrivDownloadTmd(string titleId, string titleVersion)
        {
            if (!PrivCheckInet())
            {
                throw new Exception("You're not connected to the internet!");
            }

            return TMD.Load(wcNus.DownloadData(string.Format("{0}{1}/", "http://nus.cdn.shop.wii.com/ccs/download/", titleId) + ("tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion))));
        }

        private void PrivDownloadTitle(
          string titleId,
          string titleVersion,
          string outputDir,
          StoreType[] storeTypes)
        {
            FireDebug("Downloading Title {0} v{1}...", titleId, string.IsNullOrEmpty(titleVersion) ? "[Latest]" : titleVersion);
            if (storeTypes.Length < 1)
            {
                FireDebug("  No store types were defined...");
                throw new Exception("You must at least define one store type!");
            }
            string str1 = string.Format("{0}{1}/", "http://nus.cdn.shop.wii.com/ccs/download/", titleId);
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            FireProgress(0);
            for (int index = 0; index < storeTypes.Length; ++index)
            {
                switch (storeTypes[index])
                {
                    case StoreType.EncryptedContent:
                        FireDebug("    -> Storing Encrypted Content...");
                        flag1 = true;
                        break;
                    case StoreType.DecryptedContent:
                        FireDebug("    -> Storing Decrypted Content...");
                        flag2 = true;
                        break;
                    case StoreType.WAD:
                        FireDebug("    -> Storing WAD...");
                        flag3 = true;
                        break;
                    case StoreType.All:
                        FireDebug("    -> Storing Decrypted Content...");
                        FireDebug("    -> Storing Encrypted Content...");
                        FireDebug("    -> Storing WAD...");
                        flag2 = true;
                        flag1 = true;
                        flag3 = true;
                        break;
                }
            }
            if (ContinueWithoutTicket == true)
            {
                flag2 = false;
                flag1 = true;
                flag3 = false;
            }
            FireDebug("   Checking for Internet connection...");
            if (!PrivCheckInet())
            {
                FireDebug("   Connection not found...");
                throw new Exception("You're not connected to the internet!");
            }
            if (outputDir[outputDir.Length - 1] != Path.DirectorySeparatorChar)
            {
                outputDir += Path.DirectorySeparatorChar.ToString();
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string str2 = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : "." + titleVersion);
            FireDebug("   Downloading TMD...");
            try
            {
                wcNus.DownloadFile(str1 + str2, outputDir + str2);
            }
            catch (Exception ex)
            {
                FireDebug("   Downloading TMD Failed...");
                throw new Exception("Downloading TMD Failed:\n" + ex.Message);
            }

            
            if (!File.Exists(outputDir + "cetk"))
            {
                //Download cetk
                FireDebug("   Downloading Ticket...");
                try
                {
                    wcNus.DownloadFile(string.Format("{0}{1}/", nusUrl, titleId) + "cetk", outputDir + "cetk");
                }
                catch (Exception ex)
                {
                    if (!continueWithoutTicket || !flag1)
                    {
                        FireDebug("   Downloading Ticket Failed...");
                        throw new Exception("CETK Doesn't Exist and Downloading Ticket Failed:\n" + ex.Message);
                    }

                    flag2 = false;
                    flag3 = false;
                }
            }
            

            FireProgress(10);
            FireDebug("   Parsing TMD...");
            TMD tmd = TMD.Load(outputDir + str2);
            if (string.IsNullOrEmpty(titleVersion))
            {
                FireDebug("    -> Title Version: {0}", (object)tmd.TitleVersion);
            }

            FireDebug("    -> {0} Contents", (object)tmd.NumOfContents);
            FireDebug("   Parsing Ticket...");
            Ticket tik = null;
            if (!continueWithoutTicket) { tik = Ticket.Load(outputDir + "cetk"); }
            string[] strArray1 = new string[tmd.NumOfContents];
            uint contentId;
            for (int index1 = 0; index1 < tmd.NumOfContents; ++index1)
            {
                FireDebug("   Downloading Content #{0} of {1}... ({2} bytes)", index1 + 1, tmd.NumOfContents, tmd.Contents[index1].Size);
                FireProgress((index1 + 1) * 60 / tmd.NumOfContents + 10);
                if (useLocalFiles)
                {
                    string str3 = outputDir;
                    contentId = tmd.Contents[index1].ContentID;
                    string str4 = contentId.ToString("x8");
                    if (File.Exists(str3 + str4))
                    {
                        FireDebug("   Using Local File, Skipping...");
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
                    FireDebug("   Downloading Content #{0} of {1} failed...", index1 + 1, tmd.NumOfContents);
                    throw new Exception("Downloading Content Failed:\n" + ex.Message);
                }
            }
            if (flag2 | flag3)
            {
                SHA1 shA1 = SHA1.Create();
                for (int contentIndex = 0; contentIndex < tmd.NumOfContents; ++contentIndex)
                {
                    FireDebug("   Decrypting Content #{0} of {1}...", contentIndex + 1, tmd.NumOfContents);
                    FireProgress((contentIndex + 1) * 20 / tmd.NumOfContents + 75);
                    string str3 = outputDir;
                    contentId = tmd.Contents[contentIndex].ContentID;
                    string str4 = contentId.ToString("x8");
                    byte[] array = PrivDecryptContent(File.ReadAllBytes(str3 + str4), contentIndex, tik, tmd);
                    Array.Resize<byte>(ref array, (int)tmd.Contents[contentIndex].Size);
                    if (!Shared.CompareByteArrays(shA1.ComputeHash(array), tmd.Contents[contentIndex].Hash))
                    {
                        FireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
                        throw new Exception(string.Format("Content #{0}: Hashes do not match!", contentIndex));
                    }
                    string str5 = outputDir;
                    contentId = tmd.Contents[contentIndex].ContentID;
                    string str6 = contentId.ToString("x8");
                    File.WriteAllBytes(str5 + str6 + ".app", array);
                }
                shA1.Clear();
            }
            if (flag3)
            {
                FireDebug("   Building Certificate Chain...");
                CertificateChain cert = CertificateChain.FromTikTmd(outputDir + "cetk", outputDir + str2);
                byte[][] contents = new byte[tmd.NumOfContents][];
                for (int index1 = 0; index1 < tmd.NumOfContents; ++index1)
                {
                    byte[][] numArray1 = contents;
                    int index2 = index1;
                    string str3 = outputDir;
                    contentId = tmd.Contents[index1].ContentID;
                    string str4 = contentId.ToString("x8");
                    byte[] numArray2 = File.ReadAllBytes(str3 + str4 + ".app");
                    numArray1[index2] = numArray2;
                }
                FireDebug("   Creating WAD...");
                WAD.Create(cert, tik, tmd, contents).Save(outputDir + tmd.TitleID.ToString("x16") + "v" + tmd.TitleVersion.ToString() + ".wad");
            }
            if (!flag1)
            {
                FireDebug("   Deleting Encrypted Contents...");
                for (int index = 0; index < strArray1.Length; ++index)
                {
                    if (File.Exists(outputDir + strArray1[index]))
                    {
                        File.Delete(outputDir + strArray1[index]);
                    }
                }
            }
            if (flag3 && !flag2)
            {
                FireDebug("   Deleting Decrypted Contents...");
                for (int index = 0; index < strArray1.Length; ++index)
                {
                    if (File.Exists(outputDir + strArray1[index] + ".app"))
                    {
                        File.Delete(outputDir + strArray1[index] + ".app");
                    }
                }
            }
            if (!flag2 && !flag1)
            {
                FireDebug("   Deleting TMD and Ticket...");
                File.Delete(outputDir + str2);
                if (ContinueWithoutTicket == false) 
                {
                    File.Delete(outputDir + "cetk");
                }
            }
            FireDebug("Downloading Title {0} v{1} Finished...", titleId, string.IsNullOrEmpty(titleVersion) ? "[Latest]" : titleVersion);
            FireProgress(100);
        }

        private byte[] PrivDecryptContent(byte[] content, int contentIndex, Ticket tik, TMD tmd)
        {
            Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
            byte[] titleKey = tik.TitleKey;
            byte[] numArray = new byte[16];
            byte[] bytes = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
            numArray[0] = bytes[1];
            numArray[1] = bytes[0];
#pragma warning disable SYSLIB0022 // Type or member is obsolete
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = 128,
                Key = titleKey,
                IV = numArray
            };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
            ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor();
            MemoryStream memoryStream = new MemoryStream(content);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[content.Length];
            cryptoStream.Read(buffer, 0, buffer.Length);
            cryptoStream.Dispose();
            memoryStream.Dispose();
            return buffer;
        }

        private bool PrivCheckInet()
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

        private void FireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = Debug;
            if (debug == null)
            {
                return;
            }

            debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }

        private void FireProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = Progress;
            if (progress == null)
            {
                return;
            }

            progress(new object(), new ProgressChangedEventArgs(progressPercentage, string.Empty));
        }
    }
}
