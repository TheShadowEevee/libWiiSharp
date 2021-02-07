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

namespace libWiiSharp
{
    internal class ColorIndexConverter
    {
        private uint[] rgbaPalette;
        private byte[] tplPalette;
        private readonly uint[] rgbaData;
        private byte[] tplData;
        private readonly TPL_TextureFormat tplFormat;
        private readonly TPL_PaletteFormat paletteFormat;
        private readonly int width;
        private readonly int height;

        public byte[] Palette => tplPalette;

        public byte[] Data => tplData;

        public ColorIndexConverter(
            uint[] rgbaData,
            int width,
            int height,
            TPL_TextureFormat tplFormat,
            TPL_PaletteFormat paletteFormat)
        {
            if (tplFormat != TPL_TextureFormat.CI4 && tplFormat != TPL_TextureFormat.CI8)
            {
                throw new Exception("Texture format must be either CI4 or CI8");
            }

            if (paletteFormat != TPL_PaletteFormat.IA8 && paletteFormat != TPL_PaletteFormat.RGB565 && paletteFormat != TPL_PaletteFormat.RGB5A3)
            {
                throw new Exception("Palette format must be either IA8, RGB565 or RGB5A3!");
            }

            this.rgbaData = rgbaData;
            this.width = width;
            this.height = height;
            this.tplFormat = tplFormat;
            this.paletteFormat = paletteFormat;
            BuildPalette();
            if (tplFormat != TPL_TextureFormat.CI4)
            {
                if (tplFormat == TPL_TextureFormat.CI8)
                {
                    ToCI8();
                }
                else
                {
                    ToCI14X2();
                }
            }
            else
            {
                ToCI4();
            }
        }

