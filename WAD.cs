/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2021 Github Contributors
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
            get => tmd.Region;
            set => tmd.Region = value;
        }

        public int NumOfContents => tmd.NumOfContents;

        public byte[][] Contents => contents.ToArray();

        public bool FakeSign
        {
            get => tik.FakeSign && tmd.FakeSign;
            set
            {
                tik.FakeSign = value;
                tmd.FakeSign = value;
            }
        }

        public U8 BannerApp
        {
            get => bannerApp;
            set => bannerApp = value;
        }

        public ulong StartupIOS
        {
            get => tmd.StartupIOS;
            set => tmd.StartupIOS = value;
        }

        public ulong TitleID
        {
            get => tik.TitleID;
            set
            {
                tik.TitleID = value;
                tmd.TitleID = value;
            }
        }

        public string UpperTitleID => tik.GetUpperTitleID();

        public ushort TitleVersion
        {
            get => tmd.TitleVersion;
            set => tmd.TitleVersion = value;
        }

        public ushort BootIndex
        {
            get => tmd.BootIndex;
            set => tmd.BootIndex = value;
        }

        public DateTime CreationTimeUTC => creationTimeUTC;

        public bool HasBanner => hasBanner;

        public bool Lz77CompressBannerAndIcon
        {
            get => lz77CompressBannerAndIcon;
            set
            {
                lz77CompressBannerAndIcon = value;
                if (!value)
                {
                    return;
                }

                lz77DecompressBannerAndIcon = false;
            }
        }

        public bool Lz77DecompressBannerAndIcon
        {
            get => lz77DecompressBannerAndIcon;
            set
            {
                lz77DecompressBannerAndIcon = value;
                if (!value)
                {
                    return;
                }

                lz77CompressBannerAndIcon = false;
            }
        }

        public string NandBlocks => tmd.GetNandBlocks();

        public string[] ChannelTitles
        {
            get => hasBanner ? ((Headers.IMET)bannerApp.Header).AllTitles : new string[0];
            set => ChangeChannelTitles(value);
        }

        public bool KeepOriginalFooter
        {
            get => keepOriginalFooter;
            set => keepOriginalFooter = value;
        }

        public TMD_Content[] TmdContents => tmd.Contents;

        public CommonKeyType CommonKeyType
        {
            get => tik.CommonKeyIndex;
            set => tik.CommonKeyIndex = value;
        }

        public bool SortContents
        {
            get => tmd.SortContents;
            set => tmd.SortContents = value;
        }

        public event EventHandler<ProgressChangedEventArgs> Progress;

        public event EventHandler<MessageEventArgs> Warning;

        public event EventHandler<MessageEventArgs> Debug;

        public WAD()
        {
            cert.Debug += new EventHandler<MessageEventArgs>(Cert_Debug);
            tik.Debug += new EventHandler<MessageEventArgs>(Tik_Debug);
            tmd.Debug += new EventHandler<MessageEventArgs>(Tmd_Debug);
            bannerApp.Debug += new EventHandler<MessageEventArgs>(BannerApp_Debug);
            bannerApp.Warning += new EventHandler<MessageEventArgs>(BannerApp_Warning);
        }

        ~WAD() => Dispose(false);

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
                wadHeader = null;
                cert.Dispose();
                tik.Dispose();
                tmd.Dispose();
                contents.Clear();
                contents = null;
                bannerApp.Dispose();
                footer = null;
            }
            isDisposed = true;
        }

        public static WAD Load(string pathToWad)
        {
            return WAD.Load(File.ReadAllBytes(pathToWad));
        }

        public static WAD Load(byte[] wadFile)
        {
            WAD wad = new WAD();
            MemoryStream memoryStream = new MemoryStream(wadFile);
            try
            {
                wad.ParseWad(memoryStream);
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
            wad1.ParseWad(wad);
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
                    {
                        throw new Exception("Couldn't find all content files!");
                    }
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
                    {
                        throw new Exception("Couldn't find all content files!");
                    }
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
                contents = new List<byte[]>(contents),
                wadHeader = new WAD_Header()
            };
            wad.wadHeader.TmdSize = (uint)(484 + tmd.Contents.Length * 36);
            int num1 = 0;
            for (int index = 0; index < contents.Length - 1; ++index)
            {
                num1 += Shared.AddPadding(contents[index].Length);
            }

            int num2 = num1 + contents[contents.Length - 1].Length;
            wad.wadHeader.ContentSize = (uint)num2;
            for (int index = 0; index < wad.tmd.Contents.Length; ++index)
            {
                if (wad.tmd.Contents[index].Index == 0)
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

        public void LoadFile(string pathToWad)
        {
            LoadFile(File.ReadAllBytes(pathToWad));
        }

        public void LoadFile(byte[] wadFile)
        {
            MemoryStream memoryStream = new MemoryStream(wadFile);
            try
            {
                ParseWad(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        public void LoadFile(Stream wad)
        {
            ParseWad(wad);
        }

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
                    {
                        throw new Exception("Couldn't find all content files!");
                    }
                }
            }
            byte[][] contents = new byte[tmd.Contents.Length][];
            for (int index = 0; index < tmd.Contents.Length; ++index)
            {
                string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
                contents[index] = File.ReadAllBytes(path);
            }
            CreateNew(cert, tik, tmd, contents);
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
                    {
                        throw new Exception("Couldn't find all content files!");
                    }
                }
            }
            byte[][] contents = new byte[tmd.Contents.Length][];
            for (int index = 0; index < tmd.Contents.Length; ++index)
            {
                string path = contentDir + Path.DirectorySeparatorChar.ToString() + (flag ? tmd.Contents[index].ContentID.ToString("x8") : tmd.Contents[index].Index.ToString("x8")) + ".app";
                contents[index] = File.ReadAllBytes(path);
            }
            CreateNew(cert, tik, tmd, contents);
        }

        public void CreateNew(byte[] cert, byte[] tik, byte[] tmd, byte[][] contents)
        {
            CreateNew(CertificateChain.Load(cert), Ticket.Load(tik), TMD.Load(tmd), contents);
        }

        public void CreateNew(CertificateChain cert, Ticket tik, TMD tmd, byte[][] contents)
        {
            this.cert = cert;
            this.tik = tik;
            this.tmd = tmd;
            this.contents = new List<byte[]>(contents);
            wadHeader = new WAD_Header
            {
                TmdSize = (uint)(484 + tmd.Contents.Length * 36)
            };
            int num = 0;
            for (int index = 0; index < contents.Length - 1; ++index)
            {
                num += Shared.AddPadding(contents[index].Length);
            }

            wadHeader.ContentSize = (uint)(num + contents[contents.Length - 1].Length);
            for (int index = 0; index < this.tmd.Contents.Length; ++index)
            {
                if (this.tmd.Contents[index].Index == 0)
                {
                    try
                    {
                        bannerApp.LoadFile(contents[index]);
                        hasBanner = true;
                        break;
                    }
                    catch
                    {
                        hasBanner = false;
                        break;
                    }
                }
            }
        }

        public void Save(string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using FileStream fileStream = new FileStream(savePath, FileMode.Create);
            WriteToStream(fileStream);
        }

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

        public void ChangeTitleID(LowerTitleID lowerID, string upperID)
        {
            uint num1 = upperID.Length == 4 ? BitConverter.ToUInt32(new byte[4]
            {
        (byte) upperID[3],
        (byte) upperID[2],
        (byte) upperID[1],
        (byte) upperID[0]
            }, 0) : throw new Exception("Upper Title ID must be 4 characters long!");
            ulong num2 = (ulong)lowerID << 32 | num1;
            tik.TitleID = num2;
            tmd.TitleID = num2;
        }

        public void ChangeStartupIOS(int newIos)
        {
            StartupIOS = 4294967296UL | (uint)newIos;
        }

        public void ChangeTitleKey(string newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        public void ChangeTitleKey(char[] newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        public void ChangeTitleKey(byte[] newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        public byte[] GetContentByIndex(int index)
        {
            for (int index1 = 0; index1 < tmd.NumOfContents; ++index1)
            {
                if (tmd.Contents[index1].Index == index)
                {
                    return contents[index1];
                }
            }
            throw new Exception(string.Format("Content with index {0} not found!", index));
        }

        public byte[] GetContentByID(int contentID)
        {
            for (int index = 0; index < tmd.NumOfContents; ++index)
            {
                if (tmd.Contents[index].Index == contentID)
                {
                    return contents[index];
                }
            }
            throw new Exception(string.Format("Content with content ID {0} not found!", contentID));
        }

        public void ChangeChannelTitles(params string[] newTitles)
        {
            if (!hasBanner)
            {
                return;
            } ((Headers.IMET)bannerApp.Header).ChangeTitles(newTitles);
        }

        public void AddContent(byte[] newContent, int contentID, int index, ContentType type = ContentType.Normal)
        {
            tmd.AddContent(new TMD_Content()
            {
                ContentID = (uint)contentID,
                Index = (ushort)index,
                Type = type,
                Size = (ulong)newContent.Length,
                Hash = sha.ComputeHash(newContent)
            });
            contents.Add(newContent);
            wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
        }

        public void RemoveContent(int index)
        {
            for (int index1 = 0; index1 < tmd.Contents.Length; ++index1)
            {
                if (tmd.Contents[index1].Index == index)
                {
                    tmd.RemoveContent(index);
                    contents.RemoveAt(index1);
                    wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
                    return;
                }
            }
            throw new Exception(string.Format("Content with index {0} not found!", index));
        }

        public void RemoveContentByID(int contentID)
        {
            for (int index = 0; index < tmd.Contents.Length; ++index)
            {
                if (tmd.Contents[index].Index == contentID)
                {
                    tmd.RemoveContentByID(contentID);
                    contents.RemoveAt(index);
                    wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
                    return;
                }
            }
            throw new Exception(string.Format("Content with content ID {0} not found!", contentID));
        }

        public void RemoveAllContents()
        {
            if (!hasBanner)
            {
                tmd.Contents = new TMD_Content[0];
                contents = new List<byte[]>();
                wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
            }
            else
            {
                for (int index = 0; index < tmd.NumOfContents; ++index)
                {
                    if (tmd.Contents[index].Index == 0)
                    {
                        byte[] content1 = contents[index];
                        TMD_Content content2 = tmd.Contents[index];
                        tmd.Contents = new TMD_Content[0];
                        contents = new List<byte[]>();
                        tmd.AddContent(content2);
                        contents.Add(content1);
                        wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
                        break;
                    }
                }
            }
        }

        public void Unpack(string unpackDir, bool nameContentID = false)
        {
            UnpackAll(unpackDir, nameContentID);
        }

        public void RemoveFooter()
        {
            footer = new byte[0];
            wadHeader.FooterSize = 0U;
            keepOriginalFooter = true;
        }

        public void AddFooter(byte[] footer)
        {
            ChangeFooter(footer);
        }

        public void ChangeFooter(byte[] newFooter)
        {
            if (newFooter.Length % 64 != 0)
            {
                Array.Resize<byte>(ref newFooter, Shared.AddPadding(newFooter.Length));
            }

            footer = newFooter;
            wadHeader.FooterSize = (uint)newFooter.Length;
            keepOriginalFooter = true;
        }

        private void WriteToStream(Stream writeStream)
        {
            FireDebug("Writing Wad...");
            if (!keepOriginalFooter)
            {
                FireDebug("   Building Footer Timestamp...");
                CreateFooterTimestamp();
            }
            if (hasBanner)
            {
                if (lz77CompressBannerAndIcon || lz77DecompressBannerAndIcon)
                {
                    for (int index = 0; index < bannerApp.Nodes.Count; ++index)
                    {
                        if (bannerApp.StringTable[index].ToLower() == "icon.bin" || bannerApp.StringTable[index].ToLower() == "banner.bin")
                        {
                            if (!Lz77.IsLz77Compressed(bannerApp.Data[index]) && lz77CompressBannerAndIcon)
                            {
                                FireDebug("   Compressing {0}...", (object)bannerApp.StringTable[index]);
                                byte[] file = new byte[bannerApp.Data[index].Length - 32];
                                Array.Copy(bannerApp.Data[index], 32, file, 0, file.Length);
                                byte[] numArray = Headers.IMD5.AddHeader(new Lz77().Compress(file));
                                bannerApp.Data[index] = numArray;
                                bannerApp.Nodes[index].SizeOfData = (uint)numArray.Length;
                            }
                            else if (Lz77.IsLz77Compressed(bannerApp.Data[index]) && lz77DecompressBannerAndIcon)
                            {
                                FireDebug("   Decompressing {0}...", (object)bannerApp.StringTable[index]);
                                byte[] file = new byte[bannerApp.Data[index].Length - 32];
                                Array.Copy(bannerApp.Data[index], 32, file, 0, file.Length);
                                byte[] numArray = Headers.IMD5.AddHeader(new Lz77().Decompress(file));
                                bannerApp.Data[index] = numArray;
                                bannerApp.Nodes[index].SizeOfData = (uint)numArray.Length;
                            }
                        }
                    }
                }
                for (int index = 0; index < contents.Count; ++index)
                {
                    if (tmd.Contents[index].Index == 0)
                    {
                        FireDebug("   Saving Banner App...");
                        contents[index] = bannerApp.ToByteArray();
                        break;
                    }
                }
            }
            FireDebug("   Updating Header...");
            int num = 0;
            for (int index = 0; index < contents.Count - 1; ++index)
            {
                num += Shared.AddPadding(contents[index].Length);
            }

            wadHeader.ContentSize = (uint)(num + contents[contents.Count - 1].Length);
            wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
            FireDebug("   Updating TMD Contents...");
            tmd.UpdateContents(contents.ToArray());
            FireDebug("   Writing Wad Header... (Offset: 0x{0})", (object)writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(0L, SeekOrigin.Begin);
            wadHeader.Write(writeStream);
            FireDebug("   Writing Certificate Chain... (Offset: 0x{0})", (object)writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            byte[] byteArray1 = cert.ToByteArray();
            writeStream.Write(byteArray1, 0, byteArray1.Length);
            FireDebug("   Writing Ticket... (Offset: 0x{0})", (object)writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            byte[] byteArray2 = tik.ToByteArray();
            writeStream.Write(byteArray2, 0, byteArray2.Length);
            FireDebug("   Writing TMD... (Offset: 0x{0})", (object)writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            byte[] byteArray3 = tmd.ToByteArray();
            writeStream.Write(byteArray3, 0, byteArray3.Length);
            ContentIndices[] sortedContentList = tmd.GetSortedContentList();
            for (int index = 0; index < sortedContentList.Length; ++index)
            {
                writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
                FireProgress((index + 1) * 100 / contents.Count);
                FireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper(), index + 1, contents.Count);
                FireDebug("    -> Content ID: 0x{0}", (object)tmd.Contents[sortedContentList[index].Index].ContentID.ToString("x8"));
                FireDebug("    -> Index: 0x{0}", (object)tmd.Contents[sortedContentList[index].Index].Index.ToString("x4"));
                FireDebug("    -> Type: 0x{0} ({1})", ((ushort)tmd.Contents[sortedContentList[index].Index].Type).ToString("x4"), tmd.Contents[sortedContentList[index].Index].Type.ToString());
                FireDebug("    -> Size: {0} bytes", (object)tmd.Contents[sortedContentList[index].Index].Size);
                FireDebug("    -> Hash: {0}", (object)Shared.ByteArrayToString(tmd.Contents[sortedContentList[index].Index].Hash));
                byte[] buffer = EncryptContent(contents[sortedContentList[index].Index], sortedContentList[index].Index);
                writeStream.Write(buffer, 0, buffer.Length);
            }
            if (wadHeader.FooterSize != 0U)
            {
                FireDebug("   Writing Footer... (Offset: 0x{0})", (object)writeStream.Position.ToString("x8").ToUpper());
                writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
                writeStream.Write(footer, 0, footer.Length);
            }
            while (writeStream.Position % 64L != 0L)
            {
                writeStream.WriteByte(0);
            }

            FireDebug("Writing Wad Finished... (Written Bytes: {0})", (object)writeStream.Position);
        }

        private void UnpackAll(string unpackDir, bool nameContentId)
        {
            FireDebug("Unpacking Wad to: {0}", (object)unpackDir);
            if (!Directory.Exists(unpackDir))
            {
                Directory.CreateDirectory(unpackDir);
            }

            string str1 = this.tik.TitleID.ToString("x16");
            FireDebug("   Saving Certificate Chain: {0}.cert", (object)str1);
            CertificateChain cert = this.cert;
            string str2 = unpackDir;
            char directorySeparatorChar = Path.DirectorySeparatorChar;
            string str3 = directorySeparatorChar.ToString();
            string str4 = str1;
            string savePath1 = str2 + str3 + str4 + ".cert";
            cert.Save(savePath1);
            FireDebug("   Saving Ticket: {0}.tik", (object)str1);
            Ticket tik = this.tik;
            string str5 = unpackDir;
            directorySeparatorChar = Path.DirectorySeparatorChar;
            string str6 = directorySeparatorChar.ToString();
            string str7 = str1;
            string savePath2 = str5 + str6 + str7 + ".tik";
            tik.Save(savePath2);
            FireDebug("   Saving TMD: {0}.tmd", (object)str1);
            TMD tmd = this.tmd;
            string str8 = unpackDir;
            directorySeparatorChar = Path.DirectorySeparatorChar;
            string str9 = directorySeparatorChar.ToString();
            string str10 = str1;
            string savePath3 = str8 + str9 + str10 + ".tmd";
            tmd.Save(savePath3);
            for (int index = 0; index < this.tmd.NumOfContents; ++index)
            {
                FireProgress((index + 1) * 100 / this.tmd.NumOfContents);
                FireDebug("   Saving Content #{0} of {1}: {2}.app", index + 1, this.tmd.NumOfContents, nameContentId ? this.tmd.Contents[index].ContentID.ToString("x8") : this.tmd.Contents[index].Index.ToString("x8"));
                FireDebug("    -> Content ID: 0x{0}", (object)this.tmd.Contents[index].ContentID.ToString("x8"));
                FireDebug("    -> Index: 0x{0}", (object)this.tmd.Contents[index].Index.ToString("x4"));
                object[] objArray = new object[2];
                ushort num = (ushort)this.tmd.Contents[index].Type;
                objArray[0] = num.ToString("x4");
                objArray[1] = this.tmd.Contents[index].Type.ToString();
                FireDebug("    -> Type: 0x{0} ({1})", objArray);
                FireDebug("    -> Size: {0} bytes", (object)this.tmd.Contents[index].Size);
                FireDebug("    -> Hash: {0}", (object)Shared.ByteArrayToString(this.tmd.Contents[index].Hash));
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
                {
                    str13 = this.tmd.Contents[index].ContentID.ToString("x8");
                }

                using FileStream fileStream = new FileStream(str11 + str12 + str13 + ".app", FileMode.Create);
                fileStream.Write(contents[index], 0, contents[index].Length);
            }
            FireDebug("   Saving Footer: {0}.footer", (object)str1);
            string str14 = unpackDir;
            directorySeparatorChar = Path.DirectorySeparatorChar;
            string str15 = directorySeparatorChar.ToString();
            string str16 = str1;
            using (FileStream fileStream = new FileStream(str14 + str15 + str16 + ".footer", FileMode.Create))
            {
                fileStream.Write(footer, 0, footer.Length);
            }

            FireDebug("Unpacking Wad Finished...");
        }

        private void ParseWad(Stream wadFile)
        {
            FireDebug("Parsing Wad...");
            wadFile.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[4];
            wadHeader = new WAD_Header();
            contents = new List<byte[]>();
            FireDebug("   Parsing Header... (Offset: 0x{0})", (object)wadFile.Position.ToString("x8").ToUpper());
            wadFile.Read(buffer, 0, 4);
            if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) != (int)wadHeader.HeaderSize)
            {
                throw new Exception("Invalid Headersize!");
            }

            wadFile.Read(buffer, 0, 4);
            wadHeader.WadType = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            wadFile.Seek(12L, SeekOrigin.Current);
            wadFile.Read(buffer, 0, 4);
            wadHeader.TmdSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            wadFile.Read(buffer, 0, 4);
            wadHeader.ContentSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            wadFile.Read(buffer, 0, 4);
            wadHeader.FooterSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Parsing Certificate Chain... (Offset: 0x{0})", (object)wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
            byte[] numArray1 = new byte[(int)wadHeader.CertSize];
            wadFile.Read(numArray1, 0, numArray1.Length);
            cert.LoadFile(numArray1);
            FireDebug("   Parsing Ticket... (Offset: 0x{0})", (object)wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
            byte[] numArray2 = new byte[(int)wadHeader.TicketSize];
            wadFile.Read(numArray2, 0, numArray2.Length);
            tik.LoadFile(numArray2);
            FireDebug("   Parsing TMD... (Offset: 0x{0})", (object)wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
            byte[] numArray3 = new byte[(int)wadHeader.TmdSize];
            wadFile.Read(numArray3, 0, numArray3.Length);
            tmd.LoadFile(numArray3);
            if ((long)tmd.TitleID != (long)tik.TitleID)
            {
                FireWarning("The Title ID in the Ticket doesn't match the one in the TMD!");
            }

            long position;
            for (int contentIndex = 0; contentIndex < tmd.NumOfContents; ++contentIndex)
            {
                FireProgress((contentIndex + 1) * 100 / tmd.NumOfContents);
                object[] objArray1 = new object[3]
                {
           contentIndex + 1,
           tmd.NumOfContents,
          null
                };
                position = wadFile.Position;
                objArray1[2] = position.ToString("x8").ToUpper();
                FireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", objArray1);
                FireDebug("    -> Content ID: 0x{0}", (object)tmd.Contents[contentIndex].ContentID.ToString("x8"));
                object[] objArray2 = new object[1];
                ushort num = tmd.Contents[contentIndex].Index;
                objArray2[0] = num.ToString("x4");
                FireDebug("    -> Index: 0x{0}", objArray2);
                object[] objArray3 = new object[2];
                num = (ushort)tmd.Contents[contentIndex].Type;
                objArray3[0] = num.ToString("x4");
                objArray3[1] = tmd.Contents[contentIndex].Type.ToString();
                FireDebug("    -> Type: 0x{0} ({1})", objArray3);
                FireDebug("    -> Size: {0} bytes", (object)tmd.Contents[contentIndex].Size);
                FireDebug("    -> Hash: {0}", (object)Shared.ByteArrayToString(tmd.Contents[contentIndex].Hash));
                wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
                byte[] numArray4 = new byte[Shared.AddPadding((int)tmd.Contents[contentIndex].Size, 16)];
                wadFile.Read(numArray4, 0, numArray4.Length);
                byte[] array = DecryptContent(numArray4, contentIndex);
                Array.Resize<byte>(ref array, (int)tmd.Contents[contentIndex].Size);
                if (!Shared.CompareByteArrays(tmd.Contents[contentIndex].Hash, sha.ComputeHash(array, 0, (int)tmd.Contents[contentIndex].Size)))
                {
                    FireDebug("/!\\ /!\\ /!\\ Hashes do not match /!\\ /!\\ /!\\");
                    // ISSUE: variable of a boxed type
                    int local = contentIndex + 1;
                    string str1 = tmd.Contents[contentIndex].ContentID.ToString("x8");
                    num = tmd.Contents[contentIndex].Index;
                    string str2 = num.ToString("x4");
                    FireWarning(string.Format("Content #{0} (Content ID: 0x{1}; Index: 0x{2}): Hashes do not match! The content might be corrupted!", local, str1, str2));
                }
                contents.Add(array);
                if (tmd.Contents[contentIndex].Index == 0)
                {
                    try
                    {
                        bannerApp.LoadFile(array);
                        hasBanner = true;
                    }
                    catch
                    {
                        hasBanner = false;
                    }
                }
            }
            if (wadHeader.FooterSize != 0U)
            {
                object[] objArray = new object[1];
                position = wadFile.Position;
                objArray[0] = position.ToString("x8").ToUpper();
                FireDebug("   Reading Footer... (Offset: 0x{0})", objArray);
                footer = new byte[(int)wadHeader.FooterSize];
                wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
                wadFile.Read(footer, 0, footer.Length);
                ParseFooterTimestamp();
            }
            FireDebug("Parsing Wad Finished...");
        }

        private byte[] DecryptContent(byte[] content, int contentIndex)
        {
            int length = content.Length;
            Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
            byte[] titleKey = tik.TitleKey;
            byte[] numArray = new byte[16];
            byte[] bytes = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
            numArray[0] = bytes[1];
            numArray[1] = bytes[0];
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = 128,
                Key = titleKey,
                IV = numArray
            };
            ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor();
            MemoryStream memoryStream = new MemoryStream(content);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[length];
            cryptoStream.Read(buffer, 0, buffer.Length);
            cryptoStream.Dispose();
            memoryStream.Dispose();
            return buffer;
        }

        private byte[] EncryptContent(byte[] content, int contentIndex)
        {
            Array.Resize<byte>(ref content, Shared.AddPadding(content.Length, 16));
            byte[] titleKey = tik.TitleKey;
            byte[] numArray = new byte[16];
            byte[] bytes = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
            numArray[0] = bytes[1];
            numArray[1] = bytes[0];
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = 128,
                Key = titleKey,
                IV = numArray
            };
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream(content);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[content.Length];
            cryptoStream.Read(buffer, 0, buffer.Length);
            cryptoStream.Dispose();
            memoryStream.Dispose();
            return buffer;
        }

        private void CreateFooterTimestamp()
        {
            byte[] bytes = new ASCIIEncoding().GetBytes("TmStmp" + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString());
            Array.Resize<byte>(ref bytes, 64);
            wadHeader.FooterSize = (uint)bytes.Length;
            footer = bytes;
        }

        private void ParseFooterTimestamp()
        {
            creationTimeUTC = new DateTime(1970, 1, 1);
            if ((footer[0] != 67 || footer[1] != 77 || (footer[2] != 105 || footer[3] != 105) || (footer[4] != 85 || footer[5] != 84)) && (footer[0] != 84 || footer[1] != 109 || (footer[2] != 83 || footer[3] != 116) || (footer[4] != 109 || footer[5] != 112)))
            {
                return;
            }

            string s = new ASCIIEncoding().GetString(footer, 6, 10);
            int num = 0;
            ref int local = ref num;
            if (!int.TryParse(s, out local))
            {
                return;
            }

            creationTimeUTC = creationTimeUTC.AddSeconds(num);
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

        private void FireWarning(string warningMessage)
        {
            EventHandler<MessageEventArgs> warning = Warning;
            if (warning == null)
            {
                return;
            }

            warning(new object(), new MessageEventArgs(warningMessage));
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

        private void Cert_Debug(object sender, MessageEventArgs e)
        {
            FireDebug("      Certificate Chain: {0}", (object)e.Message);
        }

        private void Tik_Debug(object sender, MessageEventArgs e)
        {
            FireDebug("      Ticket: {0}", (object)e.Message);
        }

        private void Tmd_Debug(object sender, MessageEventArgs e)
        {
            FireDebug("      TMD: {0}", (object)e.Message);
        }

        private void BannerApp_Debug(object sender, MessageEventArgs e)
        {
            FireDebug("      BannerApp: {0}", (object)e.Message);
        }

        private void BannerApp_Warning(object sender, MessageEventArgs e)
        {
            FireWarning(e.Message);
        }
    }

    public class WAD_Header
    {
        private readonly uint headerSize = 0x20;
        private uint wadType = 0x49730000;
        private readonly uint certSize = 0xA00;
        private readonly uint reserved = 0x00;
        private readonly uint tikSize = 0x2A4;
        private uint tmdSize;
        private uint contentSize;
        private uint footerSize = 0x00;

        public uint HeaderSize => headerSize;
        public uint WadType { get => wadType; set => wadType = value; }
        public uint CertSize => certSize;
        public uint Reserved => reserved;
        public uint TicketSize => tikSize;
        public uint TmdSize { get => tmdSize; set => tmdSize = value; }
        public uint ContentSize { get => contentSize; set => contentSize = value; }
        public uint FooterSize { get => footerSize; set => footerSize = value; }

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
