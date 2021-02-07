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

namespace libWiiSharp
{
    public class BNS : IDisposable
    {

        private BNS_Header bnsHeader = new BNS_Header();
        private BNS_Info bnsInfo = new BNS_Info();
        private BNS_Data bnsData = new BNS_Data();
        // Unused
        //private int[,] lSamples = new int[2, 2];
        private int[,] rlSamples = new int[2, 2];
        private int[] tlSamples = new int[2];
        // Unused
        /*
        private int[] hbcDefTbl = new int[16]
        {
            674,
            1040,
            3598,
            -1738,
            2270,
            -583,
            3967,
            -1969,
            1516,
            381,
            3453,
            -1468,
            2606,
            -617,
            3795,
            -1759
        };
        */
        private readonly int[] defTbl = new int[16]
        {
                1820,
                -856,
                3238,
                -1514,
                2333,
                -550,
                3336,
                -1376,
                2444,
                -949,
                3666,
                -1764,
                2654,
                -701,
                3420,
                -1398
        };
        private int[] pHist1 = new int[2];
        private int[] pHist2 = new int[2];
        private int tempSampleCount;
        private byte[] waveFile;
        private readonly bool loopFromWave;
        private bool converted;
        private bool toMono;
        private bool isDisposed;

        /// <summary>
        /// 0x00 (0) = No Loop, 0x01 (1) = Loop
        /// </summary>
        public bool HasLoop
        {
            get => bnsInfo.HasLoop == 1;
            set => bnsInfo.HasLoop = value ? 1 : 0;
        }

        /// <summary>
        /// The start sample of the Loop
        /// </summary>
        public uint LoopStart
        {
            get => bnsInfo.LoopStart;
            set => bnsInfo.LoopStart = value;
        }

        /// <summary>
        /// The total number of samples in this file
        /// </summary>
        public uint TotalSampleCount
        {
            get => bnsInfo.LoopEnd;
            set => bnsInfo.LoopEnd = value;
        }

        /// <summary>
        /// If true and the input Wave file is stereo, the BNS will be converted to Mono.
        /// Be sure to set this before you call Convert()!
        /// </summary>
        public bool StereoToMono
        {
            get => toMono;
            set => toMono = value;
        }

        public event EventHandler<ProgressChangedEventArgs> Progress;

        protected BNS()
        {

        }

        public BNS(string waveFile)
        {
            this.waveFile = File.ReadAllBytes(waveFile);
        }

        public BNS(string waveFile, bool loopFromWave)
        {
            this.waveFile = File.ReadAllBytes(waveFile);
            this.loopFromWave = loopFromWave;
        }

        public BNS(byte[] waveFile)
        {
            this.waveFile = waveFile;
        }

        public BNS(byte[] waveFile, bool loopFromWave)
        {
            this.waveFile = waveFile;
            this.loopFromWave = loopFromWave;
        }

        #region IDisposable Members

        ~BNS() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                bnsHeader = null;
                bnsInfo = null;
                bnsData = null;
                //this.lSamples = (int[,])null;
                rlSamples = null;
                tlSamples = null;
                //this.hbcDefTbl = (int[])null;
                pHist1 = null;
                pHist2 = null;
                waveFile = null;
            }

            isDisposed = true;

        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Returns the length of the BNS audio file in seconds
        /// </summary>
        /// <param name="bnsFile"></param>
        /// <returns></returns>
        public static int GetBnsLength(byte[] bnsFile)
        {
            uint sampleRate = Shared.Swap(BitConverter.ToUInt16(bnsFile, 44));
            uint sampleCount = Shared.Swap(BitConverter.ToUInt32(bnsFile, 52));

            return (int)(sampleCount / sampleRate);
        }

        /// <summary>
        /// Converts the Wave file to BNS
        /// </summary>
        public void Convert()
        {
            Convert(waveFile, loopFromWave);
        }

