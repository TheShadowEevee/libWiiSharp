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
