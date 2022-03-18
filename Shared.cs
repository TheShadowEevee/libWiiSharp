/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2022 TheShadowEevee, Github Contributors
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
using System.Globalization;
using System.Net;

namespace libWiiSharp
{
    public static class Shared
    {
        public static string[] MergeStringArrays(string[] a, string[] b)
        {
            List<string> stringList = new List<string>(a);
            foreach (string str in b)
            {
                if (!stringList.Contains(str))
                {
                    stringList.Add(str);
                }
            }
            stringList.Sort();
            return stringList.ToArray();
        }

        public static bool CompareByteArrays(
          byte[] first,
          int firstIndex,
          byte[] second,
          int secondIndex,
          int length)
        {
            if (first.Length < length || second.Length < length)
            {
                return false;
            }

            for (int index = 0; index < length; ++index)
            {
                if (first[firstIndex + index] != second[secondIndex + index])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareByteArrays(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int index = 0; index < first.Length; ++index)
            {
                if (first[index] != second[index])
                {
                    return false;
                }
            }
            return true;
        }

        public static string ByteArrayToString(byte[] byteArray, char separator = ' ')
        {
            string str = string.Empty;
            foreach (byte num in byteArray)
            {
                str = str + num.ToString("x2").ToUpper() + separator.ToString();
            }

            return str.Remove(str.Length - 1);
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            byte[] numArray = new byte[hexString.Length / 2];
            for (int index = 0; index < hexString.Length / 2; ++index)
            {
                numArray[index] = byte.Parse(hexString.Substring(index * 2, 2), NumberStyles.HexNumber);
            }

            return numArray;
        }

        public static int CountCharsInString(string theString, char theChar)
        {
            int num1 = 0;
            foreach (int num2 in theString)
            {
                if (num2 == theChar)
                {
                    ++num1;
                }
            }
            return num1;
        }

        public static long AddPadding(long value)
        {
            return AddPadding(value, 64);
        }

        public static long AddPadding(long value, int padding)
        {
            if (value % padding != 0L)
            {
                value += padding - value % padding;
            }

            return value;
        }

        public static int AddPadding(int value)
        {
            return AddPadding(value, 64);
        }

        public static int AddPadding(int value, int padding)
        {
            if (value % padding != 0)
            {
                value += padding - value % padding;
            }

            return value;
        }

        public static ushort Swap(ushort value)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        public static uint Swap(uint value)
        {
            return (uint)IPAddress.HostToNetworkOrder((int)value);
        }

        public static ulong Swap(ulong value)
        {
            return (ulong)IPAddress.HostToNetworkOrder((long)value);
        }

        public static byte[] UShortArrayToByteArray(ushort[] array)
        {
            List<byte> byteList = new List<byte>();
            foreach (ushort num in array)
            {
                byte[] bytes = BitConverter.GetBytes(num);
                byteList.AddRange(bytes);
            }
            return byteList.ToArray();
        }

        public static byte[] UIntArrayToByteArray(uint[] array)
        {
            List<byte> byteList = new List<byte>();
            foreach (uint num in array)
            {
                byte[] bytes = BitConverter.GetBytes(num);
                byteList.AddRange(bytes);
            }
            return byteList.ToArray();
        }

        public static uint[] ByteArrayToUIntArray(byte[] array)
        {
            uint[] numArray = new uint[array.Length / 4];
            int num = 0;
            for (int startIndex = 0; startIndex < array.Length; startIndex += 4)
            {
                numArray[num++] = BitConverter.ToUInt32(array, startIndex);
            }

            return numArray;
        }

        public static ushort[] ByteArrayToUShortArray(byte[] array)
        {
            ushort[] numArray = new ushort[array.Length / 2];
            int num = 0;
            for (int startIndex = 0; startIndex < array.Length; startIndex += 2)
            {
                numArray[num++] = BitConverter.ToUInt16(array, startIndex);
            }

            return numArray;
        }
    }
}
