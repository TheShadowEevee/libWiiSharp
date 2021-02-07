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
        private uint[] rgbaData;
        private byte[] tplData;
        private TPL_TextureFormat tplFormat;
        private TPL_PaletteFormat paletteFormat;
        private int width;
        private int height;

        public byte[] Palette => this.tplPalette;

        public byte[] Data => this.tplData;

        public ColorIndexConverter(
            uint[] rgbaData,
            int width,
            int height,
            TPL_TextureFormat tplFormat,
            TPL_PaletteFormat paletteFormat)
        {
            if (tplFormat != TPL_TextureFormat.CI4 && tplFormat != TPL_TextureFormat.CI8)
                throw new Exception("Texture format must be either CI4 or CI8");
            if (paletteFormat != TPL_PaletteFormat.IA8 && paletteFormat != TPL_PaletteFormat.RGB565 && paletteFormat != TPL_PaletteFormat.RGB5A3)
                throw new Exception("Palette format must be either IA8, RGB565 or RGB5A3!");
            this.rgbaData = rgbaData;
            this.width = width;
            this.height = height;
            this.tplFormat = tplFormat;
            this.paletteFormat = paletteFormat;
            this.buildPalette();
            if (tplFormat != TPL_TextureFormat.CI4)
            {
                if (tplFormat == TPL_TextureFormat.CI8)
                    this.toCI8();
                else
                    this.toCI14X2();
            }
            else
                this.toCI4();
        }

        private void toCI4()
        {
            byte[] numArray = new byte[Shared.AddPadding(this.width, 8) * Shared.AddPadding(this.height, 8) / 2];
            int num = 0;
            for (int index1 = 0; index1 < this.height; index1 += 8)
            {
                for (int index2 = 0; index2 < this.width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 8; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; index4 += 2)
                        {
                            uint colorIndex1 = this.getColorIndex(index3 >= this.height || index4 >= this.width ? 0U : this.rgbaData[index3 * this.width + index4]);
                            uint colorIndex2 = this.getColorIndex(index3 >= this.height || index4 >= this.width ? 0U : (index3 * this.width + index4 + 1 < this.rgbaData.Length ? this.rgbaData[index3 * this.width + index4 + 1] : 0U));
                            numArray[num++] = (byte) ((uint) (byte) colorIndex1 << 4 | (uint) (byte) colorIndex2);
                        }
                    }
                }
            }
            this.tplData = numArray;
        }

        private void toCI8()
        {
            byte[] numArray = new byte[Shared.AddPadding(this.width, 8) * Shared.AddPadding(this.height, 4)];
            int num1 = 0;
            for (int index1 = 0; index1 < this.height; index1 += 4)
            {
                for (int index2 = 0; index2 < this.width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            uint num2 = index3 >= this.height || index4 >= this.width ? 0U : this.rgbaData[index3 * this.width + index4];
                            numArray[num1++] = (byte) this.getColorIndex(num2);
                        }
                    }
                }
            }
            this.tplData = numArray;
        }

        private void toCI14X2()
        {
            byte[] numArray1 = new byte[Shared.AddPadding(this.width, 4) * Shared.AddPadding(this.height, 4) * 2];
            int num1 = 0;
            for (int index1 = 0; index1 < this.height; index1 += 4)
            {
                for (int index2 = 0; index2 < this.width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            byte[] bytes = BitConverter.GetBytes((ushort) this.getColorIndex(index3 >= this.height || index4 >= this.width ? 0U : this.rgbaData[index3 * this.width + index4]));
                            byte[] numArray2 = numArray1;
                            int index5 = num1;
                            int num2 = index5 + 1;
                            int num3 = (int) bytes[1];
                            numArray2[index5] = (byte) num3;
                            byte[] numArray3 = numArray1;
                            int index6 = num2;
                            num1 = index6 + 1;
                            int num4 = (int) bytes[0];
                            numArray3[index6] = (byte) num4;
                        }
                    }
                }
            }
            this.tplData = numArray1;
        }

        private void buildPalette()
        {
            int num1 = 256;
            if (this.tplFormat == TPL_TextureFormat.CI4)
                num1 = 16;
            else if (this.tplFormat == TPL_TextureFormat.CI14X2)
                num1 = 16384;
            List<uint> uintList = new List<uint>();
            List<ushort> ushortList = new List<ushort>();
            uintList.Add(0U);
            ushortList.Add((ushort) 0);
            for (int index = 1; index < this.rgbaData.Length && uintList.Count != num1; ++index)
            {
                if ((long) (this.rgbaData[index] >> 24 & (uint) byte.MaxValue) >= (this.tplFormat == TPL_TextureFormat.CI14X2 ? 1L : 25L))
                {
                    ushort num2 = Shared.Swap(this.convertToPaletteValue((int) this.rgbaData[index]));
                    if (!uintList.Contains(this.rgbaData[index]) && !ushortList.Contains(num2))
                    {
                        uintList.Add(this.rgbaData[index]);
                        ushortList.Add(num2);
                    }
                }
            }
            while (uintList.Count % 16 != 0)
            {
                uintList.Add(uint.MaxValue);
                ushortList.Add(ushort.MaxValue);
            }
            this.tplPalette = Shared.UShortArrayToByteArray(ushortList.ToArray());
            this.rgbaPalette = uintList.ToArray();
        }

        private ushort convertToPaletteValue(int rgba)
        {
            int num1 = 0;
            int num2;
            if (this.paletteFormat == TPL_PaletteFormat.IA8)
            {
                int num3 = ((rgba & (int) byte.MaxValue) + (rgba >> 8 & (int) byte.MaxValue) + (rgba >> 16 & (int) byte.MaxValue)) / 3 & (int) byte.MaxValue;
                num2 = (int) (ushort) ((rgba >> 24 & (int) byte.MaxValue) << 8 | num3);
            }
            else if (this.paletteFormat == TPL_PaletteFormat.RGB565)
            {
                num2 = (int) (ushort) ((rgba >> 16 & (int) byte.MaxValue) >> 3 << 11 | (rgba >> 8 & (int) byte.MaxValue) >> 2 << 5 | (rgba & (int) byte.MaxValue) >> 3);
            }
            else
            {
                int num3 = rgba >> 16 & (int) byte.MaxValue;
                int num4 = rgba >> 8 & (int) byte.MaxValue;
                int num5 = rgba & (int) byte.MaxValue;
                int num6 = rgba >> 24 & (int) byte.MaxValue;
                if (num6 <= 218)
                {
                    int num7 = num1 & -32769;
                    int num8 = num3 * 15 / (int) byte.MaxValue & 15;
                    int num9 = num4 * 15 / (int) byte.MaxValue & 15;
                    int num10 = num5 * 15 / (int) byte.MaxValue & 15;
                    int num11 = num6 * 7 / (int) byte.MaxValue & 7;
                    num2 = num7 | num11 << 12 | num10 | num9 << 4 | num8 << 8;
                }
                else
                {
                    int num7 = num1 | 32768;
                    int num8 = num3 * 31 / (int) byte.MaxValue & 31;
                    int num9 = num4 * 31 / (int) byte.MaxValue & 31;
                    int num10 = num5 * 31 / (int) byte.MaxValue & 31;
                    num2 = num7 | num10 | num9 << 5 | num8 << 10;
                }
            }
            return (ushort) num2;
        }

        private uint getColorIndex(uint value)
        {
            uint num1 = (uint) int.MaxValue;
            uint num2 = 0;
            if ((long) (value >> 24 & (uint) byte.MaxValue) < (this.tplFormat == TPL_TextureFormat.CI14X2 ? 1L : 25L))
                return 0;
            ushort paletteValue1 = this.convertToPaletteValue((int) value);
            for (int index = 0; index < this.rgbaPalette.Length; ++index)
            {
                ushort paletteValue2 = this.convertToPaletteValue((int) this.rgbaPalette[index]);
                if ((int) paletteValue1 == (int) paletteValue2)
                    return (uint) index;
                uint distance = this.getDistance(paletteValue1, paletteValue2);
                if (distance < num1)
                {
                    num1 = distance;
                    num2 = (uint) index;
                }
            }
      return num2;
        }

        private uint getDistance(ushort color, ushort paletteColor)
        {
            int rgbaValue1 = (int) this.convertToRgbaValue(color);
            uint rgbaValue2 = this.convertToRgbaValue(paletteColor);
            uint val1_1 = (uint) rgbaValue1 >> 24 & (uint) byte.MaxValue;
            uint val1_2 = (uint) rgbaValue1 >> 16 & (uint) byte.MaxValue;
            uint val1_3 = (uint) rgbaValue1 >> 8 & (uint) byte.MaxValue;
            uint val1_4 = (uint) (rgbaValue1 & (int) byte.MaxValue);
            uint val2_1 = rgbaValue2 >> 24 & (uint) byte.MaxValue;
            uint val2_2 = rgbaValue2 >> 16 & (uint) byte.MaxValue;
            uint val2_3 = rgbaValue2 >> 8 & (uint) byte.MaxValue;
            uint val2_4 = rgbaValue2 & (uint) byte.MaxValue;
            int num1 = (int) Math.Max(val1_1, val2_1) - (int) Math.Min(val1_1, val2_1);
            uint num2 = Math.Max(val1_2, val2_2) - Math.Min(val1_2, val2_2);
            uint num3 = Math.Max(val1_3, val2_3) - Math.Min(val1_3, val2_3);
            uint num4 = Math.Max(val1_4, val2_4) - Math.Min(val1_4, val2_4);
            int num5 = (int) num2;
            return (uint) (num1 + num5) + num3 + num4;
        }

        private uint convertToRgbaValue(ushort pixel)
        {
            if (this.paletteFormat == TPL_PaletteFormat.IA8)
            {
                int num1 = (int) pixel >> 8;
                int num2 = (int) pixel & (int) byte.MaxValue;
                return (uint) (num1 | num1 << 8 | num1 << 16 | num2 << 24);
            }
            if (this.paletteFormat == TPL_PaletteFormat.RGB565)
            {
                int num1 = ((int) pixel >> 11 & 31) << 3 & (int) byte.MaxValue;
                int num2 = ((int) pixel >> 5 & 63) << 2 & (int) byte.MaxValue;
                int num3 = ((int) pixel & 31) << 3 & (int) byte.MaxValue;
                int maxValue = (int) byte.MaxValue;
                return (uint) (num3 | num2 << 8 | num1 << 16 | maxValue << 24);
            }
            int num4;
            int num5;
            int num6;
            int num7;
            if (((int) pixel & 32768) != 0)
            {
                num4 = ((int) pixel >> 10 & 31) * (int) byte.MaxValue / 31;
                num5 = ((int) pixel >> 5 & 31) * (int) byte.MaxValue / 31;
                num6 = ((int) pixel & 31) * (int) byte.MaxValue / 31;
                num7 = (int) byte.MaxValue;
            }
            else
            {
                num7 = ((int) pixel >> 12 & 7) * (int) byte.MaxValue / 7;
                num4 = ((int) pixel >> 8 & 15) * (int) byte.MaxValue / 15;
                num5 = ((int) pixel >> 4 & 15) * (int) byte.MaxValue / 15;
                num6 = ((int) pixel & 15) * (int) byte.MaxValue / 15;
            }
            return (uint) (num6 | num5 << 8 | num4 << 16 | num7 << 24);
        }
    }
}
