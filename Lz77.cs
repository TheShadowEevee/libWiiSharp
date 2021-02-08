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

namespace libWiiSharp
{
    public class Lz77
    {
        //private const int N = 4096;
        //private const int F = 18;
        //private const int threshold = 2;
        private static readonly uint lz77Magic = 1280980791;
        private readonly int[] leftSon = new int[4097];
        private readonly int[] rightSon = new int[4353];
        private readonly int[] dad = new int[4097];
        private readonly ushort[] textBuffer = new ushort[4113];
        private int matchPosition;
        private int matchLength;

        /// <summary>
        /// Lz77 Magic.
        /// </summary>
        public static uint Lz77Magic => lz77Magic;

        #region Public Functions
        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(string file)
        {
            return IsLz77Compressed(File.ReadAllBytes(file));
        }

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(byte[] file)
        {
            Headers.HeaderType headerType = Headers.DetectHeader(file);
            return (int)Shared.Swap(BitConverter.ToUInt32(file, (int)headerType)) == (int)lz77Magic;
        }

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(Stream file)
        {
            Headers.HeaderType headerType = Headers.DetectHeader(file);
            byte[] buffer = new byte[4];
            file.Seek((long)headerType, SeekOrigin.Begin);
            file.Read(buffer, 0, buffer.Length);
            return (int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) == (int)lz77Magic;
        }

