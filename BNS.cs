// Decompiled with JetBrains decompiler
// Type: libWiiSharp.BNS
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

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
    private int[,] lSamples = new int[2, 2];
    private int[,] rlSamples = new int[2, 2];
    private int[] tlSamples = new int[2];
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
    private int[] defTbl = new int[16]
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
    private bool loopFromWave;
    private bool converted;
    private bool toMono;
    private bool isDisposed;

    public bool HasLoop
    {
      get => this.bnsInfo.HasLoop == (byte) 1;
      set => this.bnsInfo.HasLoop = value ? (byte) 1 : (byte) 0;
    }

    public uint LoopStart
    {
      get => this.bnsInfo.LoopStart;
      set => this.bnsInfo.LoopStart = value;
    }

    public uint TotalSampleCount
    {
      get => this.bnsInfo.LoopEnd;
      set => this.bnsInfo.LoopEnd = value;
    }

    public bool StereoToMono
    {
      get => this.toMono;
      set => this.toMono = value;
    }

    public event EventHandler<ProgressChangedEventArgs> Progress;

    protected BNS()
    {
    }

    public BNS(string waveFile) => this.waveFile = File.ReadAllBytes(waveFile);

    public BNS(string waveFile, bool loopFromWave)
    {
      this.waveFile = File.ReadAllBytes(waveFile);
      this.loopFromWave = loopFromWave;
    }

    public BNS(byte[] waveFile) => this.waveFile = waveFile;

    public BNS(byte[] waveFile, bool loopFromWave)
    {
      this.waveFile = waveFile;
      this.loopFromWave = loopFromWave;
    }

    ~BNS() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.bnsHeader = (BNS_Header) null;
        this.bnsInfo = (BNS_Info) null;
        this.bnsData = (BNS_Data) null;
        this.lSamples = (int[,]) null;
        this.rlSamples = (int[,]) null;
        this.tlSamples = (int[]) null;
        this.hbcDefTbl = (int[]) null;
        this.pHist1 = (int[]) null;
        this.pHist2 = (int[]) null;
        this.waveFile = (byte[]) null;
      }
      this.isDisposed = true;
    }

    public static int GetBnsLength(byte[] bnsFile)
    {
      uint num = (uint) Shared.Swap(BitConverter.ToUInt16(bnsFile, 44));
      return (int) (Shared.Swap(BitConverter.ToUInt32(bnsFile, 52)) / num);
    }

    public void Convert() => this.convert(this.waveFile, this.loopFromWave);

    public byte[] ToByteArray() => this.ToMemoryStream().ToArray();

    public MemoryStream ToMemoryStream()
    {
      if (!this.converted)
        this.convert(this.waveFile, this.loopFromWave);
      MemoryStream memoryStream = new MemoryStream();
      try
      {
        this.bnsHeader.Write((Stream) memoryStream);
        this.bnsInfo.Write((Stream) memoryStream);
        this.bnsData.Write((Stream) memoryStream);
        return memoryStream;
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
    }

    public void Save(string destionationFile)
    {
      if (File.Exists(destionationFile))
        File.Delete(destionationFile);
      using (FileStream fileStream = new FileStream(destionationFile, FileMode.Create))
      {
        byte[] array = this.ToMemoryStream().ToArray();
        fileStream.Write(array, 0, array.Length);
      }
    }

    public void SetLoop(int loopStartSample)
    {
      this.bnsInfo.HasLoop = (byte) 1;
      this.bnsInfo.LoopStart = (uint) loopStartSample;
    }

    private void convert(byte[] waveFile, bool loopFromWave)
    {
      Wave wave = new Wave(waveFile);
      int numLoops = wave.NumLoops;
      int loopStart = wave.LoopStart;
      this.bnsInfo.ChannelCount = (byte) wave.NumChannels;
      this.bnsInfo.SampleRate = (ushort) wave.SampleRate;
      if (this.bnsInfo.ChannelCount > (byte) 2 || this.bnsInfo.ChannelCount < (byte) 1)
        throw new Exception("Unsupported Amount of Channels!");
      if (wave.BitDepth != 16)
        throw new Exception("Only 16bit Wave files are supported!");
      this.bnsData.Data = wave.DataFormat == 1 ? this.Encode(wave.SampleData) : throw new Exception("The format of this Wave file is not supported!");
      if (this.bnsInfo.ChannelCount == (byte) 1)
      {
        this.bnsHeader.InfoLength = 96U;
        this.bnsHeader.DataOffset = 128U;
        this.bnsInfo.Size = 96U;
        this.bnsInfo.Channel1StartOffset = 28U;
        this.bnsInfo.Channel2StartOffset = 0U;
        this.bnsInfo.Channel1Start = 40U;
        this.bnsInfo.Coefficients1Offset = 0U;
      }
      this.bnsData.Size = (uint) (this.bnsData.Data.Length + 8);
      this.bnsHeader.DataLength = this.bnsData.Size;
      this.bnsHeader.FileSize = (uint) this.bnsHeader.Size + this.bnsInfo.Size + this.bnsData.Size;
      if (loopFromWave && numLoops == 1 && loopStart != -1)
      {
        this.bnsInfo.LoopStart = (uint) loopStart;
        this.bnsInfo.HasLoop = (byte) 1;
      }
      this.bnsInfo.LoopEnd = (uint) this.tempSampleCount;
      for (int index = 0; index < 16; ++index)
      {
        this.bnsInfo.Coefficients1[index] = this.defTbl[index];
        if (this.bnsInfo.ChannelCount == (byte) 2)
          this.bnsInfo.Coefficients2[index] = this.defTbl[index];
      }
      this.converted = true;
    }

    private byte[] Encode(byte[] inputFrames)
    {
      int[] inputBuffer = new int[14];
      this.tempSampleCount = inputFrames.Length / (this.bnsInfo.ChannelCount == (byte) 2 ? 4 : 2);
      int num1 = inputFrames.Length / (this.bnsInfo.ChannelCount == (byte) 2 ? 4 : 2) % 14;
      Array.Resize<byte>(ref inputFrames, inputFrames.Length + (14 - num1) * (this.bnsInfo.ChannelCount == (byte) 2 ? 4 : 2));
      int num2 = inputFrames.Length / (this.bnsInfo.ChannelCount == (byte) 2 ? 4 : 2);
      int num3 = (num2 + 13) / 14;
      List<int> intList1 = new List<int>();
      List<int> intList2 = new List<int>();
      int startIndex = 0;
      if (this.toMono && this.bnsInfo.ChannelCount == (byte) 2)
        this.bnsInfo.ChannelCount = (byte) 1;
      else if (this.toMono)
        this.toMono = false;
      for (int index = 0; index < num2; ++index)
      {
        intList1.Add((int) BitConverter.ToInt16(inputFrames, startIndex));
        startIndex += 2;
        if (this.bnsInfo.ChannelCount == (byte) 2 || this.toMono)
        {
          intList2.Add((int) BitConverter.ToInt16(inputFrames, startIndex));
          startIndex += 2;
        }
      }
      byte[] numArray1 = new byte[this.bnsInfo.ChannelCount == (byte) 2 ? num3 * 16 : num3 * 8];
      int num4 = 0;
      int num5 = num3 * 8;
      this.bnsInfo.Channel2Start = this.bnsInfo.ChannelCount == (byte) 2 ? (uint) num5 : 0U;
      int[] array1 = intList1.ToArray();
      int[] array2 = intList2.ToArray();
      for (int index1 = 0; index1 < num3; ++index1)
      {
        try
        {
          if (index1 % (num3 / 100) != 0)
          {
            if (index1 + 1 != num3)
              goto label_14;
          }
          this.ChangeProgress((index1 + 1) * 100 / num3);
        }
        catch
        {
        }
label_14:
        for (int index2 = 0; index2 < 14; ++index2)
          inputBuffer[index2] = array1[index1 * 14 + index2];
        byte[] numArray2 = this.RepackAdpcm(0, this.defTbl, inputBuffer);
        for (int index2 = 0; index2 < 8; ++index2)
          numArray1[num4 + index2] = numArray2[index2];
        num4 += 8;
        if (this.bnsInfo.ChannelCount == (byte) 2)
        {
          for (int index2 = 0; index2 < 14; ++index2)
            inputBuffer[index2] = array2[index1 * 14 + index2];
          byte[] numArray3 = this.RepackAdpcm(1, this.defTbl, inputBuffer);
          for (int index2 = 0; index2 < 8; ++index2)
            numArray1[num5 + index2] = numArray3[index2];
          num5 += 8;
        }
      }
      this.bnsInfo.LoopEnd = (uint) (num3 * 7);
      return numArray1;
    }

    private byte[] RepackAdpcm(int index, int[] table, int[] inputBuffer)
    {
      byte[] numArray1 = new byte[8];
      int[] numArray2 = new int[2];
      double num1 = 999999999.0;
      for (int tableIndex = 0; tableIndex < 8; ++tableIndex)
      {
        double outError;
        byte[] numArray3 = this.CompressAdpcm(index, table, tableIndex, inputBuffer, out outError);
        if (outError < num1)
        {
          num1 = outError;
          for (int index1 = 0; index1 < 8; ++index1)
            numArray1[index1] = numArray3[index1];
          for (int index1 = 0; index1 < 2; ++index1)
            numArray2[index1] = this.tlSamples[index1];
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
      int stdExponent = this.DetermineStdExponent(index, table, tableIndex, inputBuffer);
      while (stdExponent <= 15)
      {
        bool flag = false;
        num1 = 0;
        numArray[0] = (byte) (stdExponent | tableIndex << 4);
        for (int index1 = 0; index1 < 2; ++index1)
          this.tlSamples[index1] = this.rlSamples[index, index1];
        int num4 = 0;
        for (int index1 = 0; index1 < 14; ++index1)
        {
          int num5 = this.tlSamples[1] * num2 + this.tlSamples[0] * num3 >> 11;
          int input1 = inputBuffer[index1] - num5 >> stdExponent;
          if (input1 <= 7 && input1 >= -8)
          {
            int num6 = this.Clamp(input1, -8, 7);
            numArray[index1 / 2 + 1] = (index1 & 1) == 0 ? (byte) (num6 << 4) : (byte) ((uint) numArray[index1 / 2 + 1] | (uint) (num6 & 15));
            int input2 = num5 + (num6 << stdExponent);
            this.tlSamples[0] = this.tlSamples[1];
            this.tlSamples[1] = this.Clamp(input2, (int) short.MinValue, (int) short.MaxValue);
            num1 += (int) Math.Pow((double) (this.tlSamples[1] - inputBuffer[index1]), 2.0);
          }
          else
          {
            ++stdExponent;
            flag = true;
            break;
          }
        }
        if (!flag)
          num4 = 14;
        if (num4 == 14)
          break;
      }
      outError = (double) num1;
      return numArray;
    }

    private int DetermineStdExponent(int index, int[] table, int tableIndex, int[] inputBuffer)
    {
      int[] numArray = new int[2];
      int num1 = 0;
      int num2 = table[2 * tableIndex];
      int num3 = table[2 * tableIndex + 1];
      for (int index1 = 0; index1 < 2; ++index1)
        numArray[index1] = this.rlSamples[index, index1];
      for (int index1 = 0; index1 < 14; ++index1)
      {
        int num4 = numArray[1] * num2 + numArray[0] * num3 >> 11;
        int num5 = inputBuffer[index1] - num4;
        if (num5 > num1)
          num1 = num5;
        numArray[0] = numArray[1];
        numArray[1] = inputBuffer[index1];
      }
      return this.FindExponent((double) num1);
    }

    private int FindExponent(double residual)
    {
      int num = 0;
      for (; residual > 7.5 || residual < -8.5; residual /= 2.0)
        ++num;
      return num;
    }

    private int Clamp(int input, int min, int max)
    {
      if (input < min)
        return min;
      return input > max ? max : input;
    }

    private void ChangeProgress(int progressPercentage)
    {
      EventHandler<ProgressChangedEventArgs> progress = this.Progress;
      if (progress == null)
        return;
      progress(new object(), new ProgressChangedEventArgs(progressPercentage, new object()));
    }

    public static Wave BnsToWave(Stream inputFile)
    {
      BNS bns = new BNS();
      byte[] samples = bns.Read(inputFile);
      Wave wave = new Wave((int) bns.bnsInfo.ChannelCount, 16, (int) bns.bnsInfo.SampleRate, samples);
      if (bns.bnsInfo.HasLoop == (byte) 1)
        wave.AddLoop((int) bns.bnsInfo.LoopStart);
      return wave;
    }

    public static Wave BnsToWave(string pathToFile)
    {
      BNS bns = new BNS();
      byte[] samples = (byte[]) null;
      using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open))
        samples = bns.Read((Stream) fileStream);
      Wave wave = new Wave((int) bns.bnsInfo.ChannelCount, 16, (int) bns.bnsInfo.SampleRate, samples);
      if (bns.bnsInfo.HasLoop == (byte) 1)
        wave.AddLoop((int) bns.bnsInfo.LoopStart);
      return wave;
    }

    public static Wave BnsToWave(byte[] bnsFile)
    {
      BNS bns = new BNS();
      byte[] samples = (byte[]) null;
      using (MemoryStream memoryStream = new MemoryStream(bnsFile))
        samples = bns.Read((Stream) memoryStream);
      Wave wave = new Wave((int) bns.bnsInfo.ChannelCount, 16, (int) bns.bnsInfo.SampleRate, samples);
      if (bns.bnsInfo.HasLoop == (byte) 1)
        wave.AddLoop((int) bns.bnsInfo.LoopStart);
      return wave;
    }

    private byte[] Read(Stream input)
    {
      input.Seek(0L, SeekOrigin.Begin);
      this.bnsHeader.Read(input);
      this.bnsInfo.Read(input);
      this.bnsData.Read(input);
      return this.Decode();
    }

    private byte[] Decode()
    {
      List<byte> byteList = new List<byte>();
      int num = this.bnsData.Data.Length / (this.bnsInfo.ChannelCount == (byte) 2 ? 16 : 8);
      int dataOffset1 = 0;
      int dataOffset2 = num * 8;
      byte[] numArray1 = new byte[0];
      byte[] numArray2 = new byte[0];
      for (int index1 = 0; index1 < num; ++index1)
      {
        byte[] numArray3 = this.DecodeAdpcm(0, dataOffset1);
        if (this.bnsInfo.ChannelCount == (byte) 2)
          numArray2 = this.DecodeAdpcm(1, dataOffset2);
        for (int index2 = 0; index2 < 14; ++index2)
        {
          byteList.Add(numArray3[index2 * 2]);
          byteList.Add(numArray3[index2 * 2 + 1]);
          if (this.bnsInfo.ChannelCount == (byte) 2)
          {
            byteList.Add(numArray2[index2 * 2]);
            byteList.Add(numArray2[index2 * 2 + 1]);
          }
        }
        dataOffset1 += 8;
        if (this.bnsInfo.ChannelCount == (byte) 2)
          dataOffset2 += 8;
      }
      return byteList.ToArray();
    }

    private byte[] DecodeAdpcm(int channel, int dataOffset)
    {
      byte[] numArray = new byte[28];
      int num1 = (int) this.bnsData.Data[dataOffset] >> 4 & 15;
      int num2 = 1 << ((int) this.bnsData.Data[dataOffset] & 15);
      int num3 = this.pHist1[channel];
      int num4 = this.pHist2[channel];
      int num5 = channel == 0 ? this.bnsInfo.Coefficients1[num1 * 2] : this.bnsInfo.Coefficients2[num1 * 2];
      int num6 = channel == 0 ? this.bnsInfo.Coefficients1[num1 * 2 + 1] : this.bnsInfo.Coefficients2[num1 * 2 + 1];
      for (int index = 0; index < 14; ++index)
      {
        short num7 = (short) this.bnsData.Data[dataOffset + (index / 2 + 1)];
        int num8 = (index & 1) != 0 ? (int) num7 & 15 : (int) num7 >> 4;
        if (num8 >= 8)
          num8 -= 16;
        int num9 = this.Clamp((num2 * num8 << 11) + (num5 * num3 + num6 * num4) + 1024 >> 11, (int) short.MinValue, (int) short.MaxValue);
        numArray[index * 2] = (byte) ((uint) (short) num9 & (uint) byte.MaxValue);
        numArray[index * 2 + 1] = (byte) ((uint) (short) num9 >> 8);
        num4 = num3;
        num3 = num9;
      }
      this.pHist1[channel] = num3;
      this.pHist2[channel] = num4;
      return numArray;
    }
  }
}
