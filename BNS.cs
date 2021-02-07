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
}