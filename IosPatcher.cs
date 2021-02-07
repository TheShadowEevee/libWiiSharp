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
using System.ComponentModel;
using System.Text;

namespace libWiiSharp
{
    /// <summary>
    /// An IOS patcher which can patch fakesigning, es_identify and nand permissions.
    /// </summary>
    public class IosPatcher
    {
        private WAD wadFile;
        private int esIndex = -1;

        #region Public Functions
        /// <summary>
        /// Loads an IOS wad to patch the es module.
        /// </summary>
        /// <param name="iosWad"></param>
        public void LoadIOS(ref WAD iosWad)
        {
            wadFile = iosWad;
            GetEsIndex();
        }

        /// <summary>
        /// Patches fakesigning.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchFakeSigning()
        {
            return esIndex < 0 ? -1 : PrivPatchFakeSigning(ref wadFile.Contents[esIndex]);
        }

        /// <summary>
        /// Patches es_identify.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchEsIdentify()
        {
            return esIndex < 0 ? -1 : PrivPatchEsIdentify(ref wadFile.Contents[esIndex]);
        }

        /// <summary>
        /// Patches nand permissions.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchNandPermissions()
        {
            return esIndex < 0 ? -1 : PrivPatchNandPermissions(ref wadFile.Contents[esIndex]);
        }

        public int PatchVP()
        {
            return esIndex < 0 ? -1 : PrivPatchVP(ref wadFile.Contents[esIndex]);
        }

        /// <summary>
        /// Patches fakesigning, es_identify and nand permissions.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchAll()
        {
            return esIndex < 0 ? -1 : PrivPatchAll(ref wadFile.Contents[esIndex]);
        }

        public int PatchFakeSigning(ref byte[] esModule)
        {
            return PatchFakeSigning(ref esModule);
        }

        public int PatchEsIdentify(ref byte[] esModule)
        {
            return PatchEsIdentify(ref esModule);
        }

        public int PatchNandPermissions(ref byte[] esModule)
        {
            return PrivPatchNandPermissions(ref esModule);
        }

        public int PatchVP(ref byte[] esModule)
        {
            return PrivPatchVP(ref esModule);
        }

        public int PatchAll(ref byte[] esModule)
        {
            return PrivPatchAll(ref esModule);
        }
        #endregion

        #region Private Functions
        private int PrivPatchFakeSigning(ref byte[] esModule)
        {
            FireDebug("Patching Fakesigning...");
            int num = 0;
            byte[] second1 = new byte[4]
            {
                 32,
                 7,
                 35,
                 162
            };
            byte[] second2 = new byte[4]
            {
                 32,
                 7,
                 75,
                 11
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                FireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second1, 0, 4) || Shared.CompareByteArrays(esModule, firstIndex, second2, 0, 4))
                {
                    FireDebug("   Patching at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 1] = 0;
                    firstIndex += 4;
                    ++num;
                }
            }
            FireDebug("Patching Fakesigning Finished... (Patches applied: {0})", (object)num);
            return num;
        }