        private void ToCI4()
        {
            byte[] numArray = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2];
            int num = 0;
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 8; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; index4 += 2)
                        {
                            uint colorIndex1 = GetColorIndex(index3 >= height || index4 >= width ? 0U : rgbaData[index3 * width + index4]);
                            uint colorIndex2 = GetColorIndex(index3 >= height || index4 >= width ? 0U : (index3 * width + index4 + 1 < rgbaData.Length ? rgbaData[index3 * width + index4 + 1] : 0U));
                            numArray[num++] = (byte)((uint)(byte)colorIndex1 << 4 | (byte)colorIndex2);
                        }
                    }
                }
            }
            tplData = numArray;
        }

        private void ToCI8()
        {
            byte[] numArray = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4)];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            uint num2 = index3 >= height || index4 >= width ? 0U : rgbaData[index3 * width + index4];
                            numArray[num1++] = (byte)GetColorIndex(num2);
                        }
                    }
                }
            }
            tplData = numArray;
        }

        private void ToCI14X2()
        {
            byte[] numArray1 = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            byte[] bytes = BitConverter.GetBytes((ushort)GetColorIndex(index3 >= height || index4 >= width ? 0U : rgbaData[index3 * width + index4]));
                            byte[] numArray2 = numArray1;
                            int index5 = num1;
                            int num2 = index5 + 1;
                            int num3 = bytes[1];
                            numArray2[index5] = (byte)num3;
                            byte[] numArray3 = numArray1;
                            int index6 = num2;
                            num1 = index6 + 1;
                            int num4 = bytes[0];
                            numArray3[index6] = (byte)num4;
                        }
                    }
                }
            }
            tplData = numArray1;
        }

        private void BuildPalette()
        {
            int num1 = 256;
            if (tplFormat == TPL_TextureFormat.CI4)
            {
                num1 = 16;
            }
            else if (tplFormat == TPL_TextureFormat.CI14X2)
            {
                num1 = 16384;
            }

            List<uint> uintList = new List<uint>();
            List<ushort> ushortList = new List<ushort>();
            uintList.Add(0U);
            ushortList.Add(0);
            for (int index = 1; index < rgbaData.Length && uintList.Count != num1; ++index)
            {
                if ((rgbaData[index] >> 24 & byte.MaxValue) >= (tplFormat == TPL_TextureFormat.CI14X2 ? 1L : 25L))
                {
                    ushort num2 = Shared.Swap(ConvertToPaletteValue((int)rgbaData[index]));
                    if (!uintList.Contains(rgbaData[index]) && !ushortList.Contains(num2))
                    {
                        uintList.Add(rgbaData[index]);
                        ushortList.Add(num2);
                    }
                }
            }
            while (uintList.Count % 16 != 0)
            {
                uintList.Add(uint.MaxValue);
                ushortList.Add(ushort.MaxValue);
            }
            tplPalette = Shared.UShortArrayToByteArray(ushortList.ToArray());
            rgbaPalette = uintList.ToArray();
        }

        private ushort ConvertToPaletteValue(int rgba)
        {
            int num1 = 0;
            int num2;
            if (paletteFormat == TPL_PaletteFormat.IA8)
            {
                int num3 = ((rgba & byte.MaxValue) + (rgba >> 8 & byte.MaxValue) + (rgba >> 16 & byte.MaxValue)) / 3 & byte.MaxValue;
                num2 = (ushort)((rgba >> 24 & byte.MaxValue) << 8 | num3);
            }
            else if (paletteFormat == TPL_PaletteFormat.RGB565)
            {
                num2 = (ushort)((rgba >> 16 & byte.MaxValue) >> 3 << 11 | (rgba >> 8 & byte.MaxValue) >> 2 << 5 | (rgba & byte.MaxValue) >> 3);
            }
            else
            {
                int num3 = rgba >> 16 & byte.MaxValue;
                int num4 = rgba >> 8 & byte.MaxValue;
                int num5 = rgba & byte.MaxValue;
                int num6 = rgba >> 24 & byte.MaxValue;
                if (num6 <= 218)
                {
                    int num7 = num1 & -32769;
                    int num8 = num3 * 15 / byte.MaxValue & 15;
                    int num9 = num4 * 15 / byte.MaxValue & 15;
                    int num10 = num5 * 15 / byte.MaxValue & 15;
                    int num11 = num6 * 7 / byte.MaxValue & 7;
                    num2 = num7 | num11 << 12 | num10 | num9 << 4 | num8 << 8;
                }
                else
                {
                    int num7 = num1 | 32768;
                    int num8 = num3 * 31 / byte.MaxValue & 31;
                    int num9 = num4 * 31 / byte.MaxValue & 31;
                    int num10 = num5 * 31 / byte.MaxValue & 31;
                    num2 = num7 | num10 | num9 << 5 | num8 << 10;
                }
            }
            return (ushort)num2;
        }

        private uint GetColorIndex(uint value)
        {
            uint num1 = int.MaxValue;
            uint num2 = 0;
            if ((value >> 24 & byte.MaxValue) < (tplFormat == TPL_TextureFormat.CI14X2 ? 1L : 25L))
            {
                return 0;
            }

            ushort paletteValue1 = ConvertToPaletteValue((int)value);
            for (int index = 0; index < rgbaPalette.Length; ++index)
            {
                ushort paletteValue2 = ConvertToPaletteValue((int)rgbaPalette[index]);
                if (paletteValue1 == paletteValue2)
                {
                    return (uint)index;
                }

                uint distance = GetDistance(paletteValue1, paletteValue2);
                if (distance < num1)
                {
                    num1 = distance;
                    num2 = (uint)index;
                }
            }
            return num2;
        }

        private uint GetDistance(ushort color, ushort paletteColor)
        {
            int rgbaValue1 = (int)ConvertToRgbaValue(color);
            uint rgbaValue2 = ConvertToRgbaValue(paletteColor);
            uint val1_1 = (uint)rgbaValue1 >> 24 & byte.MaxValue;
            uint val1_2 = (uint)rgbaValue1 >> 16 & byte.MaxValue;
            uint val1_3 = (uint)rgbaValue1 >> 8 & byte.MaxValue;
            uint val1_4 = (uint)(rgbaValue1 & byte.MaxValue);
            uint val2_1 = rgbaValue2 >> 24 & byte.MaxValue;
            uint val2_2 = rgbaValue2 >> 16 & byte.MaxValue;
            uint val2_3 = rgbaValue2 >> 8 & byte.MaxValue;
            uint val2_4 = rgbaValue2 & byte.MaxValue;
            int num1 = (int)Math.Max(val1_1, val2_1) - (int)Math.Min(val1_1, val2_1);
            uint num2 = Math.Max(val1_2, val2_2) - Math.Min(val1_2, val2_2);
            uint num3 = Math.Max(val1_3, val2_3) - Math.Min(val1_3, val2_3);
            uint num4 = Math.Max(val1_4, val2_4) - Math.Min(val1_4, val2_4);
            int num5 = (int)num2;
            return (uint)(num1 + num5) + num3 + num4;
        }

        private uint ConvertToRgbaValue(ushort pixel)
        {
            if (paletteFormat == TPL_PaletteFormat.IA8)
            {
                int num1 = pixel >> 8;
                int num2 = pixel & byte.MaxValue;
                return (uint)(num1 | num1 << 8 | num1 << 16 | num2 << 24);
            }
            if (paletteFormat == TPL_PaletteFormat.RGB565)
            {
                int num1 = (pixel >> 11 & 31) << 3 & byte.MaxValue;
                int num2 = (pixel >> 5 & 63) << 2 & byte.MaxValue;
                int num3 = (pixel & 31) << 3 & byte.MaxValue;
                int maxValue = byte.MaxValue;
                return (uint)(num3 | num2 << 8 | num1 << 16 | maxValue << 24);
            }
            int num4;
            int num5;
            int num6;
            int num7;
            if ((pixel & 32768) != 0)
            {
                num4 = (pixel >> 10 & 31) * byte.MaxValue / 31;
                num5 = (pixel >> 5 & 31) * byte.MaxValue / 31;
                num6 = (pixel & 31) * byte.MaxValue / 31;
                num7 = byte.MaxValue;
            }
            else
            {
                num7 = (pixel >> 12 & 7) * byte.MaxValue / 7;
                num4 = (pixel >> 8 & 15) * byte.MaxValue / 15;
                num5 = (pixel >> 4 & 15) * byte.MaxValue / 15;
                num6 = (pixel & 15) * byte.MaxValue / 15;
            }
            return (uint)(num6 | num5 << 8 | num4 << 16 | num7 << 24);
        }
    }
}
