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
using System.IO;

namespace libWiiSharp
{
    public class Brlan
    {
        #region Public Functions
        /// <summary>
        /// Gets all TPLs that are required by the brlan (Frame Animation).
        /// </summary>
        /// <param name="pathTobrlan"></param>
        /// <returns></returns>
        public static string[] GetBrlanTpls(string pathTobrlan)
        {
            return PrivGetBrlanTpls(File.ReadAllBytes(pathTobrlan));
        }

        /// <summary>
        /// Gets all TPLs that are required by the brlan (Frame Animation).
        /// </summary>
        /// <param name="brlanFile"></param>
        /// <returns></returns>
        public static string[] GetBrlanTpls(byte[] brlanFile)
        {
            return PrivGetBrlanTpls(brlanFile);
        }

        /// <summary>
        /// Gets all TPLs that are required by the brlan (Frame Animation).
        /// </summary>
        /// <param name="wad"></param>
        /// <param name="banner"></param>
        /// <returns></returns>
        public static string[] GetBrlanTpls(WAD wad, bool banner)
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
                        if (u8.StringTable[index2].ToLower() == str + "_start.brlan" || u8.StringTable[index2].ToLower() == str + "_loop.brlan" || u8.StringTable[index2].ToLower() == str + ".brlan")
                        {
                            a = Shared.MergeStringArrays(a, GetBrlanTpls(u8.Data[index2]));
                        }
                    }
                    return a;
                }
            }
            return new string[0];
        }
        #endregion

        #region Private Functions
        private static string[] PrivGetBrlanTpls(byte[] brlanFile)
        {
            List<string> stringList = new List<string>();
            int numOfTpls = GetNumOfTpls(brlanFile);
            int index1 = 36 + numOfTpls * 4;
            for (int index2 = 0; index2 < numOfTpls; ++index2)
            {
                string empty = string.Empty;
                while (brlanFile[index1] != 0)
                {
                    empty += Convert.ToChar(brlanFile[index1++]).ToString();
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

        private static int GetNumOfTpls(byte[] brlanFile)
        {
            return Shared.Swap(BitConverter.ToUInt16(brlanFile, 28));
        }
        #endregion
    }
}