        private int PrivPatchEsIdentify(ref byte[] esModule)
        {
            FireDebug("Patching ES_Identify...");
            int num = 0;
            byte[] second = new byte[4]
            {
                 40,
                 3,
                 209,
                 35
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                FireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 4))
                {
                    FireDebug("   Patching at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = 0;
                    esModule[firstIndex + 3] = 0;
                    firstIndex += 4;
                    ++num;
                }
            }
            FireDebug("Patching ES_Identify Finished... (Patches applied: {0})", (object)num);
            return num;
        }

        private int PrivPatchNandPermissions(ref byte[] esModule)
        {
            FireDebug("Patching NAND Permissions...");
            int num = 0;
            byte[] second = new byte[6]
            {
                 66,
                 139,
                 208,
                 1,
                 37,
                 102
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 6; ++firstIndex)
            {
                FireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 6))
                {
                    FireDebug("   Patching at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = 224;
                    firstIndex += 6;
                    ++num;
                }
            }
            FireDebug("Patching NAND Permissions Finished... (Patches applied: {0})", (object)num);
            return num;
        }

        private int PrivPatchVP(ref byte[] esModule)
        {
            FireDebug("Patching VP...");
            int num = 0;
            byte[] second = new byte[4]
            {
                 210,
                 1,
                 78,
                 86
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                FireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 4))
                {
                    FireDebug("   Patching for VP at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex] = 224;
                    firstIndex += 4;
                    ++num;
                }
            }
            FireDebug("Patching VP Finished... (Patches applied: {0})", (object)num);
            return num;
        }

        private int PrivPatchAll(ref byte[] esModule)
        {
            FireDebug("Patching Fakesigning, ES_Identify, NAND Permissions and VP ...");
            int num = 0;
            byte[] second1 = new byte[4]
            {
                 32,
                 7,
                 35,
                 162
            };
            byte[] second2 = new byte[4]
            {
                 32,
                 7,
                 75,
                 11
            };
            byte[] second3 = new byte[4]
            {
                 40,
                 3,
                 209,
                 35
            };
            byte[] second4 = new byte[6]
            {
                 66,
                 139,
                 208,
                 1,
                 37,
                 102
            };
            byte[] second5 = new byte[4]
            {
                 210,
                 1,
                 78,
                 86
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 6; ++firstIndex)
            {
                FireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second1, 0, 4) || Shared.CompareByteArrays(esModule, firstIndex, second2, 0, 4))
                {
                    FireDebug("   Patching Fakesigning at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 1] = 0;
                    firstIndex += 4;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second3, 0, 4))
                {
                    FireDebug("   Patching ES_Identify at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = 0;
                    esModule[firstIndex + 3] = 0;
                    firstIndex += 4;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second4, 0, 6))
                {
                    FireDebug("   Patching NAND Permissions at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = 224;
                    firstIndex += 6;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second5, 0, 4))
                {
                    FireDebug("   Patching VP at Offset: 0x{0}", (object)firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex] = 224;
                    firstIndex += 4;
                    ++num;
                }
            }
            FireDebug("Patching Fakesigning, ES_Identify, NAND Permissions and VP Finished... (Patches applied: {0})", (object)num);
            return num;
        }

        private void GetEsIndex()
        {
            FireDebug("Scanning for ES Module...");
            string str = "$IOSVersion:";
            for (int index1 = wadFile.NumOfContents - 1; index1 >= 0; --index1)
            {
                FireDebug("   Scanning Content #{0} of {1}...", index1 + 1, wadFile.NumOfContents);
                FireProgress((index1 + 1) * 100 / wadFile.NumOfContents);
                for (int index2 = 0; index2 < wadFile.Contents[index1].Length - 64; ++index2)
                {
                    if (Encoding.ASCII.GetString(wadFile.Contents[index1], index2, 12) == str)
                    {
                        int index3 = index2 + 12;
                        while (wadFile.Contents[index1][index3] == 32)
                        {
                            ++index3;
                        }

                        if (Encoding.ASCII.GetString(wadFile.Contents[index1], index3, 3) == "ES:")
                        {
                            FireDebug("    -> ES Module found!");
                            FireDebug("Scanning for ES Module Finished...");
                            esIndex = index1;
                            FireProgress(100);
                            return;
                        }
                    }
                }
            }
            FireDebug("/!\\/!\\/!\\ ES Module wasn't found! /!\\/!\\/!\\");
            throw new Exception("ES module wasn't found!");
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires the Progress of various operations
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> Progress;
        /// <summary>
        /// Fires debugging messages. You may write them into a log file or log textbox.
        /// </summary>
        public event EventHandler<MessageEventArgs> Debug;
        private void FireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = Debug;
            if (debug == null)
            {
                return;
            }

            debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }

        private void FireProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = Progress;
            if (progress == null)
            {
                return;
            }

            progress(new object(), new ProgressChangedEventArgs(progressPercentage, string.Empty));
        }
        #endregion
    }
}
