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
    public class Wave : IDisposable
    {
        private WaveHeader header = new WaveHeader();
        private WaveFmtChunk fmt = new WaveFmtChunk();
        private WaveDataChunk data = new WaveDataChunk();
        private WaveSmplChunk smpl = new WaveSmplChunk();
        private bool hasSmpl;
        private bool isDisposed;

        public int SampleRate => (int)fmt.SampleRate;

        public int BitDepth => fmt.BitsPerSample;

        public int NumChannels => fmt.NumChannels;

        public int NumLoops => !hasSmpl ? 0 : (int)smpl.NumLoops;

        public int LoopStart => NumLoops == 0 ? 0 : (int)smpl.Loops[0].LoopStart;

        public int NumSamples => (int)(data.DataSize / (fmt.BitsPerSample / 8) / fmt.NumChannels);

        public int DataFormat => (int)fmt.AudioFormat;

        public byte[] SampleData => data.Data;

        public int PlayLength => (int)(data.DataSize / fmt.NumChannels / (fmt.BitsPerSample / 8) / fmt.SampleRate);

        public Wave(string pathToFile)
        {
            using FileStream fileStream = new FileStream(pathToFile, FileMode.Open);
            using BinaryReader reader = new BinaryReader(fileStream);
            ParseWave(reader);
        }

        public Wave(Stream wave)
        {
            ParseWave(new BinaryReader(wave));
        }

        public Wave(byte[] waveFile)
        {
            using MemoryStream memoryStream = new MemoryStream(waveFile);
            using BinaryReader reader = new BinaryReader(memoryStream);
            ParseWave(reader);
        }

        public Wave(int numChannels, int bitsPerSample, int sampleRate, byte[] samples)
        {
            fmt.SampleRate = (uint)sampleRate;
            fmt.NumChannels = (ushort)numChannels;
            fmt.BitsPerSample = (ushort)bitsPerSample;
            data.Data = samples;
        }

        ~Wave() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                header = null;
                fmt = null;
                data = null;
                smpl = null;
            }
            isDisposed = true;
        }

        public void Write(Stream writeStream)
        {
            WriteToStream(new BinaryWriter(writeStream));
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            WriteToStream(new BinaryWriter(memoryStream));
            return memoryStream;
        }

        public byte[] ToByteArray()
        {
            return ToMemoryStream().ToArray();
        }

        public void Save(string savePath)
        {
            using FileStream fileStream = new FileStream(savePath, FileMode.Create);
            using BinaryWriter writer = new BinaryWriter(fileStream);
            WriteToStream(writer);
        }

        public void AddLoop(int loopStartSample)
        {
            smpl.AddLoop(loopStartSample, NumSamples);
            hasSmpl = true;
        }

        public void RemoveLoop()
        {
            hasSmpl = false;
        }

        public void TrimStart(int newStartSample)
        {
            int offset = fmt.NumChannels * (fmt.BitsPerSample / 8) * newStartSample;
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(data.Data, offset, data.Data.Length - offset);
            data.Data = memoryStream.ToArray();
            memoryStream.Dispose();
        }

        private void WriteToStream(BinaryWriter writer)
        {
            header.FileSize = (uint)(4 + (int)fmt.FmtSize + 8 + (int)data.DataSize + 8 + (hasSmpl ? (int)smpl.SmplSize + 8 : 0));
            header.Write(writer);
            fmt.Write(writer);
            data.Write(writer);
            if (!hasSmpl)
            {
                return;
            }

            smpl.Write(writer);
        }

        private void ParseWave(BinaryReader reader)
        {
            bool[] flagArray = new bool[3];
            while (reader.BaseStream.Position < reader.BaseStream.Length - 4L)
            {
                uint num1 = Shared.Swap(reader.ReadUInt32());
                uint num2 = reader.ReadUInt32();
                long offset = reader.BaseStream.Position + num2;
                switch (num1)
                {
                    case 1380533830:
                        try
                        {
                            reader.BaseStream.Seek(-8L, SeekOrigin.Current);
                            header.Read(reader);
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
                            data.Read(reader);
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
                            fmt.Read(reader);
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
                            smpl.Read(reader);
                            hasSmpl = true;
                            break;
                        }
                        catch
                        {
                            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                            break;
                        }
                    default:
                        reader.BaseStream.Seek(num2, SeekOrigin.Current);
                        break;
                }
                if (flagArray[0] && flagArray[1] && (flagArray[2] && hasSmpl))
                {
                    break;
                }
            }
            if (!flagArray[0] || !flagArray[1] || !flagArray[2])
            {
                throw new Exception("Couldn't parse Wave file...");
            }
        }
    }
}
