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
            this.wadFile = iosWad;
            this.getEsIndex();
        }

        /// <summary>
        /// Patches fakesigning.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchFakeSigning() => this.esIndex < 0 ? -1 : this.patchFakeSigning(ref this.wadFile.Contents[this.esIndex]);

        /// <summary>
        /// Patches es_identify.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchEsIdentify() => this.esIndex < 0 ? -1 : this.patchEsIdentify(ref this.wadFile.Contents[this.esIndex]);

        /// <summary>
        /// Patches nand permissions.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchNandPermissions() => this.esIndex < 0 ? -1 : this.patchNandPermissions(ref this.wadFile.Contents[this.esIndex]);

        public int PatchVP() => this.esIndex < 0 ? -1 : this.patchVP(ref this.wadFile.Contents[this.esIndex]);

        /// <summary>
        /// Patches fakesigning, es_identify and nand permissions.
        /// Returns the number of applied patches.
        /// </summary>
        /// <returns></returns>
        public int PatchAll() => this.esIndex < 0 ? -1 : this.patchAll(ref this.wadFile.Contents[this.esIndex]);

        public int PatchFakeSigning(ref byte[] esModule) => this.patchFakeSigning(ref esModule);

        public int PatchEsIdentify(ref byte[] esModule) => this.patchEsIdentify(ref esModule);

        public int PatchNandPermissions(ref byte[] esModule) => this.patchNandPermissions(ref esModule);

        public int PatchVP(ref byte[] esModule) => this.patchVP(ref esModule);

        public int PatchAll(ref byte[] esModule) => this.patchAll(ref esModule);
        #endregion

        #region Private Functions
        private int patchFakeSigning(ref byte[] esModule)
        {
            this.fireDebug("Patching Fakesigning...");
            int num = 0;
            byte[] second1 = new byte[4]
            {
                (byte) 32,
                (byte) 7,
                (byte) 35,
                (byte) 162
            };
            byte[] second2 = new byte[4]
            {
                (byte) 32,
                (byte) 7,
                (byte) 75,
                (byte) 11
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                this.fireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second1, 0, 4) || Shared.CompareByteArrays(esModule, firstIndex, second2, 0, 4))
                {
                    this.fireDebug("   Patching at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 1] = (byte) 0;
                    firstIndex += 4;
                    ++num;
                }
            }
            this.fireDebug("Patching Fakesigning Finished... (Patches applied: {0})", (object) num);
             return num;
        }

        private int patchEsIdentify(ref byte[] esModule)
        {
            this.fireDebug("Patching ES_Identify...");
            int num = 0;
            byte[] second = new byte[4]
            {
                (byte) 40,
                (byte) 3,
                (byte) 209,
                (byte) 35
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                this.fireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 4))
                {
                    this.fireDebug("   Patching at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = (byte) 0;
                    esModule[firstIndex + 3] = (byte) 0;
                    firstIndex += 4;
                    ++num;
                }
            }
            this.fireDebug("Patching ES_Identify Finished... (Patches applied: {0})", (object) num);
            return num;
        }

        private int patchNandPermissions(ref byte[] esModule)
        {
            this.fireDebug("Patching NAND Permissions...");
            int num = 0;
            byte[] second = new byte[6]
            {
                (byte) 66,
                (byte) 139,
                (byte) 208,
                (byte) 1,
                (byte) 37,
                (byte) 102
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 6; ++firstIndex)
            {
                this.fireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 6))
                {
                    this.fireDebug("   Patching at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = (byte) 224;
                    firstIndex += 6;
                    ++num;
                }
            }
            this.fireDebug("Patching NAND Permissions Finished... (Patches applied: {0})", (object) num);
            return num;
        }

        private int patchVP(ref byte[] esModule)
        {
            this.fireDebug("Patching VP...");
            int num = 0;
            byte[] second = new byte[4]
            {
                (byte) 210,
                (byte) 1,
                (byte) 78,
                (byte) 86
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 4; ++firstIndex)
            {
                this.fireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second, 0, 4))
                {
                    this.fireDebug("   Patching for VP at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex] = (byte) 224;
                    firstIndex += 4;
                    ++num;
                }
            }
            this.fireDebug("Patching VP Finished... (Patches applied: {0})", (object) num);
            return num;
        }

        private int patchAll(ref byte[] esModule)
        {
            this.fireDebug("Patching Fakesigning, ES_Identify, NAND Permissions and VP ...");
            int num = 0;
            byte[] second1 = new byte[4]
            {
                (byte) 32,
                (byte) 7,
                (byte) 35,
                (byte) 162
            };
            byte[] second2 = new byte[4]
            {
                (byte) 32,
                (byte) 7,
                (byte) 75,
                (byte) 11
            };
            byte[] second3 = new byte[4]
            {
                (byte) 40,
                (byte) 3,
                (byte) 209,
                (byte) 35
            };
            byte[] second4 = new byte[6]
            {
                (byte) 66,
                (byte) 139,
                (byte) 208,
                (byte) 1,
                (byte) 37,
                (byte) 102
            };
            byte[] second5 = new byte[4]
            {
                (byte) 210,
                (byte) 1,
                (byte) 78,
                (byte) 86
            };
            for (int firstIndex = 0; firstIndex < esModule.Length - 6; ++firstIndex)
            {
                this.fireProgress((firstIndex + 1) * 100 / esModule.Length);
                if (Shared.CompareByteArrays(esModule, firstIndex, second1, 0, 4) || Shared.CompareByteArrays(esModule, firstIndex, second2, 0, 4))
                {
                    this.fireDebug("   Patching Fakesigning at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 1] = (byte) 0;
                    firstIndex += 4;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second3, 0, 4))
                {
                    this.fireDebug("   Patching ES_Identify at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = (byte) 0;
                    esModule[firstIndex + 3] = (byte) 0;
                    firstIndex += 4;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second4, 0, 6))
                {
                    this.fireDebug("   Patching NAND Permissions at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex + 2] = (byte) 224;
                    firstIndex += 6;
                    ++num;
                }
                else if (Shared.CompareByteArrays(esModule, firstIndex, second5, 0, 4))
                {
                    this.fireDebug("   Patching VP at Offset: 0x{0}", (object) firstIndex.ToString("x8").ToUpper());
                    esModule[firstIndex] = (byte) 224;
                    firstIndex += 4;
                    ++num;
                }
            }
            this.fireDebug("Patching Fakesigning, ES_Identify, NAND Permissions and VP Finished... (Patches applied: {0})", (object) num);
            return num;
        }

        private void getEsIndex()
        {
            this.fireDebug("Scanning for ES Module...");
            string str = "$IOSVersion:";
            for (int index1 = this.wadFile.NumOfContents - 1; index1 >= 0; --index1)
            {
                this.fireDebug("   Scanning Content #{0} of {1}...", (object) (index1 + 1), (object) this.wadFile.NumOfContents);
                this.fireProgress((index1 + 1) * 100 / this.wadFile.NumOfContents);
                for (int index2 = 0; index2 < this.wadFile.Contents[index1].Length - 64; ++index2)
                {
                    if (Encoding.ASCII.GetString(this.wadFile.Contents[index1], index2, 12) == str)
                    {
                        int index3 = index2 + 12;
                        while (this.wadFile.Contents[index1][index3] == (byte) 32)
                        ++index3;
                        if (Encoding.ASCII.GetString(this.wadFile.Contents[index1], index3, 3) == "ES:")
                        {
                            this.fireDebug("    -> ES Module found!");
                            this.fireDebug("Scanning for ES Module Finished...");
                            this.esIndex = index1;
                            this.fireProgress(100);
                            return;
                        }
                    }
                }
            }
            this.fireDebug("/!\\/!\\/!\\ ES Module wasn't found! /!\\/!\\/!\\");
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
        private void fireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = this.Debug;
            if (debug == null)
                return;
            debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }

        private void fireProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = this.Progress;
            if (progress == null)
                return;
            progress(new object(), new ProgressChangedEventArgs(progressPercentage, (object) string.Empty));
        }
        #endregion
    }
}
