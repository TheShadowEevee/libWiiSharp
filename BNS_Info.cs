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
using System.IO;

namespace libWiiSharp
{
    internal class BNS_Info
    {
        //Private Variables
        private byte[] magic = new byte[4]
        {
            (byte) 73,
            (byte) 78,
            (byte) 70,
            (byte) 79
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
            get => this.hasLoop;
            set => this.hasLoop = value;
        }

        public uint Coefficients1Offset
        {
            get => this.coefficients1Offset;
            set => this.coefficients1Offset = value;
        }

        public uint Channel1StartOffset
        {
            get => this.channel1StartOffset;
            set => this.channel1StartOffset = value;
        }

        public uint Channel2StartOffset
        {
            get => this.channel2StartOffset;
            set => this.channel2StartOffset = value;
        }

        public uint Size
        {
            get => this.size;
            set => this.size = value;
        }

        public ushort SampleRate
        {
            get => this.sampleRate;
            set => this.sampleRate = value;
        }

        public byte ChannelCount
        {
            get => this.channelCount;
            set => this.channelCount = value;
        }

        public uint Channel1Start
        {
            get => this.channel1Start;
            set => this.channel1Start = value;
        }

        public uint Channel2Start
        {
        get => this.channel2Start;
        set => this.channel2Start = value;
        }

        public uint LoopStart
        {
            get => this.loopStart;
            set => this.loopStart = value;
        }

        public uint LoopEnd
        {
            get => this.loopEnd;
            set => this.loopEnd = value;
        }

        public int[] Coefficients1
        {
            get => this.coefficients1;
            set => this.coefficients1 = value;
        }

        public int[] Coefficients2
        {
            get => this.coefficients2;
            set => this.coefficients2 = value;
        }