        /// <summary>
        /// Returns the BNS file as a Byte Array. If not already converted, it will be done first.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ToMemoryStream().ToArray();
        }

        /// <summary>
        /// Returns the BNS file as a Memory Stream. If not already converted, it will be done first.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToMemoryStream()
        {
            if (!converted)
            {
                Convert(waveFile, loopFromWave);
            }

            MemoryStream memoryStream = new MemoryStream();
            try
            {
                bnsHeader.Write(memoryStream);
                bnsInfo.Write(memoryStream);
                bnsData.Write(memoryStream);
                return memoryStream;
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Saves the BNS file to the given path. If not already converted, it will be done first.
        /// </summary>
        /// <param name="destinationFile"></param>
        public void Save(string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            using FileStream fileStream = new FileStream(destinationFile, FileMode.Create);
            byte[] array = ToMemoryStream().ToArray();
            fileStream.Write(array, 0, array.Length);
        }

        /// <summary>
        /// Sets the Loop to the given Start Sample. Be sure that you call Convert() first!
        /// </summary>
        /// <param name="loopStartSample"></param>
        public void SetLoop(int loopStartSample)
        {
            bnsInfo.HasLoop = 1;
            bnsInfo.LoopStart = (uint)loopStartSample;
        }

        #endregion

        #region Private Functions

        private void Convert(byte[] waveFile, bool loopFromWave)
        {
            Wave wave = new Wave(waveFile);
            int numLoops = wave.NumLoops;
            int loopStart = wave.LoopStart;
            bnsInfo.ChannelCount = (byte)wave.NumChannels;
            bnsInfo.SampleRate = (ushort)wave.SampleRate;
            if (bnsInfo.ChannelCount > 2 || bnsInfo.ChannelCount < 1)
            {
                throw new Exception("Unsupported Amount of Channels!");
            }

            if (wave.BitDepth != 16)
            {
                throw new Exception("Only 16bit Wave files are supported!");
            }

            bnsData.Data = wave.DataFormat == 1 ? Encode(wave.SampleData) : throw new Exception("The format of this Wave file is not supported!");
            if (bnsInfo.ChannelCount == 1)
            {
                bnsHeader.InfoLength = 96U;
                bnsHeader.DataOffset = 128U;
                bnsInfo.Size = 96U;
                bnsInfo.Channel1StartOffset = 28U;
                bnsInfo.Channel2StartOffset = 0U;
                bnsInfo.Channel1Start = 40U;
                bnsInfo.Coefficients1Offset = 0U;
            }
            bnsData.Size = (uint)(bnsData.Data.Length + 8);
            bnsHeader.DataLength = bnsData.Size;
            bnsHeader.FileSize = bnsHeader.Size + bnsInfo.Size + bnsData.Size;
            if (loopFromWave && numLoops == 1 && loopStart != -1)
            {
                bnsInfo.LoopStart = (uint)loopStart;
                bnsInfo.HasLoop = 1;
            }
            bnsInfo.LoopEnd = (uint)tempSampleCount;
            for (int index = 0; index < 16; ++index)
            {
                bnsInfo.Coefficients1[index] = defTbl[index];
                if (bnsInfo.ChannelCount == 2)
                {
                    bnsInfo.Coefficients2[index] = defTbl[index];
                }
            }
            converted = true;
        }

        private byte[] Encode(byte[] inputFrames)
        {
            int[] inputBuffer = new int[14];
            tempSampleCount = inputFrames.Length / (bnsInfo.ChannelCount == 2 ? 4 : 2);
            int num1 = inputFrames.Length / (bnsInfo.ChannelCount == 2 ? 4 : 2) % 14;
            Array.Resize<byte>(ref inputFrames, inputFrames.Length + (14 - num1) * (bnsInfo.ChannelCount == 2 ? 4 : 2));
            int num2 = inputFrames.Length / (bnsInfo.ChannelCount == 2 ? 4 : 2);
            int num3 = (num2 + 13) / 14;
            List<int> intList1 = new List<int>();
            List<int> intList2 = new List<int>();
            int startIndex = 0;
            if (toMono && bnsInfo.ChannelCount == 2)
            {
                bnsInfo.ChannelCount = 1;
            }
            else if (toMono)
            {
                toMono = false;
            }

            for (int index = 0; index < num2; ++index)
            {
                intList1.Add(BitConverter.ToInt16(inputFrames, startIndex));
                startIndex += 2;
                if (bnsInfo.ChannelCount == 2 || toMono)
                {
                    intList2.Add(BitConverter.ToInt16(inputFrames, startIndex));
                    startIndex += 2;
                }
            }
            byte[] numArray1 = new byte[bnsInfo.ChannelCount == 2 ? num3 * 16 : num3 * 8];
            int num4 = 0;
            int num5 = num3 * 8;
            bnsInfo.Channel2Start = bnsInfo.ChannelCount == 2 ? (uint)num5 : 0U;
            int[] array1 = intList1.ToArray();
            int[] array2 = intList2.ToArray();
            for (int index1 = 0; index1 < num3; ++index1)
            {
                try
                {
                    if (index1 % (num3 / 100) != 0)
                    {
                        if (index1 + 1 != num3)
                        {
                            goto label_14;
                        }
                    }
                    ChangeProgress((index1 + 1) * 100 / num3);
                }
                catch
                {
                }
            label_14:
                for (int index2 = 0; index2 < 14; ++index2)
                {
                    inputBuffer[index2] = array1[index1 * 14 + index2];
                }

                byte[] numArray2 = RepackAdpcm(0, defTbl, inputBuffer);
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    numArray1[num4 + index2] = numArray2[index2];
                }

                num4 += 8;
                if (bnsInfo.ChannelCount == 2)
                {
                    for (int index2 = 0; index2 < 14; ++index2)
                    {
                        inputBuffer[index2] = array2[index1 * 14 + index2];
                    }

                    byte[] numArray3 = RepackAdpcm(1, defTbl, inputBuffer);
                    for (int index2 = 0; index2 < 8; ++index2)
                    {
                        numArray1[num5 + index2] = numArray3[index2];
                    }

                    num5 += 8;
                }
            }
            bnsInfo.LoopEnd = (uint)(num3 * 7);
            return numArray1;
        }

        private byte[] RepackAdpcm(int index, int[] table, int[] inputBuffer)
        {
            byte[] numArray1 = new byte[8];
            int[] numArray2 = new int[2];
            double num1 = 999999999.0;
            for (int tableIndex = 0; tableIndex < 8; ++tableIndex)
            {
                byte[] numArray3 = CompressAdpcm(index, table, tableIndex, inputBuffer, out double outError);
                if (outError < num1)
                {
                    num1 = outError;
                    for (int index1 = 0; index1 < 8; ++index1)
                    {
                        numArray1[index1] = numArray3[index1];
                    }

                    for (int index1 = 0; index1 < 2; ++index1)
                    {
                        numArray2[index1] = tlSamples[index1];
                    }
                }
            }
            for (int index1 = 0; index1 < 2; ++index1)
            {
                int[,] rlSamples = this.rlSamples;
                int num2 = index1;
                int index2 = index;
                int index3 = num2;
                int num3 = numArray2[index1];
                rlSamples[index2, index3] = num3;
            }
            return numArray1;
        }

        private byte[] CompressAdpcm(
            int index,
            int[] table,
            int tableIndex,
            int[] inputBuffer,
            out double outError)
        {
            byte[] numArray = new byte[8];
            int num1 = 0;
            int num2 = table[2 * tableIndex];
            int num3 = table[2 * tableIndex + 1];
            int stdExponent = DetermineStdExponent(index, table, tableIndex, inputBuffer);
            while (stdExponent <= 15)
            {
                bool flag = false;
                num1 = 0;
                numArray[0] = (byte)(stdExponent | tableIndex << 4);
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    tlSamples[index1] = rlSamples[index, index1];
                }

                int num4 = 0;
                for (int index1 = 0; index1 < 14; ++index1)
                {
                    int num5 = tlSamples[1] * num2 + tlSamples[0] * num3 >> 11;
                    int input1 = inputBuffer[index1] - num5 >> stdExponent;
                    if (input1 <= 7 && input1 >= -8)
                    {
                        int num6 = Clamp(input1, -8, 7);
                        numArray[index1 / 2 + 1] = (index1 & 1) == 0 ? (byte)(num6 << 4) : (byte)(numArray[index1 / 2 + 1] | (uint)(num6 & 15));
                        int input2 = num5 + (num6 << stdExponent);
                        tlSamples[0] = tlSamples[1];
                        tlSamples[1] = Clamp(input2, short.MinValue, short.MaxValue);
                        num1 += (int)Math.Pow(tlSamples[1] - inputBuffer[index1], 2.0);
                    }
                    else
                    {
                        ++stdExponent;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    num4 = 14;
                }

                if (num4 == 14)
                {
                    break;
                }
            }
            outError = num1;
            return numArray;
        }

        private int DetermineStdExponent(int index, int[] table, int tableIndex, int[] inputBuffer)
        {
            int[] numArray = new int[2];
            int num1 = 0;
            int num2 = table[2 * tableIndex];
            int num3 = table[2 * tableIndex + 1];
            for (int index1 = 0; index1 < 2; ++index1)
            {
                numArray[index1] = rlSamples[index, index1];
            }

            for (int index1 = 0; index1 < 14; ++index1)
            {
                int num4 = numArray[1] * num2 + numArray[0] * num3 >> 11;
                int num5 = inputBuffer[index1] - num4;
                if (num5 > num1)
                {
                    num1 = num5;
                }

                numArray[0] = numArray[1];
                numArray[1] = inputBuffer[index1];
            }
            return FindExponent(num1);
        }

        private int FindExponent(double residual)
        {
            int num = 0;
            for (; residual > 7.5 || residual < -8.5; residual /= 2.0)
            {
                ++num;
            }

            return num;
        }

        private int Clamp(int input, int min, int max)
        {
            if (input < min)
            {
                return min;
            }

            return input > max ? max : input;
        }

        private void ChangeProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = Progress;
            if (progress == null)
            {
                return;
            }

            progress(new object(), new ProgressChangedEventArgs(progressPercentage, new object()));
        }

        #endregion

        #region BNS to Wave

        #region Public Functions

        /// <summary>
        /// Converts a BNS audio file to Wave format.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        public static Wave BnsToWave(Stream inputFile)
        {
            BNS bns = new BNS();
            byte[] samples = bns.Read(inputFile);
            Wave wave = new Wave(bns.bnsInfo.ChannelCount, 16, bns.bnsInfo.SampleRate, samples);
            if (bns.bnsInfo.HasLoop == 1)
            {
                wave.AddLoop((int)bns.bnsInfo.LoopStart);
            }

            return wave;
        }

        public static Wave BnsToWave(string pathToFile)
        {
            BNS bns = new BNS();
            byte[] samples = null;
            using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open))
            {
                samples = bns.Read(fileStream);
            }

            Wave wave = new Wave(bns.bnsInfo.ChannelCount, 16, bns.bnsInfo.SampleRate, samples);
            if (bns.bnsInfo.HasLoop == 1)
            {
                wave.AddLoop((int)bns.bnsInfo.LoopStart);
            }

            return wave;
        }

        public static Wave BnsToWave(byte[] bnsFile)
        {
            BNS bns = new BNS();
            byte[] samples = null;
            using (MemoryStream memoryStream = new MemoryStream(bnsFile))
            {
                samples = bns.Read(memoryStream);
            }

            Wave wave = new Wave(bns.bnsInfo.ChannelCount, 16, bns.bnsInfo.SampleRate, samples);
            if (bns.bnsInfo.HasLoop == 1)
            {
                wave.AddLoop((int)bns.bnsInfo.LoopStart);
            }

            return wave;
        }

        #endregion

        #region Private Functions

        private byte[] Read(Stream input)
        {
            input.Seek(0L, SeekOrigin.Begin);
            bnsHeader.Read(input);
            bnsInfo.Read(input);
            bnsData.Read(input);
            return Decode();
        }

        private byte[] Decode()
        {
            List<byte> byteList = new List<byte>();
            int num = bnsData.Data.Length / (bnsInfo.ChannelCount == 2 ? 16 : 8);
            int dataOffset1 = 0;
            int dataOffset2 = num * 8;
            //byte[] numArray1 = new byte[0];
            byte[] numArray2 = new byte[0];
            for (int index1 = 0; index1 < num; ++index1)
            {
                byte[] numArray3 = DecodeAdpcm(0, dataOffset1);
                if (bnsInfo.ChannelCount == 2)
                {
                    numArray2 = DecodeAdpcm(1, dataOffset2);
                }

                for (int index2 = 0; index2 < 14; ++index2)
                {
                    byteList.Add(numArray3[index2 * 2]);
                    byteList.Add(numArray3[index2 * 2 + 1]);
                    if (bnsInfo.ChannelCount == 2)
                    {
                        byteList.Add(numArray2[index2 * 2]);
                        byteList.Add(numArray2[index2 * 2 + 1]);
                    }
                }
                dataOffset1 += 8;
                if (bnsInfo.ChannelCount == 2)
                {
                    dataOffset2 += 8;
                }
            }
            return byteList.ToArray();
        }

        private byte[] DecodeAdpcm(int channel, int dataOffset)
        {
            byte[] numArray = new byte[28];
            int num1 = bnsData.Data[dataOffset] >> 4 & 15;
            int num2 = 1 << (bnsData.Data[dataOffset] & 15);
            int num3 = pHist1[channel];
            int num4 = pHist2[channel];
            int num5 = channel == 0 ? bnsInfo.Coefficients1[num1 * 2] : bnsInfo.Coefficients2[num1 * 2];
            int num6 = channel == 0 ? bnsInfo.Coefficients1[num1 * 2 + 1] : bnsInfo.Coefficients2[num1 * 2 + 1];
            for (int index = 0; index < 14; ++index)
            {
                short num7 = bnsData.Data[dataOffset + (index / 2 + 1)];
                int num8 = (index & 1) != 0 ? num7 & 15 : num7 >> 4;
                if (num8 >= 8)
                {
                    num8 -= 16;
                }

                int num9 = Clamp((num2 * num8 << 11) + (num5 * num3 + num6 * num4) + 1024 >> 11, short.MinValue, short.MaxValue);
                numArray[index * 2] = (byte)((uint)(short)num9 & byte.MaxValue);
                numArray[index * 2 + 1] = (byte)((uint)(short)num9 >> 8);
                num4 = num3;
                num3 = num9;
            }
            pHist1[channel] = num3;
            pHist2[channel] = num4;
            return numArray;
        }
        #endregion
        #endregion
    }
    internal class BNS_Data
    {
        private readonly byte[] magic = new byte[4]
        {
             68,
             65,
             84,
             65
        };
        private uint size = 315392;
        private byte[] data;

        public uint Size
        {
            get => size;
            set => size = value;
        }

        public byte[] Data
        {
            get => data;
            set => data = value;
        }

        public void Write(Stream outStream)
        {
            byte[] bytes = BitConverter.GetBytes(Shared.Swap(size));
            outStream.Write(magic, 0, magic.Length);
            outStream.Write(bytes, 0, bytes.Length);
            outStream.Write(data, 0, data.Length);
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            size = Shared.CompareByteArrays(magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
            data = binaryReader.ReadBytes((int)size - 8);
        }
    }
    internal class BNS_Header
    {
        private readonly byte[] magic = new byte[4]
            {
                 66,
                 78,
                 83,
                 32
            };
        private uint flags = 4278124800;
        private uint fileSize = 315584;
        private ushort size = 32;
        private ushort chunkCount = 2;
        private uint infoOffset = 32;
        private uint infoLength = 160;
        private uint dataOffset = 192;
        private uint dataLength = 315392;

        public uint DataOffset
        {
            get => dataOffset;
            set => dataOffset = value;
        }

        public uint InfoLength
        {
            get => infoLength;
            set => infoLength = value;
        }

        public ushort Size
        {
            get => size;
            set => size = value;
        }

        public uint DataLength
        {
            get => dataLength;
            set => dataLength = value;
        }

        public uint FileSize
        {
            get => fileSize;
            set => fileSize = value;
        }

        public void Write(Stream outStream)
        {
            outStream.Write(magic, 0, magic.Length);
            byte[] bytes1 = BitConverter.GetBytes(flags);
            Array.Reverse(bytes1);
            outStream.Write(bytes1, 0, bytes1.Length);
            byte[] bytes2 = BitConverter.GetBytes(fileSize);
            Array.Reverse(bytes2);
            outStream.Write(bytes2, 0, bytes2.Length);
            byte[] bytes3 = BitConverter.GetBytes(size);
            Array.Reverse(bytes3);
            outStream.Write(bytes3, 0, bytes3.Length);
            byte[] bytes4 = BitConverter.GetBytes(chunkCount);
            Array.Reverse(bytes4);
            outStream.Write(bytes4, 0, bytes4.Length);
            byte[] bytes5 = BitConverter.GetBytes(infoOffset);
            Array.Reverse(bytes5);
            outStream.Write(bytes5, 0, bytes5.Length);
            byte[] bytes6 = BitConverter.GetBytes(infoLength);
            Array.Reverse(bytes6);
            outStream.Write(bytes6, 0, bytes6.Length);
            byte[] bytes7 = BitConverter.GetBytes(dataOffset);
            Array.Reverse(bytes7);
            outStream.Write(bytes7, 0, bytes7.Length);
            byte[] bytes8 = BitConverter.GetBytes(dataLength);
            Array.Reverse(bytes8);
            outStream.Write(bytes8, 0, bytes8.Length);
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            if (!Shared.CompareByteArrays(magic, binaryReader.ReadBytes(4)))
            {
                binaryReader.BaseStream.Seek(28L, SeekOrigin.Current);
                if (!Shared.CompareByteArrays(magic, binaryReader.ReadBytes(4)))
                {
                    throw new Exception("This is not a valid BNS audio file!");
                }
            }
            flags = Shared.Swap(binaryReader.ReadUInt32());
            fileSize = Shared.Swap(binaryReader.ReadUInt32());
            size = Shared.Swap(binaryReader.ReadUInt16());
            chunkCount = Shared.Swap(binaryReader.ReadUInt16());
            infoOffset = Shared.Swap(binaryReader.ReadUInt32());
            infoLength = Shared.Swap(binaryReader.ReadUInt32());
            dataOffset = Shared.Swap(binaryReader.ReadUInt32());
            dataLength = Shared.Swap(binaryReader.ReadUInt32());
        }
    }

    internal class BNS_Info
    {
        //Private Variables
        private readonly byte[] magic = new byte[4]
        {
             73,
             78,
             70,
             79
        };
        private uint size = 160;
        private byte codec;
        private byte hasLoop;
        private byte channelCount = 2;
        private byte zero;
        private ushort sampleRate = 44100;
        private ushort pad0;
        private uint loopStart;
        private uint loopEnd; //Or total sample count
        private uint offsetToChannelStart = 24;
        private uint pad1;
        private uint channel1StartOffset = 32;
        private uint channel2StartOffset = 44;
        private uint channel1Start;
        private uint coefficients1Offset = 56;
        private uint pad2;
        private uint channel2Start;
        private uint coefficients2Offset = 104;
        private uint pad3;
        private int[] coefficients1 = new int[16];
        private ushort channel1Gain;
        private ushort channel1PredictiveScale;
        private ushort channel1PreviousValue;
        private ushort channel1NextPreviousValue;
        private ushort channel1LoopPredictiveScale;
        private ushort channel1LoopPreviousValue;
        private ushort channel1LoopNextPreviousValue;
        private ushort channel1LoopPadding;
        private int[] coefficients2 = new int[16];
        private ushort channel2Gain;
        private ushort channel2PredictiveScale;
        private ushort channel2PreviousValue;
        private ushort channel2NextPreviousValue;
        private ushort channel2LoopPredictiveScale;
        private ushort channel2LoopPreviousValue;
        private ushort channel2LoopNextPreviousValue;
        private ushort channel2LoopPadding;

        //Public Variables
        public byte HasLoop
        {
            get => hasLoop;
            set => hasLoop = value;
        }

        public uint Coefficients1Offset
        {
            get => coefficients1Offset;
            set => coefficients1Offset = value;
        }

        public uint Channel1StartOffset
        {
            get => channel1StartOffset;
            set => channel1StartOffset = value;
        }

        public uint Channel2StartOffset
        {
            get => channel2StartOffset;
            set => channel2StartOffset = value;
        }

        public uint Size
        {
            get => size;
            set => size = value;
        }

        public ushort SampleRate
        {
            get => sampleRate;
            set => sampleRate = value;
        }

        public byte ChannelCount
        {
            get => channelCount;
            set => channelCount = value;
        }

        public uint Channel1Start
        {
            get => channel1Start;
            set => channel1Start = value;
        }

        public uint Channel2Start
        {
            get => channel2Start;
            set => channel2Start = value;
        }

        public uint LoopStart
        {
            get => loopStart;
            set => loopStart = value;
        }

        public uint LoopEnd
        {
            get => loopEnd;
            set => loopEnd = value;
        }

        public int[] Coefficients1
        {
            get => coefficients1;
            set => coefficients1 = value;
        }

        public int[] Coefficients2
        {
            get => coefficients2;
            set => coefficients2 = value;
        }

        public void Write(Stream outStream)
        {
            outStream.Write(magic, 0, magic.Length);
            byte[] bytes1 = BitConverter.GetBytes(size);
            Array.Reverse(bytes1);
            outStream.Write(bytes1, 0, bytes1.Length);
            outStream.WriteByte(codec);
            outStream.WriteByte(hasLoop);
            outStream.WriteByte(channelCount);
            outStream.WriteByte(zero);
            byte[] bytes2 = BitConverter.GetBytes(sampleRate);
            Array.Reverse(bytes2);
            outStream.Write(bytes2, 0, bytes2.Length);
            byte[] bytes3 = BitConverter.GetBytes(pad0);
            Array.Reverse(bytes3);
            outStream.Write(bytes3, 0, bytes3.Length);
            byte[] bytes4 = BitConverter.GetBytes(loopStart);
            Array.Reverse(bytes4);
            outStream.Write(bytes4, 0, bytes4.Length);
            byte[] bytes5 = BitConverter.GetBytes(loopEnd);
            Array.Reverse(bytes5);
            outStream.Write(bytes5, 0, bytes5.Length);
            byte[] bytes6 = BitConverter.GetBytes(offsetToChannelStart);
            Array.Reverse(bytes6);
            outStream.Write(bytes6, 0, bytes6.Length);
            byte[] bytes7 = BitConverter.GetBytes(pad1);
            Array.Reverse(bytes7);
            outStream.Write(bytes7, 0, bytes7.Length);
            byte[] bytes8 = BitConverter.GetBytes(channel1StartOffset);
            Array.Reverse(bytes8);
            outStream.Write(bytes8, 0, bytes8.Length);
            byte[] bytes9 = BitConverter.GetBytes(channel2StartOffset);
            Array.Reverse(bytes9);
            outStream.Write(bytes9, 0, bytes9.Length);
            byte[] bytes10 = BitConverter.GetBytes(channel1Start);
            Array.Reverse(bytes10);
            outStream.Write(bytes10, 0, bytes10.Length);
            byte[] bytes11 = BitConverter.GetBytes(coefficients1Offset);
            Array.Reverse(bytes11);
            outStream.Write(bytes11, 0, bytes11.Length);
            if (channelCount == 2)
            {
                byte[] bytes12 = BitConverter.GetBytes(pad2);
                Array.Reverse(bytes12);
                outStream.Write(bytes12, 0, bytes12.Length);
                byte[] bytes13 = BitConverter.GetBytes(channel2Start);
                Array.Reverse(bytes13);
                outStream.Write(bytes13, 0, bytes13.Length);
                byte[] bytes14 = BitConverter.GetBytes(coefficients2Offset);
                Array.Reverse(bytes14);
                outStream.Write(bytes14, 0, bytes14.Length);
                byte[] bytes15 = BitConverter.GetBytes(pad3);
                Array.Reverse(bytes15);
                outStream.Write(bytes15, 0, bytes15.Length);
                foreach (int num in coefficients1)
                {
                    byte[] bytes16 = BitConverter.GetBytes(num);
                    Array.Reverse(bytes16);
                    outStream.Write(bytes16, 2, bytes16.Length - 2);
                }
                byte[] bytes17 = BitConverter.GetBytes(channel1Gain);
                Array.Reverse(bytes17);
                outStream.Write(bytes17, 0, bytes17.Length);
                byte[] bytes18 = BitConverter.GetBytes(channel1PredictiveScale);
                Array.Reverse(bytes18);
                outStream.Write(bytes18, 0, bytes18.Length);
                byte[] bytes19 = BitConverter.GetBytes(channel1PreviousValue);
                Array.Reverse(bytes19);
                outStream.Write(bytes19, 0, bytes19.Length);
                byte[] bytes20 = BitConverter.GetBytes(channel1NextPreviousValue);
                Array.Reverse(bytes20);
                outStream.Write(bytes20, 0, bytes20.Length);
                byte[] bytes21 = BitConverter.GetBytes(channel1LoopPredictiveScale);
                Array.Reverse(bytes21);
                outStream.Write(bytes21, 0, bytes21.Length);
                byte[] bytes22 = BitConverter.GetBytes(channel1LoopPreviousValue);
                Array.Reverse(bytes22);
                outStream.Write(bytes22, 0, bytes22.Length);
                byte[] bytes23 = BitConverter.GetBytes(channel1LoopNextPreviousValue);
                Array.Reverse(bytes23);
                outStream.Write(bytes23, 0, bytes23.Length);
                byte[] bytes24 = BitConverter.GetBytes(channel1LoopPadding);
                Array.Reverse(bytes24);
                outStream.Write(bytes24, 0, bytes24.Length);
                foreach (int num in coefficients2)
                {
                    byte[] bytes16 = BitConverter.GetBytes(num);
                    Array.Reverse(bytes16);
                    outStream.Write(bytes16, 2, bytes16.Length - 2);
                }
                byte[] bytes25 = BitConverter.GetBytes(channel2Gain);
                Array.Reverse(bytes25);
                outStream.Write(bytes25, 0, bytes25.Length);
                byte[] bytes26 = BitConverter.GetBytes(channel2PredictiveScale);
                Array.Reverse(bytes26);
                outStream.Write(bytes26, 0, bytes26.Length);
                byte[] bytes27 = BitConverter.GetBytes(channel2PreviousValue);
                Array.Reverse(bytes27);
                outStream.Write(bytes27, 0, bytes27.Length);
                byte[] bytes28 = BitConverter.GetBytes(channel2NextPreviousValue);
                Array.Reverse(bytes28);
                outStream.Write(bytes28, 0, bytes28.Length);
                byte[] bytes29 = BitConverter.GetBytes(channel2LoopPredictiveScale);
                Array.Reverse(bytes29);
                outStream.Write(bytes29, 0, bytes29.Length);
                byte[] bytes30 = BitConverter.GetBytes(channel2LoopPreviousValue);
                Array.Reverse(bytes30);
                outStream.Write(bytes30, 0, bytes30.Length);
                byte[] bytes31 = BitConverter.GetBytes(channel2LoopNextPreviousValue);
                Array.Reverse(bytes31);
                outStream.Write(bytes31, 0, bytes31.Length);
                byte[] bytes32 = BitConverter.GetBytes(channel2LoopPadding);
                Array.Reverse(bytes32);
                outStream.Write(bytes32, 0, bytes32.Length);
            }
            else
            {
                if (channelCount != 1)
                {
                    return;
                }

                foreach (int num in coefficients1)
                {
                    byte[] bytes12 = BitConverter.GetBytes(num);
                    Array.Reverse(bytes12);
                    outStream.Write(bytes12, 2, bytes12.Length - 2);
                }
                byte[] bytes13 = BitConverter.GetBytes(channel1Gain);
                Array.Reverse(bytes13);
                outStream.Write(bytes13, 0, bytes13.Length);
                byte[] bytes14 = BitConverter.GetBytes(channel1PredictiveScale);
                Array.Reverse(bytes14);
                outStream.Write(bytes14, 0, bytes14.Length);
                byte[] bytes15 = BitConverter.GetBytes(channel1PreviousValue);
                Array.Reverse(bytes15);
                outStream.Write(bytes15, 0, bytes15.Length);
                byte[] bytes16 = BitConverter.GetBytes(channel1NextPreviousValue);
                Array.Reverse(bytes16);
                outStream.Write(bytes16, 0, bytes16.Length);
                byte[] bytes17 = BitConverter.GetBytes(channel1LoopPredictiveScale);
                Array.Reverse(bytes17);
                outStream.Write(bytes17, 0, bytes17.Length);
                byte[] bytes18 = BitConverter.GetBytes(channel1LoopPreviousValue);
                Array.Reverse(bytes18);
                outStream.Write(bytes18, 0, bytes18.Length);
                byte[] bytes19 = BitConverter.GetBytes(channel1LoopNextPreviousValue);
                Array.Reverse(bytes19);
                outStream.Write(bytes19, 0, bytes19.Length);
                byte[] bytes20 = BitConverter.GetBytes(channel1LoopPadding);
                Array.Reverse(bytes20);
                outStream.Write(bytes20, 0, bytes20.Length);
            }
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            size = Shared.CompareByteArrays(magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
            codec = binaryReader.ReadByte();
            hasLoop = binaryReader.ReadByte();
            channelCount = binaryReader.ReadByte();
            zero = binaryReader.ReadByte();
            sampleRate = Shared.Swap(binaryReader.ReadUInt16());
            pad0 = Shared.Swap(binaryReader.ReadUInt16());
            loopStart = Shared.Swap(binaryReader.ReadUInt32());
            loopEnd = Shared.Swap(binaryReader.ReadUInt32());
            offsetToChannelStart = Shared.Swap(binaryReader.ReadUInt32());
            pad1 = Shared.Swap(binaryReader.ReadUInt32());
            channel1StartOffset = Shared.Swap(binaryReader.ReadUInt32());
            channel2StartOffset = Shared.Swap(binaryReader.ReadUInt32());
            channel1Start = Shared.Swap(binaryReader.ReadUInt32());
            coefficients1Offset = Shared.Swap(binaryReader.ReadUInt32());
            if (channelCount == 2)
            {
                pad2 = Shared.Swap(binaryReader.ReadUInt32());
                channel2Start = Shared.Swap(binaryReader.ReadUInt32());
                coefficients2Offset = Shared.Swap(binaryReader.ReadUInt32());
                pad3 = Shared.Swap(binaryReader.ReadUInt32());
                for (int index = 0; index < 16; ++index)
                {
                    coefficients1[index] = (short)Shared.Swap(binaryReader.ReadUInt16());
                }

                channel1Gain = Shared.Swap(binaryReader.ReadUInt16());
                channel1PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel1PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
                for (int index = 0; index < 16; ++index)
                {
                    coefficients2[index] = (short)Shared.Swap(binaryReader.ReadUInt16());
                }

                channel2Gain = Shared.Swap(binaryReader.ReadUInt16());
                channel2PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel2PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel2NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel2LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel2LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel2LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel2LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
            }
            else
            {
                if (channelCount != 1)
                {
                    return;
                }

                for (int index = 0; index < 16; ++index)
                {
                    coefficients1[index] = (short)Shared.Swap(binaryReader.ReadUInt16());
                }

                channel1Gain = Shared.Swap(binaryReader.ReadUInt16());
                channel1PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel1PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                channel1LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
            }
        }
    }
}