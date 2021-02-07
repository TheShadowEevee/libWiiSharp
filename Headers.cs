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
    public class Headers
    {
        private static readonly uint imd5Magic = 1229800501;
        private static readonly uint imetMagic = 1229800788;

        /// <summary>
        /// Convert HeaderType to int to get it's Length.
        /// </summary>
        public enum HeaderType
        {
            None = 0,
            /// <summary>
            /// Used in banner.bin / icon.bin
            /// </summary>
            IMD5 = 32,
            /// <summary>
            /// Used in opening.bnr
            /// </summary>
            ShortIMET = 1536,
            /// <summary>
            /// Used in 00000000.app
            /// </summary>
            IMET = 1600,
        }

        #region Public Functions
        /// <summary>
        /// Checks a file for Headers.
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <returns></returns>
        public static Headers.HeaderType DetectHeader(string pathToFile)
        {
            return DetectHeader(File.ReadAllBytes(pathToFile));
        }

        /// <summary>
        /// Checks the byte array for Headers.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Headers.HeaderType DetectHeader(byte[] file)
        {
            if (file.Length > 68 && (int)Shared.Swap(BitConverter.ToUInt32(file, 64)) == (int)imetMagic)
            {
                return HeaderType.ShortIMET;
            }

            if (file.Length > 132 && (int)Shared.Swap(BitConverter.ToUInt32(file, 128)) == (int)imetMagic)
            {
                return HeaderType.IMET;
            }

            return file.Length > 4 && (int)Shared.Swap(BitConverter.ToUInt32(file, 0)) == (int)imd5Magic ? HeaderType.IMD5 : HeaderType.None;
        }

        /// <summary>
        /// Checks the stream for Headers.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Headers.HeaderType DetectHeader(Stream file)
        {
            byte[] buffer = new byte[4];
            if (file.Length > 68L)
            {
                file.Seek(64L, SeekOrigin.Begin);
                file.Read(buffer, 0, buffer.Length);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int)imetMagic)
                {
                    return HeaderType.ShortIMET;
                }
            }
            if (file.Length > 132L)
            {
                file.Seek(128L, SeekOrigin.Begin);
                file.Read(buffer, 0, buffer.Length);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int)imetMagic)
                {
                    return HeaderType.IMET;
                }
            }
            if (file.Length > 4L)
            {
                file.Seek(0L, SeekOrigin.Begin);
                file.Read(buffer, 0, buffer.Length);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int)imd5Magic)
                {
                    return HeaderType.IMD5;
                }
            }
            return HeaderType.None;
        }
        #endregion

        public class IMET
        {
            private bool hashesMatch = true;
            private bool isShortImet;
            private readonly byte[] additionalPadding = new byte[64];
            private readonly byte[] padding = new byte[64];
            private readonly uint imetMagic = 1229800788;
            private readonly uint sizeOfHeader = 1536;
            private uint unknown = 3;
            private uint iconSize;
            private uint bannerSize;
            private uint soundSize;
            private uint flags;
            private byte[] japaneseTitle = new byte[84];
            private byte[] englishTitle = new byte[84];
            private byte[] germanTitle = new byte[84];
            private byte[] frenchTitle = new byte[84];
            private byte[] spanishTitle = new byte[84];
            private byte[] italianTitle = new byte[84];
            private byte[] dutchTitle = new byte[84];
            private readonly byte[] unknownTitle1 = new byte[84];
            private readonly byte[] unknownTitle2 = new byte[84];
            private byte[] koreanTitle = new byte[84];
            private readonly byte[] padding2 = new byte[588];
            private byte[] hash = new byte[16];

            /// <summary>
            /// Short IMET has a padding of 64 bytes at the beginning while Long IMET has 128.
            /// </summary>
            public bool IsShortIMET
            {
                get => isShortImet;
                set => isShortImet = value;
            }

            /// <summary>
            /// The size of uncompressed icon.bin
            /// </summary>
            public uint IconSize
            {
                get => iconSize;
                set => iconSize = value;
            }

            /// <summary>
            /// The size of uncompressed banner.bin
            /// </summary>
            public uint BannerSize
            {
                get => bannerSize;
                set => bannerSize = value;
            }

            /// <summary>
            /// The size of uncompressed sound.bin
            /// </summary>
            public uint SoundSize
            {
                get => soundSize;
                set => soundSize = value;
            }

            /// <summary>
            /// The Japanese Title.
            /// </summary>
            public string JapaneseTitle
            {
                get => ReturnTitleAsString(japaneseTitle);
                set => SetTitleFromString(value, 0);
            }

            /// <summary>
            /// The English Title.
            /// </summary>
            public string EnglishTitle
            {
                get => ReturnTitleAsString(englishTitle);
                set => SetTitleFromString(value, 1);
            }

            /// <summary>
            /// The German Title.
            /// </summary>
            public string GermanTitle
            {
                get => ReturnTitleAsString(germanTitle);
                set => SetTitleFromString(value, 2);
            }

            /// <summary>
            /// The French Title.
            /// </summary>
            public string FrenchTitle
            {
                get => ReturnTitleAsString(frenchTitle);
                set => SetTitleFromString(value, 3);
            }

            /// <summary>
            /// The Spanish Title.
            /// </summary>
            public string SpanishTitle
            {
                get => ReturnTitleAsString(spanishTitle);
                set => SetTitleFromString(value, 4);
            }

            /// <summary>
            /// The Italian Title.
            /// </summary>
            public string ItalianTitle
            {
                get => ReturnTitleAsString(italianTitle);
                set => SetTitleFromString(value, 5);
            }

            /// <summary>
            /// The Dutch Title.
            /// </summary>
            public string DutchTitle
            {
                get => ReturnTitleAsString(dutchTitle);
                set => SetTitleFromString(value, 6);
            }

            /// <summary>
            /// The Korean Title.
            /// </summary>
            public string KoreanTitle
            {
                get => ReturnTitleAsString(koreanTitle);
                set => SetTitleFromString(value, 7);
            }

            /// <summary>
            /// All Titles as a string array.
            /// </summary>
            public string[] AllTitles => new string[8]
            {
                JapaneseTitle,
                EnglishTitle,
                GermanTitle,
                FrenchTitle,
                SpanishTitle,
                ItalianTitle,
                DutchTitle,
                KoreanTitle
            };

            /// <summary>
            /// When parsing an IMET header, this value will turn false if the hash stored in the header doesn't match the headers hash.
            /// </summary>
            public bool HashesMatch => hashesMatch;

            #region Public Functions
            /// <summary>
            /// Loads the IMET Header of a file.
            /// </summary>
            /// <param name="pathToFile"></param>
            /// <returns></returns>
            public static Headers.IMET Load(string pathToFile)
            {
                return Load(File.ReadAllBytes(pathToFile));
            }

            /// <summary>
            /// Loads the IMET Header of a byte array.
            /// </summary>
            /// <param name="fileOrHeader"></param>
            /// <returns></returns>
            public static Headers.IMET Load(byte[] fileOrHeader)
            {
                Headers.HeaderType headerType = DetectHeader(fileOrHeader);
                switch (headerType)
                {
                    case HeaderType.ShortIMET:
                    case HeaderType.IMET:
                        Headers.IMET imet = new Headers.IMET();
                        if (headerType == HeaderType.ShortIMET)
                        {
                            imet.isShortImet = true;
                        }

                        MemoryStream memoryStream = new MemoryStream(fileOrHeader);
                        try
                        {
                            imet.ParseHeader(memoryStream);
                        }
                        catch
                        {
                            memoryStream.Dispose();
                            throw;
                        }
                        memoryStream.Dispose();
                        return imet;
                    default:
                        throw new Exception("No IMET Header found!");
                }
            }

            /// <summary>
            /// Loads the IMET Header of a stream.
            /// </summary>
            /// <param name="fileOrHeader"></param>
            /// <returns></returns>
            public static Headers.IMET Load(Stream fileOrHeader)
            {
                Headers.HeaderType headerType = DetectHeader(fileOrHeader);
                switch (headerType)
                {
                    case HeaderType.ShortIMET:
                    case HeaderType.IMET:
                        Headers.IMET imet = new Headers.IMET();
                        if (headerType == HeaderType.ShortIMET)
                        {
                            imet.isShortImet = true;
                        }

                        imet.ParseHeader(fileOrHeader);
                        return imet;
                    default:
                        throw new Exception("No IMET Header found!");
                }
            }

            /// <summary>
            /// Creates a new IMET Header.
            /// </summary>
            /// <param name="isShortImet"></param>
            /// <param name="iconSize"></param>
            /// <param name="bannerSize"></param>
            /// <param name="soundSize"></param>
            /// <param name="titles"></param>
            /// <returns></returns>
            public static Headers.IMET Create(
                bool isShortImet,
                int iconSize,
                int bannerSize,
                int soundSize,
                params string[] titles)
            {
                Headers.IMET imet = new Headers.IMET
                {
                    isShortImet = isShortImet
                };
                for (int titleIndex = 0; titleIndex < titles.Length; ++titleIndex)
                {
                    imet.SetTitleFromString(titles[titleIndex], titleIndex);
                }

                for (int length = titles.Length; length < 8; ++length)
                {
                    imet.SetTitleFromString(titles.Length > 1 ? titles[1] : titles[0], length);
                }

                imet.iconSize = (uint)iconSize;
                imet.bannerSize = (uint)bannerSize;
                imet.soundSize = (uint)soundSize;
                return imet;
            }

            /// <summary>
            /// Removes the IMET Header of a file.
            /// </summary>
            /// <param name="pathToFile"></param>
            public static void RemoveHeader(string pathToFile)
            {
                byte[] bytes = RemoveHeader(File.ReadAllBytes(pathToFile));
                File.Delete(pathToFile);
                File.WriteAllBytes(pathToFile, bytes);
            }

            /// <summary>
            /// Removes the IMET Header of a byte array.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static byte[] RemoveHeader(byte[] file)
            {
                Headers.HeaderType headerType = DetectHeader(file);
                switch (headerType)
                {
                    case HeaderType.ShortIMET:
                    case HeaderType.IMET:
                        byte[] numArray = new byte[(int)(file.Length - headerType)];
                        Array.Copy(file, (int)headerType, numArray, 0, numArray.Length);
                        return numArray;
                    default:
                        throw new Exception("No IMET Header found!");
                }
            }

            /// <summary>
            /// Sets all title to the given string.
            /// </summary>
            /// <param name="newTitle"></param>
            public void SetAllTitles(string newTitle)
            {
                for (int titleIndex = 0; titleIndex < 10; ++titleIndex)
                {
                    SetTitleFromString(newTitle, titleIndex);
                }
            }

            /// <summary>
            /// Returns the Header as a memory stream.
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
            /// Returns the Header as a byte array.
            /// </summary>
            /// <returns></returns>
            public byte[] ToByteArray()
            {
                return ToMemoryStream().ToArray();
            }

            /// <summary>
            /// Writes the Header to the given stream.
            /// </summary>
            /// <param name="writeStream"></param>
            public void Write(Stream writeStream)
            {
                WriteToStream(writeStream);
            }

            /// <summary>
            /// Changes the Titles.
            /// </summary>
            /// <param name="newTitles"></param>
            public void ChangeTitles(params string[] newTitles)
            {
                for (int titleIndex = 0; titleIndex < newTitles.Length; ++titleIndex)
                {
                    SetTitleFromString(newTitles[titleIndex], titleIndex);
                }

                for (int length = newTitles.Length; length < 8; ++length)
                {
                    SetTitleFromString(newTitles.Length > 1 ? newTitles[1] : newTitles[0], length);
                }
            }

            /// <summary>
            /// Returns a string array with the Titles.
            /// </summary>
            /// <returns></returns>
            public string[] GetTitles()
            {
                return new string[8]
{
                JapaneseTitle,
                EnglishTitle,
                GermanTitle,
                FrenchTitle,
                SpanishTitle,
                ItalianTitle,
                DutchTitle,
                KoreanTitle
};
            }
            #endregion

            #region Private Functions
            private void WriteToStream(Stream writeStream)
            {
                writeStream.Seek(0L, SeekOrigin.Begin);
                if (!isShortImet)
                {
                    writeStream.Write(additionalPadding, 0, additionalPadding.Length);
                }

                writeStream.Write(padding, 0, padding.Length);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(imetMagic)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(sizeOfHeader)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(unknown)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(iconSize)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(bannerSize)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(soundSize)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(flags)), 0, 4);
                writeStream.Write(japaneseTitle, 0, japaneseTitle.Length);
                writeStream.Write(englishTitle, 0, englishTitle.Length);
                writeStream.Write(germanTitle, 0, germanTitle.Length);
                writeStream.Write(frenchTitle, 0, frenchTitle.Length);
                writeStream.Write(spanishTitle, 0, spanishTitle.Length);
                writeStream.Write(italianTitle, 0, italianTitle.Length);
                writeStream.Write(dutchTitle, 0, dutchTitle.Length);
                writeStream.Write(unknownTitle1, 0, unknownTitle1.Length);
                writeStream.Write(unknownTitle2, 0, unknownTitle2.Length);
                writeStream.Write(koreanTitle, 0, koreanTitle.Length);
                writeStream.Write(padding2, 0, padding2.Length);
                int position = (int)writeStream.Position;
                hash = new byte[16];
                writeStream.Write(hash, 0, hash.Length);
                byte[] numArray = new byte[writeStream.Position];
                writeStream.Seek(0L, SeekOrigin.Begin);
                writeStream.Read(numArray, 0, numArray.Length);
                ComputeHash(numArray, !isShortImet ? 64 : 0);
                writeStream.Seek(position, SeekOrigin.Begin);
                writeStream.Write(hash, 0, hash.Length);
            }

            private void ComputeHash(byte[] headerBytes, int hashPos)
            {
                MD5 md5 = MD5.Create();
                hash = md5.ComputeHash(headerBytes, hashPos, 1536);
                md5.Clear();
            }

            private void ParseHeader(Stream headerStream)
            {
                headerStream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer1 = new byte[4];
                if (!isShortImet)
                {
                    headerStream.Read(additionalPadding, 0, additionalPadding.Length);
                }

                headerStream.Read(padding, 0, padding.Length);
                headerStream.Read(buffer1, 0, 4);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int)imetMagic)
                {
                    throw new Exception("Invalid Magic!");
                }

                headerStream.Read(buffer1, 0, 4);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int)sizeOfHeader)
                {
                    throw new Exception("Invalid Header Size!");
                }

                headerStream.Read(buffer1, 0, 4);
                unknown = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                headerStream.Read(buffer1, 0, 4);
                iconSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                headerStream.Read(buffer1, 0, 4);
                bannerSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                headerStream.Read(buffer1, 0, 4);
                soundSize = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                headerStream.Read(buffer1, 0, 4);
                flags = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                headerStream.Read(japaneseTitle, 0, japaneseTitle.Length);
                headerStream.Read(englishTitle, 0, englishTitle.Length);
                headerStream.Read(germanTitle, 0, germanTitle.Length);
                headerStream.Read(frenchTitle, 0, frenchTitle.Length);
                headerStream.Read(spanishTitle, 0, spanishTitle.Length);
                headerStream.Read(italianTitle, 0, italianTitle.Length);
                headerStream.Read(dutchTitle, 0, dutchTitle.Length);
                headerStream.Read(unknownTitle1, 0, unknownTitle1.Length);
                headerStream.Read(unknownTitle2, 0, unknownTitle2.Length);
                headerStream.Read(koreanTitle, 0, koreanTitle.Length);
                headerStream.Read(padding2, 0, padding2.Length);
                headerStream.Read(this.hash, 0, this.hash.Length);
                headerStream.Seek(-16L, SeekOrigin.Current);
                headerStream.Write(new byte[16], 0, 16);
                byte[] buffer2 = new byte[headerStream.Length];
                headerStream.Seek(0L, SeekOrigin.Begin);
                headerStream.Read(buffer2, 0, buffer2.Length);
                MD5 md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(buffer2, !isShortImet ? 64 : 0, 1536);
                md5.Clear();
                hashesMatch = Shared.CompareByteArrays(hash, this.hash);
            }

            private string ReturnTitleAsString(byte[] title)
            {
                string empty = string.Empty;
                for (int index = 0; index < 84; index += 2)
                {
                    char ch = BitConverter.ToChar(new byte[2]
                    {
                        title[index + 1],
                        title[index]
                    }, 0);
                    if (ch != char.MinValue)
                    {
                        empty += ch.ToString();
                    }
                }
                return empty;
            }

            private void SetTitleFromString(string title, int titleIndex)
            {
                byte[] numArray = new byte[84];
                for (int index = 0; index < title.Length; ++index)
                {
                    byte[] bytes = BitConverter.GetBytes(title[index]);
                    numArray[index * 2 + 1] = bytes[0];
                    numArray[index * 2] = bytes[1];
                }
                switch (titleIndex)
                {
                    case 0:
                        japaneseTitle = numArray;
                        break;
                    case 1:
                        englishTitle = numArray;
                        break;
                    case 2:
                        germanTitle = numArray;
                        break;
                    case 3:
                        frenchTitle = numArray;
                        break;
                    case 4:
                        spanishTitle = numArray;
                        break;
                    case 5:
                        italianTitle = numArray;
                        break;
                    case 6:
                        dutchTitle = numArray;
                        break;
                    case 7:
                        koreanTitle = numArray;
                        break;
                }
            }
            #endregion
        }

        public class IMD5
        {
            private readonly uint imd5Magic = 1229800501;
            private uint fileSize;
            private readonly byte[] padding = new byte[8];
            private byte[] hash = new byte[16];

            /// <summary>
            /// The size of the file without the IMD5 Header.
            /// </summary>
            public uint FileSize => fileSize;

            /// <summary>
            /// The hash of the file without the IMD5 Header.
            /// </summary>
            public byte[] Hash => hash;

            private IMD5()
            {
            }

            #region Public Functions
            /// <summary>
            /// Loads the IMD5 Header of a file.
            /// </summary>
            /// <param name="pathToFile"></param>
            /// <returns></returns>
            public static Headers.IMD5 Load(string pathToFile)
            {
                return Load(File.ReadAllBytes(pathToFile));
            }

            /// <summary>
            /// Loads the IMD5 Header of a byte array.
            /// </summary>
            /// <param name="fileOrHeader"></param>
            /// <returns></returns>
            public static Headers.IMD5 Load(byte[] fileOrHeader)
            {
                if (DetectHeader(fileOrHeader) != HeaderType.IMD5)
                {
                    throw new Exception("No IMD5 Header found!");
                }

                Headers.IMD5 imD5 = new Headers.IMD5();
                MemoryStream memoryStream = new MemoryStream(fileOrHeader);
                try
                {
                    imD5.ParseHeader(memoryStream);
                }
                catch
                {
                    memoryStream.Dispose();
                    throw;
                }
                memoryStream.Dispose();
                return imD5;
            }

            /// <summary>
            /// Loads the IMD5 Header of a stream.
            /// </summary>
            /// <param name="fileOrHeader"></param>
            /// <returns></returns>
            public static Headers.IMD5 Load(Stream fileOrHeader)
            {
                if (DetectHeader(fileOrHeader) != HeaderType.IMD5)
                {
                    throw new Exception("No IMD5 Header found!");
                }

                Headers.IMD5 imD5 = new Headers.IMD5();
                imD5.ParseHeader(fileOrHeader);
                return imD5;
            }

            /// <summary>
            /// Creates a new IMD5 Header.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static Headers.IMD5 Create(byte[] file)
            {
                IMD5 imD5 = new IMD5
                {
                    fileSize = (uint)file.Length
                };
                imD5.ComputeHash(file);
                return imD5;
            }

            /// <summary>
            /// Adds an IMD5 Header to a file.
            /// </summary>
            /// <param name="pathToFile"></param>
            public static void AddHeader(string pathToFile)
            {
                byte[] buffer = AddHeader(File.ReadAllBytes(pathToFile));
                File.Delete(pathToFile);
                using FileStream fileStream = new FileStream(pathToFile, FileMode.Create);
                fileStream.Write(buffer, 0, buffer.Length);
            }

            /// <summary>
            /// Adds an IMD5 Header to a byte array.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static byte[] AddHeader(byte[] file)
            {
                Headers.IMD5 imD5 = Create(file);
                MemoryStream memoryStream1 = new MemoryStream();
                MemoryStream memoryStream2 = memoryStream1;
                imD5.WriteToStream(memoryStream2);
                memoryStream1.Write(file, 0, file.Length);
                byte[] array = memoryStream1.ToArray();
                memoryStream1.Dispose();
                return array;
            }

            /// <summary>
            /// Removes the IMD5 Header of a file.
            /// </summary>
            /// <param name="pathToFile"></param>
            public static void RemoveHeader(string pathToFile)
            {
                byte[] buffer = RemoveHeader(File.ReadAllBytes(pathToFile));
                File.Delete(pathToFile);
                using FileStream fileStream = new FileStream(pathToFile, FileMode.Create);
                fileStream.Write(buffer, 0, buffer.Length);
            }

            /// <summary>
            /// Removes the IMD5 Header of a byte array.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static byte[] RemoveHeader(byte[] file)
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(file, 32, file.Length - 32);
                byte[] array = memoryStream.ToArray();
                memoryStream.Dispose();
                return array;
            }

            /// <summary>
            /// Returns the IMD5 Header as a memory stream.
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
            /// Returns the IMD5 Header as a byte array.
            /// </summary>
            /// <returns></returns>
            public byte[] ToByteArray()
            {
                return ToMemoryStream().ToArray();
            }

            /// <summary>
            /// Writes the IMD5 Header to the given stream.
            /// </summary>
            /// <param name="writeStream"></param>
            public void Write(Stream writeStream)
            {
                WriteToStream(writeStream);
            }
            #endregion

            #region Private Functions
            private void WriteToStream(Stream writeStream)
            {
                writeStream.Seek(0L, SeekOrigin.Begin);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(imd5Magic)), 0, 4);
                writeStream.Write(BitConverter.GetBytes(Shared.Swap(fileSize)), 0, 4);
                writeStream.Write(padding, 0, padding.Length);
                writeStream.Write(hash, 0, hash.Length);
            }

            private void ComputeHash(byte[] bytesToHash)
            {
                MD5 md5 = MD5.Create();
                hash = md5.ComputeHash(bytesToHash);
                md5.Clear();
            }

            private void ParseHeader(Stream headerStream)
            {
                headerStream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer = new byte[4];
                headerStream.Read(buffer, 0, 4);
                if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) != (int)imd5Magic)
                {
                    throw new Exception("Invalid Magic!");
                }

                headerStream.Read(buffer, 0, 4);
                fileSize = Shared.Swap(BitConverter.ToUInt32(buffer, 0));
                headerStream.Read(padding, 0, padding.Length);
                headerStream.Read(hash, 0, hash.Length);
            }
            #endregion
        }
    }
}
