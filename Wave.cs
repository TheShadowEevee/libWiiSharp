// Decompiled with JetBrains decompiler
// Type: libWiiSharp.Wave
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
  public class Wave : IDisposable
  {
    private WaveHeader header = new WaveHeader();
    private WaveFmtChunk fmt = new WaveFmtChunk();
    private WaveDataChunk data = new WaveDataChunk();
    private WaveSmplChunk smpl = new WaveSmplChunk();
    private bool hasSmpl;
    private bool isDisposed;

    public int SampleRate => (int) this.fmt.SampleRate;

    public int BitDepth => (int) this.fmt.BitsPerSample;

    public int NumChannels => (int) this.fmt.NumChannels;

    public int NumLoops => !this.hasSmpl ? 0 : (int) this.smpl.NumLoops;

    public int LoopStart => this.NumLoops == 0 ? 0 : (int) this.smpl.Loops[0].LoopStart;

    public int NumSamples => (int) ((long) this.data.DataSize / (long) ((int) this.fmt.BitsPerSample / 8) / (long) this.fmt.NumChannels);

    public int DataFormat => (int) this.fmt.AudioFormat;

    public byte[] SampleData => this.data.Data;

    public int PlayLength => (int) ((long) (this.data.DataSize / (uint) this.fmt.NumChannels) / (long) ((int) this.fmt.BitsPerSample / 8) / (long) this.fmt.SampleRate);

    public Wave(string pathToFile)
    {
      using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader((Stream) fileStream))
          this.parseWave(reader);
      }
    }

    public Wave(Stream wave) => this.parseWave(new BinaryReader(wave));

    public Wave(byte[] waveFile)
    {
      using (MemoryStream memoryStream = new MemoryStream(waveFile))
      {
        using (BinaryReader reader = new BinaryReader((Stream) memoryStream))
          this.parseWave(reader);
      }
    }

    public Wave(int numChannels, int bitsPerSample, int sampleRate, byte[] samples)
    {
      this.fmt.SampleRate = (uint) sampleRate;
      this.fmt.NumChannels = (ushort) numChannels;
      this.fmt.BitsPerSample = (ushort) bitsPerSample;
      this.data.Data = samples;
    }

    ~Wave() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.header = (WaveHeader) null;
        this.fmt = (WaveFmtChunk) null;
        this.data = (WaveDataChunk) null;
        this.smpl = (WaveSmplChunk) null;
      }
      this.isDisposed = true;
    }

    public void Write(Stream writeStream) => this.writeToStream(new BinaryWriter(writeStream));

    public MemoryStream ToMemoryStream()
    {
      MemoryStream memoryStream = new MemoryStream();
      this.writeToStream(new BinaryWriter((Stream) memoryStream));
      return memoryStream;
    }

    public byte[] ToByteArray() => this.ToMemoryStream().ToArray();

    public void Save(string savePath)
    {
      using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter((Stream) fileStream))
          this.writeToStream(writer);
      }
    }

    public void AddLoop(int loopStartSample)
    {
      this.smpl.AddLoop(loopStartSample, this.NumSamples);
      this.hasSmpl = true;
    }

    public void RemoveLoop() => this.hasSmpl = false;

    public void TrimStart(int newStartSample)
    {
      int offset = (int) this.fmt.NumChannels * ((int) this.fmt.BitsPerSample / 8) * newStartSample;
      MemoryStream memoryStream = new MemoryStream();
      memoryStream.Write(this.data.Data, offset, this.data.Data.Length - offset);
      this.data.Data = memoryStream.ToArray();
      memoryStream.Dispose();
    }

    private void writeToStream(BinaryWriter writer)
    {
      this.header.FileSize = (uint) (4 + (int) this.fmt.FmtSize + 8 + (int) this.data.DataSize + 8 + (this.hasSmpl ? (int) this.smpl.SmplSize + 8 : 0));
      this.header.Write(writer);
      this.fmt.Write(writer);
      this.data.Write(writer);
      if (!this.hasSmpl)
        return;
      this.smpl.Write(writer);
    }

    private void parseWave(BinaryReader reader)
    {
      bool[] flagArray = new bool[3];
      while (reader.BaseStream.Position < reader.BaseStream.Length - 4L)
      {
        uint num1 = Shared.Swap(reader.ReadUInt32());
        uint num2 = reader.ReadUInt32();
        long offset = reader.BaseStream.Position + (long) num2;
        switch (num1)
        {
          case 1380533830:
            try
            {
              reader.BaseStream.Seek(-8L, SeekOrigin.Current);
              this.header.Read(reader);
              flagArray[0] = true;
              break;
            }
            catch
            {
              reader.BaseStream.Seek(offset, SeekOrigin.Begin);
              break;
            }
          case 1684108385:
            try
            {
              reader.BaseStream.Seek(-8L, SeekOrigin.Current);
              this.data.Read(reader);
              flagArray[2] = true;
              break;
            }
            catch
            {
              reader.BaseStream.Seek(offset, SeekOrigin.Begin);
              break;
            }
          case 1718449184:
            try
            {
              reader.BaseStream.Seek(-8L, SeekOrigin.Current);
              this.fmt.Read(reader);
              flagArray[1] = true;
              break;
            }
            catch
            {
              reader.BaseStream.Seek(offset, SeekOrigin.Begin);
              break;
            }
          case 1936552044:
            try
            {
              reader.BaseStream.Seek(-8L, SeekOrigin.Current);
              this.smpl.Read(reader);
              this.hasSmpl = true;
              break;
            }
            catch
            {
              reader.BaseStream.Seek(offset, SeekOrigin.Begin);
              break;
            }
          default:
            reader.BaseStream.Seek((long) num2, SeekOrigin.Current);
            break;
        }
        if (flagArray[0] && flagArray[1] && (flagArray[2] && this.hasSmpl))
          break;
      }
      if (!flagArray[0] || !flagArray[1] || !flagArray[2])
        throw new Exception("Couldn't parse Wave file...");
    }
  }
}
