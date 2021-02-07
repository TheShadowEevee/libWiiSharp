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
    public enum ContentType : ushort
    {
        Normal = 1,
        DLC = 16385, // 0x4001
        Shared = 32769, // 0x8001
    }
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
            get => (Region)region;
            set => region = (ushort)value;
        }

        public ulong StartupIOS
        {
            get => startupIos;
            set => startupIos = value;
        }

        public ulong TitleID
        {
            get => titleId;
            set => titleId = value;
        }

        public ushort TitleVersion
        {
            get => titleVersion;
            set => titleVersion = value;
        }

        public ushort NumOfContents => numOfContents;

        public ushort BootIndex
        {
            get => bootIndex;
            set
            {
                if (value > numOfContents)
                {
                    return;
                }

                bootIndex = value;
            }
        }

        public TMD_Content[] Contents
        {
            get => contents.ToArray();
            set
            {
                contents = new List<TMD_Content>(value);
                numOfContents = (ushort)value.Length;
            }
        }

        public bool FakeSign
        {
            get => fakeSign;
            set => fakeSign = value;
        }

        public bool SortContents
        {
            get => sortContents;
            set => sortContents = true;
        }

        public event EventHandler<MessageEventArgs> Debug;

        ~TMD() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                signature = null;
                padding = null;
                issuer = null;
                reserved = null;
                contents.Clear();
                contents = null;
            }
            isDisposed = true;
        }

        public static TMD Load(string pathToTmd)
        {
            return TMD.Load(File.ReadAllBytes(pathToTmd));
        }

        public static TMD Load(byte[] tmdFile)
        {
            TMD tmd = new TMD();
            MemoryStream memoryStream = new MemoryStream(tmdFile);
            try
            {
                tmd.ParseTmd(memoryStream);
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
            tmd1.ParseTmd(tmd);
            return tmd1;
        }

        public void LoadFile(string pathToTmd)
        {
            LoadFile(File.ReadAllBytes(pathToTmd));
        }

        public void LoadFile(byte[] tmdFile)
        {
            MemoryStream memoryStream = new MemoryStream(tmdFile);
            try
            {
                ParseTmd(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        public void LoadFile(Stream tmd)
        {
            ParseTmd(tmd);
        }

        public void Save(string savePath)
        {
            Save(savePath, false);
        }

        public void Save(string savePath, bool fakeSign)
        {
            if (fakeSign)
            {
                this.fakeSign = true;
            }

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using FileStream fileStream = new FileStream(savePath, FileMode.Create);
            WriteToStream(fileStream);
        }

        public MemoryStream ToMemoryStream()
        {
            return ToMemoryStream(false);
        }

        public MemoryStream ToMemoryStream(bool fakeSign)
        {
            if (fakeSign)
            {
                this.fakeSign = true;
            }

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
            return ToByteArray(false);
        }

        public byte[] ToByteArray(bool fakeSign)
        {
            if (fakeSign)
            {
                this.fakeSign = true;
            }

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

        public void UpdateContents(string contentDir)
        {
            bool flag = true;
            char directorySeparatorChar;
            for (int index = 0; index < contents.Count; ++index)
            {
                string str1 = contentDir;
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str2 = directorySeparatorChar.ToString();
                string str3 = contents[index].ContentID.ToString("x8");
                if (!File.Exists(str1 + str2 + str3 + ".app"))
                {
                    flag = false;
                    break;
                }
            }
            if (!flag)
            {
                for (int index = 0; index < contents.Count; ++index)
                {
                    string str1 = contentDir;
                    directorySeparatorChar = Path.DirectorySeparatorChar;
                    string str2 = directorySeparatorChar.ToString();
                    string str3 = contents[index].ContentID.ToString("x8");
                    if (!File.Exists(str1 + str2 + str3 + ".app"))
                    {
                        throw new Exception("Couldn't find all content files!");
                    }
                }
            }
            byte[][] conts = new byte[contents.Count][];
            for (int index = 0; index < contents.Count; ++index)
            {
                string str1 = contentDir;
                directorySeparatorChar = Path.DirectorySeparatorChar;
                string str2 = directorySeparatorChar.ToString();
                string str3 = flag ? contents[index].ContentID.ToString("x8") : contents[index].Index.ToString("x8");
                string path = str1 + str2 + str3 + ".app";
                conts[index] = File.ReadAllBytes(path);
            }
            UpdateContents(conts);
        }

        public void UpdateContents(byte[][] contents)
        {
            UpdateContents(contents);
        }

        public string GetUpperTitleID()
        {
            byte[] bytes = BitConverter.GetBytes(Shared.Swap((uint)titleId));
            return new string(new char[4]
            {
        (char) bytes[0],
        (char) bytes[1],
        (char) bytes[2],
        (char) bytes[3]
            });
        }

        public string GetNandBlocks()
        {
            return CalculateNandBlocks();
        }

        public void AddContent(TMD_Content content)
        {
            contents.Add(content);
            numOfContents = (ushort)contents.Count;
        }

        public void RemoveContent(int contentIndex)
        {
            for (int index = 0; index < numOfContents; ++index)
            {
                if (contents[index].Index == contentIndex)
                {
                    contents.RemoveAt(index);
                    break;
                }
            }
            numOfContents = (ushort)contents.Count;
        }

        public void RemoveContentByID(int contentId)
        {
            for (int index = 0; index < numOfContents; ++index)
            {
                if (contents[index].ContentID == contentId)
                {
                    contents.RemoveAt(index);
                    break;
                }
            }
            numOfContents = (ushort)contents.Count;
        }

        public ContentIndices[] GetSortedContentList()
        {
            List<ContentIndices> contentIndicesList = new List<ContentIndices>();
            for (int index = 0; index < contents.Count; ++index)
            {
                contentIndicesList.Add(new ContentIndices(index, contents[index].Index));
            }

            if (sortContents)
            {
                contentIndicesList.Sort();
            }

            return contentIndicesList.ToArray();
        }

        private void WriteToStream(Stream writeStream)
        {
            FireDebug("Writing TMD...");
            if (fakeSign)
            {
                FireDebug("   Clearing Signature...");
                signature = new byte[256];
            }
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Seek(0L, SeekOrigin.Begin);
            FireDebug("   Writing Signature Exponent... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(signatureExponent)), 0, 4);
            FireDebug("   Writing Signature... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(signature, 0, signature.Length);
            FireDebug("   Writing Padding... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(padding, 0, padding.Length);
            FireDebug("   Writing Issuer... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(issuer, 0, issuer.Length);
            FireDebug("   Writing Version... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(version);
            FireDebug("   Writing CA Crl Version... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(caCrlVersion);
            FireDebug("   Writing Signer Crl Version... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(signerCrlVersion);
            FireDebug("   Writing Padding Byte... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(paddingByte);
            FireDebug("   Writing Startup IOS... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(startupIos)), 0, 8);
            FireDebug("   Writing Title ID... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(titleId)), 0, 8);
            FireDebug("   Writing Title Type... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(titleType)), 0, 4);
            FireDebug("   Writing Group ID... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(groupId)), 0, 2);
            FireDebug("   Writing Padding2... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(padding2)), 0, 2);
            FireDebug("   Writing Region... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(region)), 0, 2);
            FireDebug("   Writing Reserved... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(reserved, 0, reserved.Length);
            FireDebug("   Writing Access Rights... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(accessRights)), 0, 4);
            FireDebug("   Writing Title Version... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(titleVersion)), 0, 2);
            FireDebug("   Writing NumOfContents... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(numOfContents)), 0, 2);
            FireDebug("   Writing Boot Index... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(bootIndex)), 0, 2);
            FireDebug("   Writing Padding3... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(padding3)), 0, 2);
            List<ContentIndices> contentIndicesList = new List<ContentIndices>();
            for (int index = 0; index < contents.Count; ++index)
            {
                contentIndicesList.Add(new ContentIndices(index, contents[index].Index));
            }

            if (sortContents)
            {
                contentIndicesList.Sort();
            }

            for (int index = 0; index < contentIndicesList.Count; ++index)
            {
                FireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", memoryStream.Position.ToString("x8").ToUpper().ToUpper(), index + 1, numOfContents);
                memoryStream.Write(BitConverter.GetBytes(Shared.Swap(contents[contentIndicesList[index].Index].ContentID)), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(Shared.Swap(contents[contentIndicesList[index].Index].Index)), 0, 2);
                memoryStream.Write(BitConverter.GetBytes(Shared.Swap((ushort)contents[contentIndicesList[index].Index].Type)), 0, 2);
                memoryStream.Write(BitConverter.GetBytes(Shared.Swap(contents[contentIndicesList[index].Index].Size)), 0, 8);
                memoryStream.Write(contents[contentIndicesList[index].Index].Hash, 0, contents[contentIndicesList[index].Index].Hash.Length);
            }
            byte[] array = memoryStream.ToArray();
            memoryStream.Dispose();
            if (fakeSign)
            {
                FireDebug("   Fakesigning TMD...");
                //byte[] numArray = new byte[20];
                SHA1 shA1 = SHA1.Create();
                for (ushort index = 0; index < ushort.MaxValue; ++index)
                {
                    byte[] bytes = BitConverter.GetBytes(index);
                    array[482] = bytes[1];
                    array[483] = bytes[0];
                    if (shA1.ComputeHash(array)[0] == 0)
                    {
                        FireDebug("   -> Signed ({0})", (object)index);
                        break;
                    }
                    if (index == 65534)
                    {
                        FireDebug("    -> Signing Failed...");
                        throw new Exception("Fakesigning failed...");
                    }
                }
                shA1.Clear();
            }
            writeStream.Seek(0L, SeekOrigin.Begin);
            writeStream.Write(array, 0, array.Length);
            FireDebug("Writing TMD Finished...");
        }

        /*
        private void PrivUpdateContents(byte[][] conts)
        {
            SHA1 shA1 = SHA1.Create();
            for (int index = 0; index < this.contents.Count; ++index)
            {
                this.contents[index].Size = (ulong)conts[index].Length;
                this.contents[index].Hash = shA1.ComputeHash(conts[index]);
            }
            shA1.Clear();
        }
        */

        private void ParseTmd(Stream tmdFile)
        {
            FireDebug("Pasing TMD...");
            tmdFile.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[8];
            FireDebug("   Reading Signature Exponent... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 4);
            signatureExponent = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Reading Signature... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(signature, 0, signature.Length);
            FireDebug("   Reading Padding... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(padding, 0, padding.Length);
            FireDebug("   Reading Issuer... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(issuer, 0, issuer.Length);
            FireDebug("   Reading Version... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading CA Crl Version... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading Signer Crl Version... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading Padding Byte... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 4);
            version = buffer[0];
            caCrlVersion = buffer[1];
            signerCrlVersion = buffer[2];
            paddingByte = buffer[3];
            FireDebug("   Reading Startup IOS... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 8);
            startupIos = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
            FireDebug("   Reading Title ID... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 8);
            titleId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
            FireDebug("   Reading Title Type... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 4);
            titleType = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Reading Group ID... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 2);
            groupId = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            FireDebug("   Reading Padding2... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 2);
            padding2 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            FireDebug("   Reading Region... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 2);
            region = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            FireDebug("   Reading Reserved... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(reserved, 0, reserved.Length);
            FireDebug("   Reading Access Rights... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 4);
            accessRights = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Reading Title Version... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading NumOfContents... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading Boot Index... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading Padding3... (Offset: 0x{0})", (object)tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(buffer, 0, 8);
            titleVersion = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            numOfContents = Shared.Swap(BitConverter.ToUInt16(buffer, 2));
            bootIndex = Shared.Swap(BitConverter.ToUInt16(buffer, 4));
            padding3 = Shared.Swap(BitConverter.ToUInt16(buffer, 6));
            contents = new List<TMD_Content>();
            for (int index = 0; index < numOfContents; ++index)
            {
                FireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", index + 1, numOfContents, tmdFile.Position.ToString("x8").ToUpper().ToUpper());
                TMD_Content tmdContent = new TMD_Content
                {
                    Hash = new byte[20]
                };
                tmdFile.Read(buffer, 0, 8);
                tmdContent.ContentID = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
                tmdContent.Index = Shared.Swap(BitConverter.ToUInt16(buffer, 4));
                tmdContent.Type = (ContentType)Shared.Swap(BitConverter.ToUInt16(buffer, 6));
                tmdFile.Read(buffer, 0, 8);
                tmdContent.Size = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
                tmdFile.Read(tmdContent.Hash, 0, tmdContent.Hash.Length);
                contents.Add(tmdContent);
            }
            FireDebug("Pasing TMD Finished...");
        }

        private string CalculateNandBlocks()
        {
            int num1 = 0;
            int num2 = 0;
            for (int index = 0; index < numOfContents; ++index)
            {
                num2 += (int)contents[index].Size;
                if (contents[index].Type == ContentType.Normal)
                {
                    num1 += (int)contents[index].Size;
                }
            }
            int num3 = (int)Math.Ceiling(num1 / 131072.0);
            int num4 = (int)Math.Ceiling(num2 / 131072.0);
            return num3 == num4 ? num4.ToString() : string.Format("{0} - {1}", num3, num4);
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
    }
}