        public void Write(Stream outStream)
        {
            outStream.Write(this.magic, 0, this.magic.Length);
            byte[] bytes1 = BitConverter.GetBytes(this.size);
            Array.Reverse((Array) bytes1);
            outStream.Write(bytes1, 0, bytes1.Length);
            outStream.WriteByte(this.codec);
            outStream.WriteByte(this.hasLoop);
            outStream.WriteByte(this.channelCount);
            outStream.WriteByte(this.zero);
            byte[] bytes2 = BitConverter.GetBytes(this.sampleRate);
            Array.Reverse((Array) bytes2);
            outStream.Write(bytes2, 0, bytes2.Length);
            byte[] bytes3 = BitConverter.GetBytes(this.pad0);
            Array.Reverse((Array) bytes3);
            outStream.Write(bytes3, 0, bytes3.Length);
            byte[] bytes4 = BitConverter.GetBytes(this.loopStart);
            Array.Reverse((Array) bytes4);
            outStream.Write(bytes4, 0, bytes4.Length);
            byte[] bytes5 = BitConverter.GetBytes(this.loopEnd);
            Array.Reverse((Array) bytes5);
            outStream.Write(bytes5, 0, bytes5.Length);
            byte[] bytes6 = BitConverter.GetBytes(this.offsetToChannelStart);
            Array.Reverse((Array) bytes6);
            outStream.Write(bytes6, 0, bytes6.Length);
            byte[] bytes7 = BitConverter.GetBytes(this.pad1);
            Array.Reverse((Array) bytes7);
            outStream.Write(bytes7, 0, bytes7.Length);
            byte[] bytes8 = BitConverter.GetBytes(this.channel1StartOffset);
            Array.Reverse((Array) bytes8);
            outStream.Write(bytes8, 0, bytes8.Length);
            byte[] bytes9 = BitConverter.GetBytes(this.channel2StartOffset);
            Array.Reverse((Array) bytes9);
            outStream.Write(bytes9, 0, bytes9.Length);
            byte[] bytes10 = BitConverter.GetBytes(this.channel1Start);
            Array.Reverse((Array) bytes10);
            outStream.Write(bytes10, 0, bytes10.Length);
            byte[] bytes11 = BitConverter.GetBytes(this.coefficients1Offset);
            Array.Reverse((Array) bytes11);
            outStream.Write(bytes11, 0, bytes11.Length);
            if (this.channelCount == (byte) 2)
            {
                byte[] bytes12 = BitConverter.GetBytes(this.pad2);
                Array.Reverse((Array) bytes12);
                outStream.Write(bytes12, 0, bytes12.Length);
                byte[] bytes13 = BitConverter.GetBytes(this.channel2Start);
                Array.Reverse((Array) bytes13);
                outStream.Write(bytes13, 0, bytes13.Length);
                byte[] bytes14 = BitConverter.GetBytes(this.coefficients2Offset);
                Array.Reverse((Array) bytes14);
                outStream.Write(bytes14, 0, bytes14.Length);
                byte[] bytes15 = BitConverter.GetBytes(this.pad3);
                Array.Reverse((Array) bytes15);
                outStream.Write(bytes15, 0, bytes15.Length);
                foreach (int num in this.coefficients1)
                {
                    byte[] bytes16 = BitConverter.GetBytes(num);
                    Array.Reverse((Array) bytes16);
                    outStream.Write(bytes16, 2, bytes16.Length - 2);
                }
                byte[] bytes17 = BitConverter.GetBytes(this.channel1Gain);
                Array.Reverse((Array) bytes17);
                outStream.Write(bytes17, 0, bytes17.Length);
                byte[] bytes18 = BitConverter.GetBytes(this.channel1PredictiveScale);
                Array.Reverse((Array) bytes18);
                outStream.Write(bytes18, 0, bytes18.Length);
                byte[] bytes19 = BitConverter.GetBytes(this.channel1PreviousValue);
                Array.Reverse((Array) bytes19);
                outStream.Write(bytes19, 0, bytes19.Length);
                byte[] bytes20 = BitConverter.GetBytes(this.channel1NextPreviousValue);
                Array.Reverse((Array) bytes20);
                outStream.Write(bytes20, 0, bytes20.Length);
                byte[] bytes21 = BitConverter.GetBytes(this.channel1LoopPredictiveScale);
                Array.Reverse((Array) bytes21);
                outStream.Write(bytes21, 0, bytes21.Length);
                byte[] bytes22 = BitConverter.GetBytes(this.channel1LoopPreviousValue);
                Array.Reverse((Array) bytes22);
                outStream.Write(bytes22, 0, bytes22.Length);
                byte[] bytes23 = BitConverter.GetBytes(this.channel1LoopNextPreviousValue);
                Array.Reverse((Array) bytes23);
                outStream.Write(bytes23, 0, bytes23.Length);
                byte[] bytes24 = BitConverter.GetBytes(this.channel1LoopPadding);
                Array.Reverse((Array) bytes24);
                outStream.Write(bytes24, 0, bytes24.Length);
                foreach (int num in this.coefficients2)
                {
                    byte[] bytes16 = BitConverter.GetBytes(num);
                    Array.Reverse((Array) bytes16);
                    outStream.Write(bytes16, 2, bytes16.Length - 2);
                }
                byte[] bytes25 = BitConverter.GetBytes(this.channel2Gain);
                Array.Reverse((Array) bytes25);
                outStream.Write(bytes25, 0, bytes25.Length);
                byte[] bytes26 = BitConverter.GetBytes(this.channel2PredictiveScale);
                Array.Reverse((Array) bytes26);
                outStream.Write(bytes26, 0, bytes26.Length);
                byte[] bytes27 = BitConverter.GetBytes(this.channel2PreviousValue);
                Array.Reverse((Array) bytes27);
                outStream.Write(bytes27, 0, bytes27.Length);
                byte[] bytes28 = BitConverter.GetBytes(this.channel2NextPreviousValue);
                Array.Reverse((Array) bytes28);
                outStream.Write(bytes28, 0, bytes28.Length);
                byte[] bytes29 = BitConverter.GetBytes(this.channel2LoopPredictiveScale);
                Array.Reverse((Array) bytes29);
                outStream.Write(bytes29, 0, bytes29.Length);
                byte[] bytes30 = BitConverter.GetBytes(this.channel2LoopPreviousValue);
                Array.Reverse((Array) bytes30);
                outStream.Write(bytes30, 0, bytes30.Length);
                byte[] bytes31 = BitConverter.GetBytes(this.channel2LoopNextPreviousValue);
                Array.Reverse((Array) bytes31);
                outStream.Write(bytes31, 0, bytes31.Length);
                byte[] bytes32 = BitConverter.GetBytes(this.channel2LoopPadding);
                Array.Reverse((Array) bytes32);
                outStream.Write(bytes32, 0, bytes32.Length);
            }
            else
            {
                if (this.channelCount != (byte) 1)
                    return;
                foreach (int num in this.coefficients1)
                {
                    byte[] bytes12 = BitConverter.GetBytes(num);
                    Array.Reverse((Array) bytes12);
                    outStream.Write(bytes12, 2, bytes12.Length - 2);
                }
                byte[] bytes13 = BitConverter.GetBytes(this.channel1Gain);
                Array.Reverse((Array) bytes13);
                outStream.Write(bytes13, 0, bytes13.Length);
                byte[] bytes14 = BitConverter.GetBytes(this.channel1PredictiveScale);
                Array.Reverse((Array) bytes14);
                outStream.Write(bytes14, 0, bytes14.Length);
                byte[] bytes15 = BitConverter.GetBytes(this.channel1PreviousValue);
                Array.Reverse((Array) bytes15);
                outStream.Write(bytes15, 0, bytes15.Length);
                byte[] bytes16 = BitConverter.GetBytes(this.channel1NextPreviousValue);
                Array.Reverse((Array) bytes16);
                outStream.Write(bytes16, 0, bytes16.Length);
                byte[] bytes17 = BitConverter.GetBytes(this.channel1LoopPredictiveScale);
                Array.Reverse((Array) bytes17);
                outStream.Write(bytes17, 0, bytes17.Length);
                byte[] bytes18 = BitConverter.GetBytes(this.channel1LoopPreviousValue);
                Array.Reverse((Array) bytes18);
                outStream.Write(bytes18, 0, bytes18.Length);
                byte[] bytes19 = BitConverter.GetBytes(this.channel1LoopNextPreviousValue);
                Array.Reverse((Array) bytes19);
                outStream.Write(bytes19, 0, bytes19.Length);
                byte[] bytes20 = BitConverter.GetBytes(this.channel1LoopPadding);
                Array.Reverse((Array) bytes20);
                outStream.Write(bytes20, 0, bytes20.Length);
            }
        }

