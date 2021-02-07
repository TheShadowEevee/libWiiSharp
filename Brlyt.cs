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
using System.IO;

namespace libWiiSharp
{
    public class Brlyt
    {
        #region Public Functions
        /// <summary>
        /// Gets all TPLs that are required by the brlyt.
        /// </summary>
        /// <param name="pathToBrlyt"></param>
        /// <returns></returns>
        public static string[] GetBrlytTpls(string pathToBrlyt)
        {
            return PrivGetBrlytTpls(File.ReadAllBytes(pathToBrlyt));
        }

        /// <summary>
        /// Gets all TPLs that are required by the brlyt.
        /// </summary>
        /// <param name="brlytFile"></param>
        /// <returns></returns>
        public static string[] GetBrlytTpls(byte[] brlytFile)
        {
            return PrivGetBrlytTpls(brlytFile);
        }

        /// <summary>
        /// Gets all TPLs that are required by the brlyt.
        /// </summary>
        /// <param name="wad"></param>
        /// <param name="banner"></param>
        /// <returns></returns>
        public static string[] GetBrlytTpls(WAD wad, bool banner)
        {
            if (!wad.HasBanner)
            {
                return new string[0];
            }

            string str = nameof(banner);
            if (!banner)
            {
                str = "icon";
            }

            for (int index1 = 0; index1 < wad.BannerApp.Nodes.Count; ++index1)
            {
                if (wad.BannerApp.StringTable[index1].ToLower() == str + ".bin")
                {
                    U8 u8 = U8.Load(wad.BannerApp.Data[index1]);
                    string[] a = new string[0];
                    for (int index2 = 0; index2 < u8.Nodes.Count; ++index2)
                    {
                        if (u8.StringTable[index2].ToLower() == str + ".brlyt")
                        {
                            a = Shared.MergeStringArrays(a, GetBrlytTpls(u8.Data[index2]));
                        }
                    }
                    return a;
                }
            }
            return new string[0];
        }
        #endregion

        #region Private Functions
        private static string[] PrivGetBrlytTpls(byte[] brlytFile)
        {
            List<string> stringList = new List<string>();
            int numOfTpls = GetNumOfTpls(brlytFile);
            int index1 = 48 + numOfTpls * 8;
            for (int index2 = 0; index2 < numOfTpls; ++index2)
            {
                string empty = string.Empty;
                while (brlytFile[index1] != 0)
                {
                    empty += Convert.ToChar(brlytFile[index1++]).ToString();
                }

                stringList.Add(empty);
                ++index1;
            }
            for (int index2 = stringList.Count - 1; index2 >= 0; --index2)
            {
                if (!stringList[index2].ToLower().EndsWith(".tpl"))
                {
                    stringList.RemoveAt(index2);
                }
            }
            return stringList.ToArray();
        }

        private static int GetNumOfTpls(byte[] brlytFile)
        {
            return Shared.Swap(BitConverter.ToUInt16(brlytFile, 44));
        }
        #endregion
    }
}
