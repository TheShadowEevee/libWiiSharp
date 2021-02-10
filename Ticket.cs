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
            get => decryptedTitleKey;
            set
            {
                decryptedTitleKey = value;
                titleKeyChanged = true;
                reDecrypt = false;
            }
        }

        public CommonKeyType CommonKeyIndex
        {
            get => (CommonKeyType)newKeyIndex;
            set => newKeyIndex = (byte)value;
        }

        public ulong TicketID
        {
            get => ticketId;
            set => ticketId = value;
        }

        public uint ConsoleID
        {
            get => consoleId;
            set => consoleId = value;
        }

        public ulong TitleID
        {
            get => titleId;
            set
            {
                titleId = value;
                if (!reDecrypt)
                {
                    return;
                }

                PrivReDecryptTitleKey();
            }
        }

        public ushort NumOfDLC
        {
            get => numOfDlc;
            set => numOfDlc = value;
        }

        public bool FakeSign
        {
            get => fakeSign;
            set => fakeSign = value;
        }

        public bool TitleKeyChanged => titleKeyChanged;

        public event EventHandler<MessageEventArgs> Debug;

        ~Ticket() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                decryptedTitleKey = null;
                newEncryptedTitleKey = null;
                signature = null;
                padding = null;
                issuer = null;
                unknown = null;
                encryptedTitleKey = null;
                unknown5 = null;
                unknown6 = null;
                padding4 = null;
            }
            isDisposed = true;
        }

        public static Ticket Load(string pathToTicket)
        {
            return Load(File.ReadAllBytes(pathToTicket));
        }

        public static Ticket Load(byte[] ticket)
        {
            Ticket ticket1 = new Ticket();
            MemoryStream memoryStream = new MemoryStream(ticket);
            try
            {
                ticket1.PrivParseTicket(memoryStream);
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
            ticket1.PrivParseTicket(ticket);
            return ticket1;
        }

        public void LoadFile(string pathToTicket)
        {
            LoadFile(File.ReadAllBytes(pathToTicket));
        }

        public void LoadFile(byte[] ticket)
        {
            MemoryStream memoryStream = new MemoryStream(ticket);
            try
            {
                PrivParseTicket(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        public void LoadFile(Stream ticket)
        {
            PrivParseTicket(ticket);
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
            PrivWriteToStream(fileStream);
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
                PrivWriteToStream(memoryStream);
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
                PrivWriteToStream(memoryStream);
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

        public void SetTitleKey(string newTitleKey)
        {
            SetTitleKey(newTitleKey.ToCharArray());
        }

        public void SetTitleKey(char[] newTitleKey)
        {
            if (newTitleKey.Length != 16)
            {
                throw new Exception("The title key must be 16 characters long!");
            }

            for (int index = 0; index < 16; ++index)
            {
                encryptedTitleKey[index] = (byte)newTitleKey[index];
            }

            PrivDecryptTitleKey();
            titleKeyChanged = true;
            reDecrypt = true;
            newEncryptedTitleKey = encryptedTitleKey;
        }

        public void SetTitleKey(byte[] newTitleKey)
        {
            encryptedTitleKey = newTitleKey.Length == 16 ? newTitleKey : throw new Exception("The title key must be 16 characters long!");
            PrivDecryptTitleKey();
            titleKeyChanged = true;
            reDecrypt = true;
            newEncryptedTitleKey = newTitleKey;
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

        private void PrivWriteToStream(Stream writeStream)
        {
            FireDebug("Writing Ticket...");
            FireDebug("   Encrypting Title Key...");
            PrivEncryptTitleKey();
            FireDebug("    -> Decrypted Title Key: {0}", (object)Shared.ByteArrayToString(decryptedTitleKey));
            FireDebug("    -> Encrypted Title Key: {0}", (object)Shared.ByteArrayToString(encryptedTitleKey));
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
            FireDebug("   Writing Unknown... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(unknown, 0, unknown.Length);
            FireDebug("   Writing Title Key... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(encryptedTitleKey, 0, encryptedTitleKey.Length);
            FireDebug("   Writing Unknown2... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(unknown2);
            FireDebug("   Writing Ticket ID... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(ticketId)), 0, 8);
            FireDebug("   Writing Console ID... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(consoleId)), 0, 4);
            FireDebug("   Writing Title ID... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(titleId)), 0, 8);
            FireDebug("   Writing Unknwon3... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(unknown3)), 0, 2);
            FireDebug("   Writing NumOfDLC... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(numOfDlc)), 0, 2);
            FireDebug("   Writing Unknwon4... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(unknown4)), 0, 8);
            FireDebug("   Writing Padding2... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(padding2);
            FireDebug("   Writing Common Key Index... (Offset: 0x{0})", (object)memoryStream.Position.ToString("x8").ToUpper());
            memoryStream.WriteByte(commonKeyIndex);
            object[] objArray1 = new object[1];
            long position = memoryStream.Position;
            objArray1[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Unknown5... (Offset: 0x{0})", objArray1);
            memoryStream.Write(unknown5, 0, unknown5.Length);
            object[] objArray2 = new object[1];
            position = memoryStream.Position;
            objArray2[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Unknown6... (Offset: 0x{0})", objArray2);
            memoryStream.Write(unknown6, 0, unknown6.Length);
            object[] objArray3 = new object[1];
            position = memoryStream.Position;
            objArray3[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Padding3... (Offset: 0x{0})", objArray3);
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(padding3)), 0, 2);
            object[] objArray4 = new object[1];
            position = memoryStream.Position;
            objArray4[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Enable Time Limit... (Offset: 0x{0})", objArray4);
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(enableTimeLimit)), 0, 4);
            object[] objArray5 = new object[1];
            position = memoryStream.Position;
            objArray5[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Time Limit... (Offset: 0x{0})", objArray5);
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(timeLimit)), 0, 4);
            object[] objArray6 = new object[1];
            position = memoryStream.Position;
            objArray6[0] = position.ToString("x8").ToUpper();
            FireDebug("   Writing Padding4... (Offset: 0x{0})", objArray6);
            memoryStream.Write(padding4, 0, padding4.Length);
            byte[] array = memoryStream.ToArray();
            memoryStream.Dispose();
            if (fakeSign)
            {
                FireDebug("   Fakesigning Ticket...");
                //byte[] numArray = new byte[20];
                SHA1 shA1 = SHA1.Create();
                for (ushort index = 0; index < ushort.MaxValue; ++index)
                {
                    byte[] bytes = BitConverter.GetBytes(index);
                    array[498] = bytes[1];
                    array[499] = bytes[0];
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
            FireDebug("Writing Ticket Finished...");
        }

        private void PrivParseTicket(Stream ticketFile)
        {
            FireDebug("Parsing Ticket...");
            ticketFile.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[8];
            FireDebug("   Reading Signature Exponent... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 4);
            signatureExponent = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Reading Signature... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(signature, 0, signature.Length);
            FireDebug("   Reading Padding... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(padding, 0, padding.Length);
            FireDebug("   Reading Issuer... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(issuer, 0, issuer.Length);
            FireDebug("   Reading Unknown... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown, 0, unknown.Length);
            FireDebug("   Reading Title Key... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(encryptedTitleKey, 0, encryptedTitleKey.Length);
            FireDebug("   Reading Unknown2... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            unknown2 = (byte)ticketFile.ReadByte();
            FireDebug("   Reading Ticket ID.. (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 8);
            ticketId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
            FireDebug("   Reading Console ID... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 4);
            consoleId = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            FireDebug("   Reading Title ID... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 8);
            titleId = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
            FireDebug("   Reading Unknown3... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading NumOfDLC... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 4);
            unknown3 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            numOfDlc = Shared.Swap(BitConverter.ToUInt16(buffer, 2));
            FireDebug("   Reading Unknown4... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 8);
            unknown4 = Shared.Swap(BitConverter.ToUInt64(buffer, 0));
            FireDebug("   Reading Padding2... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            padding2 = (byte)ticketFile.ReadByte();
            FireDebug("   Reading Common Key Index... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            commonKeyIndex = (byte)ticketFile.ReadByte();
            newKeyIndex = commonKeyIndex;
            FireDebug("   Reading Unknown5... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown5, 0, unknown5.Length);
            FireDebug("   Reading Unknown6... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown6, 0, unknown6.Length);
            FireDebug("   Reading Padding3... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 2);
            padding3 = Shared.Swap(BitConverter.ToUInt16(buffer, 0));
            FireDebug("   Reading Enable Time Limit... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            FireDebug("   Reading Time Limit... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(buffer, 0, 8);
            enableTimeLimit = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
            timeLimit = Shared.Swap(BitConverter.ToUInt32(buffer, 4));
            FireDebug("   Reading Padding4... (Offset: 0x{0})", (object)ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(padding4, 0, padding4.Length);
            FireDebug("   Decrypting Title Key...");
            PrivDecryptTitleKey();
            FireDebug("    -> Encrypted Title Key: {0}", (object)Shared.ByteArrayToString(encryptedTitleKey));
            FireDebug("    -> Decrypted Title Key: {0}", (object)Shared.ByteArrayToString(decryptedTitleKey));
            FireDebug("Parsing Ticket Finished...");
        }

        private void PrivDecryptTitleKey()
        {
            byte[] numArray = commonKeyIndex == 1 ? CommonKey.GetKoreanKey() : CommonKey.GetStandardKey();
            byte[] bytes = BitConverter.GetBytes(Shared.Swap(titleId));
            Array.Resize<byte>(ref bytes, 16);
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = 128,
                Key = numArray,
                IV = bytes
            };
            ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor();
            MemoryStream memoryStream = new MemoryStream(encryptedTitleKey);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            cryptoStream.Read(decryptedTitleKey, 0, decryptedTitleKey.Length);
            cryptoStream.Dispose();
            memoryStream.Dispose();
            decryptor.Dispose();
            rijndaelManaged.Clear();
        }

        private void PrivEncryptTitleKey()
        {
            commonKeyIndex = newKeyIndex;
            byte[] numArray = commonKeyIndex == 1 ? CommonKey.GetKoreanKey() : CommonKey.GetStandardKey();
            byte[] bytes = BitConverter.GetBytes(Shared.Swap(titleId));
            Array.Resize<byte>(ref bytes, 16);
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None,
                KeySize = 128,
                BlockSize = 128,
                Key = numArray,
                IV = bytes
            };
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream(decryptedTitleKey);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read);
            cryptoStream.Read(encryptedTitleKey, 0, encryptedTitleKey.Length);
            cryptoStream.Dispose();
            memoryStream.Dispose();
            encryptor.Dispose();
            rijndaelManaged.Clear();
        }

        private void PrivReDecryptTitleKey()
        {
            encryptedTitleKey = newEncryptedTitleKey;
            PrivDecryptTitleKey();
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