        public void Read(Stream input)
        {
            BinaryReader binaryReader = new BinaryReader(input);
            this.size = Shared.CompareByteArrays(this.magic, binaryReader.ReadBytes(4)) ? Shared.Swap(binaryReader.ReadUInt32()) : throw new Exception("This is not a valid BNS audfo file!");
            this.codec = binaryReader.ReadByte();
            this.hasLoop = binaryReader.ReadByte();
            this.channelCount = binaryReader.ReadByte();
            this.zero = binaryReader.ReadByte();
            this.sampleRate = Shared.Swap(binaryReader.ReadUInt16());
            this.pad0 = Shared.Swap(binaryReader.ReadUInt16());
            this.loopStart = Shared.Swap(binaryReader.ReadUInt32());
            this.loopEnd = Shared.Swap(binaryReader.ReadUInt32());
            this.offsetToChannelStart = Shared.Swap(binaryReader.ReadUInt32());
            this.pad1 = Shared.Swap(binaryReader.ReadUInt32());
            this.channel1StartOffset = Shared.Swap(binaryReader.ReadUInt32());
            this.channel2StartOffset = Shared.Swap(binaryReader.ReadUInt32());
            this.channel1Start = Shared.Swap(binaryReader.ReadUInt32());
            this.coefficients1Offset = Shared.Swap(binaryReader.ReadUInt32());
            if (this.channelCount == (byte) 2)
            {
                this.pad2 = Shared.Swap(binaryReader.ReadUInt32());
                this.channel2Start = Shared.Swap(binaryReader.ReadUInt32());
                this.coefficients2Offset = Shared.Swap(binaryReader.ReadUInt32());
                this.pad3 = Shared.Swap(binaryReader.ReadUInt32());
                for (int index = 0; index < 16; ++index)
                    this.coefficients1[index] = (int) (short) Shared.Swap(binaryReader.ReadUInt16());
                this.channel1Gain = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
                for (int index = 0; index < 16; ++index)
                    this.coefficients2[index] = (int) (short) Shared.Swap(binaryReader.ReadUInt16());
                this.channel2Gain = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel2LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
            }
            else
            {
                if (this.channelCount != (byte) 1)
                    return;
                for (int index = 0; index < 16; ++index)
                    this.coefficients1[index] = (int) (short) Shared.Swap(binaryReader.ReadUInt16());
                this.channel1Gain = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1PredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1PreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1NextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPredictiveScale = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopNextPreviousValue = Shared.Swap(binaryReader.ReadUInt16());
                this.channel1LoopPadding = Shared.Swap(binaryReader.ReadUInt16());
            }
        }
    }
}