        /// <summary>
        /// Compresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Compress(string inFile, string outFile)
        {
            Stream stream = null;
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open))
            {
                stream = PrivCompress(fileStream);
            }

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            using (FileStream fileStream = new FileStream(outFile, FileMode.Create))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Compresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] file)
        {
            return ((MemoryStream)PrivCompress(new MemoryStream(file))).ToArray();
        }

        /// <summary>
        /// Compresses the stream using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Stream Compress(Stream file)
        {
            return PrivCompress(file);
        }

        /// <summary>
        /// Decompresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Decompress(string inFile, string outFile)
        {
            Stream stream = null;
            using (FileStream fileStream = new FileStream(inFile, FileMode.Open))
            {
                stream = PrivDecompress(fileStream);
            }

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            using (FileStream fileStream = new FileStream(outFile, FileMode.Create))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Decompresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Decompress(byte[] file)
        {
            return ((MemoryStream)PrivDecompress(new MemoryStream(file))).ToArray();
        }

        public Stream Decompress(Stream file)
        {
            return PrivDecompress(file);
        }
        #endregion

        #region Private Functions
        private Stream PrivDecompress(Stream inFile)
        {
            if (!IsLz77Compressed(inFile))
            {
                return inFile;
            }

            inFile.Seek(0L, SeekOrigin.Begin);
            uint num1 = 0;
            Headers.HeaderType headerType = Headers.DetectHeader(inFile);
            byte[] buffer = new byte[8];
            inFile.Seek((long)headerType, SeekOrigin.Begin);
            inFile.Read(buffer, 0, 8);
            if ((int)Shared.Swap(BitConverter.ToUInt32(buffer, 0)) != (int)lz77Magic)
            {
                inFile.Dispose();
                throw new Exception("Invaild Magic!");
            }
            if (buffer[4] != 16)
            {
                inFile.Dispose();
                throw new Exception("Unsupported Compression Type!");
            }
            uint num2 = BitConverter.ToUInt32(buffer, 4) >> 8;
            for (int index = 0; index < 4078; ++index)
            {
                textBuffer[index] = 223;
            }

            int num3 = 4078;
            uint num4 = 7;
            int num5 = 7;
            MemoryStream memoryStream = new MemoryStream();
        label_10:
            while (true)
            {
                num4 <<= 1;
                ++num5;
                if (num5 == 8)
                {
                    int num6;
                    if ((num6 = inFile.ReadByte()) != -1)
                    {
                        num4 = (uint)num6;
                        num5 = 0;
                    }
                    else
                    {
                        goto label_24;
                    }
                }
                if (((int)num4 & 128) == 0)
                {
                    int num6;
                    if ((num6 = inFile.ReadByte()) != inFile.Length - 1L)
                    {
                        if (num1 < num2)
                        {
                            memoryStream.WriteByte((byte)num6);
                        }

                        ushort[] textBuffer = this.textBuffer;
                        int index = num3;
                        int num7 = index + 1;
                        int num8 = (byte)num6;
                        textBuffer[index] = (ushort)num8;
                        num3 = num7 & 4095;
                        ++num1;
                    }
                    else
                    {
                        goto label_24;
                    }
                }
                else
                {
                    break;
                }
            }
            int num9;
            int num10;
            if ((num9 = inFile.ReadByte()) != -1 && (num10 = inFile.ReadByte()) != -1)
            {
                int num6 = num10 | num9 << 8 & 3840;
                int num7 = (num9 >> 4 & 15) + 2;
                for (int index1 = 0; index1 <= num7; ++index1)
                {
                    int num8 = this.textBuffer[num3 - num6 - 1 & 4095];
                    if (num1 < num2)
                    {
                        memoryStream.WriteByte((byte)num8);
                    }

                    ushort[] textBuffer = this.textBuffer;
                    int index2 = num3;
                    int num11 = index2 + 1;
                    int num12 = (byte)num8;
                    textBuffer[index2] = (ushort)num12;
                    num3 = num11 & 4095;
                    ++num1;
                }
                goto label_10;
            }
        label_24:
            return memoryStream;
        }

        private Stream PrivCompress(Stream inFile)
        {
            if (Lz77.IsLz77Compressed(inFile))
                return inFile;
            inFile.Seek(0L, SeekOrigin.Begin);
            int num1 = 0;
            int[] numArray1 = new int[17];
            uint num2 = (uint)(((int)Convert.ToUInt32(inFile.Length) << 8) + 16);
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(BitConverter.GetBytes(Shared.Swap(Lz77.lz77Magic)), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(num2), 0, 4);
            this.InitTree();
            numArray1[0] = 0;
            int num3 = 1;
            int num4 = 128;
            int p = 0;
            int r = 4078;
            for (int index = p; index < r; ++index)
                this.textBuffer[index] = ushort.MaxValue;
            int num5;
            int num6;
            for (num5 = 0; num5 < 18 && (num6 = inFile.ReadByte()) != -1; ++num5)
                this.textBuffer[r + num5] = (ushort)num6;
            if (num5 == 0)
                return inFile;
            for (int index = 1; index <= 18; ++index)
                this.InsertNode(r - index);
            this.InsertNode(r);
            do
            {
                if (this.matchLength > num5)
                    this.matchLength = num5;
                if (this.matchLength <= 2)
                {
                    this.matchLength = 1;
                    numArray1[num3++] = (int)this.textBuffer[r];
                }
                else
                {
                    numArray1[0] |= num4;
                    int[] numArray2 = numArray1;
                    int index1 = num3;
                    int num7 = index1 + 1;
                    int num8 = (int)(ushort)(r - this.matchPosition - 1 >> 8 & 15) | this.matchLength - 3 << 4;
                    numArray2[index1] = num8;
                    int[] numArray3 = numArray1;
                    int index2 = num7;
                    num3 = index2 + 1;
                    int num9 = (int)(ushort)(r - this.matchPosition - 1 & (int)byte.MaxValue);
                    numArray3[index2] = num9;
                }
                if ((num4 >>= 1) == 0)
                {
                    for (int index = 0; index < num3; ++index)
                        memoryStream.WriteByte((byte)numArray1[index]);
                    num1 += num3;
                    numArray1[0] = 0;
                    num3 = 1;
                    num4 = 128;
                }
                int matchLength = this.matchLength;
                int num10;
                int num11;
                for (num10 = 0; num10 < matchLength && (num11 = inFile.ReadByte()) != -1; ++num10)
                {
                    this.DeleteNode(p);
                    this.textBuffer[p] = (ushort)num11;
                    if (p < 17)
                        this.textBuffer[p + 4096] = (ushort)num11;
                    p = p + 1 & 4095;
                    r = r + 1 & 4095;
                    this.InsertNode(r);
                }
                while (num10++ < matchLength)
                {
                    this.DeleteNode(p);
                    p = p + 1 & 4095;
                    r = r + 1 & 4095;
                    if (--num5 != 0)
                        this.InsertNode(r);
                }
            }
            while (num5 > 0);
            if (num3 > 1)
            {
                for (int index = 0; index < num3; ++index)
                    memoryStream.WriteByte((byte)numArray1[index]);
                num1 += num3;
            }
            if (num1 % 4 != 0)
            {
                for (int index = 0; index < 4 - num1 % 4; ++index)
                    memoryStream.WriteByte((byte)0);
            }
            return (Stream)memoryStream;
        }

        private void InitTree()
        {
            for (int index = 4097; index <= 4352; ++index)
                this.rightSon[index] = 4096;
            for (int index = 0; index < 4096; ++index)
                this.dad[index] = 4096;
        }
        private void InsertNode(int r)
        {
            int num1 = 1;
            int index = 4097 + (this.textBuffer[r] != ushort.MaxValue ? (int)this.textBuffer[r] : 0);
            this.rightSon[r] = this.leftSon[r] = 4096;
            this.matchLength = 0;
            int num2;
            do
            {
                do
                {
                    if (num1 >= 0)
                    {
                        if (this.rightSon[index] == 4096)
                        {
                            this.rightSon[index] = r;
                            this.dad[r] = index;
                            return;
                        }
                        index = this.rightSon[index];
                    }
                    else
                    {
                        if (this.leftSon[index] == 4096)
                        {
                            this.leftSon[index] = r;
                            this.dad[r] = index;
                            return;
                        }
                        index = this.leftSon[index];
                    }
                    num2 = 1;
                    while (num2 < 18 && (num1 = (int)this.textBuffer[r + num2] - (int)this.textBuffer[index + num2]) == 0)
                        ++num2;
                }
                while (num2 <= this.matchLength);
                this.matchPosition = index;
            }
            while ((this.matchLength = num2) < 18);
            this.dad[r] = this.dad[index];
            this.leftSon[r] = this.leftSon[index];
            this.rightSon[r] = this.rightSon[index];
            this.dad[this.leftSon[index]] = r;
            this.dad[this.rightSon[index]] = r;
            if (this.rightSon[this.dad[index]] == index)
                this.rightSon[this.dad[index]] = r;
            else
                this.leftSon[this.dad[index]] = r;
            this.dad[index] = 4096;
        }
        private void DeleteNode(int p)
        {
            if (this.dad[p] == 4096)
                return;
            int index;
            if (this.rightSon[p] == 4096)
                index = this.leftSon[p];
            else if (this.leftSon[p] == 4096)
            {
                index = this.rightSon[p];
            }
            else
            {
                index = this.leftSon[p];
                if (this.rightSon[index] != 4096)
                {
                    do
                    {
                        index = this.rightSon[index];
                    }
                    while (this.rightSon[index] != 4096);
                    this.rightSon[this.dad[index]] = this.leftSon[index];
                    this.dad[this.leftSon[index]] = this.dad[index];
                    this.leftSon[index] = this.leftSon[p];
                    this.dad[this.leftSon[p]] = index;
                }
                this.rightSon[index] = this.rightSon[p];
                this.dad[this.rightSon[p]] = index;
            }
            this.dad[index] = this.dad[p];
            if (this.rightSon[this.dad[p]] == p)
                this.rightSon[this.dad[p]] = index;
            else
                this.leftSon[this.dad[p]] = index;
            this.dad[p] = 4096;
        }
        #endregion
    }
}
