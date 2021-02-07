// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TPL
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace libWiiSharp
{
    public class TPL : IDisposable
    {
        private TPL_Header tplHeader = new TPL_Header();
        private List<TPL_TextureEntry> tplTextureEntries = new List<TPL_TextureEntry>();
        private List<TPL_TextureHeader> tplTextureHeaders = new List<TPL_TextureHeader>();
        private List<TPL_PaletteHeader> tplPaletteHeaders = new List<TPL_PaletteHeader>();
        private List<byte[]> textureData = new List<byte[]>();
        private List<byte[]> paletteData = new List<byte[]>();
        private bool isDisposed;

        public int NumOfTextures => (int)tplHeader.NumOfTextures;

        public event EventHandler<MessageEventArgs> Debug;

        ~TPL() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                tplHeader = null;
                tplTextureEntries.Clear();
                tplTextureEntries = null;
                tplTextureHeaders.Clear();
                tplTextureHeaders = null;
                tplPaletteHeaders.Clear();
                tplPaletteHeaders = null;
                textureData.Clear();
                textureData = null;
                paletteData.Clear();
                paletteData = null;
            }
            isDisposed = true;
        }

        public static TPL Load(string pathToTpl)
        {
            TPL tpl = new TPL();
            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(pathToTpl));
            try
            {
                tpl.ParseTpl(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
            return tpl;
        }

        public static TPL Load(byte[] tplFile)
        {
            TPL tpl = new TPL();
            MemoryStream memoryStream = new MemoryStream(tplFile);
            try
            {
                tpl.ParseTpl(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
            return tpl;
        }

        public static TPL Load(Stream tpl)
        {
            TPL tpl1 = new TPL();
            tpl1.ParseTpl(tpl);
            return tpl1;
        }

        public static TPL FromImage(
          string pathToImage,
          TPL_TextureFormat tplFormat,
          TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            return TPL.FromImages(new string[1] { pathToImage }, new TPL_TextureFormat[1]
            {
        tplFormat
            }, new TPL_PaletteFormat[1] { paletteFormat });
        }

        public static TPL FromImage(
          Image img,
          TPL_TextureFormat tplFormat,
          TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            return TPL.FromImages(new Image[1] { img }, new TPL_TextureFormat[1]
            {
        tplFormat
            }, new TPL_PaletteFormat[1] { paletteFormat });
        }

        public static TPL FromImages(
          string[] imagePaths,
          TPL_TextureFormat[] tplFormats,
          TPL_PaletteFormat[] paletteFormats)
        {
            if (tplFormats.Length < imagePaths.Length)
            {
                throw new Exception("You must specify a format for each image!");
            }

            List<Image> imageList = new List<Image>();
            foreach (string imagePath in imagePaths)
            {
                imageList.Add(Image.FromFile(imagePath));
            }

            TPL tpl = new TPL();
            tpl.PrivCreateFromImages(imageList.ToArray(), tplFormats, paletteFormats);
            return tpl;
        }

        public static TPL FromImages(
          Image[] images,
          TPL_TextureFormat[] tplFormats,
          TPL_PaletteFormat[] paletteFormats)
        {
            if (tplFormats.Length < images.Length)
            {
                throw new Exception("You must specify a format for each image!");
            }

            TPL tpl = new TPL();
            tpl.PrivCreateFromImages(images, tplFormats, paletteFormats);
            return tpl;
        }

        public void LoadFile(string pathToTpl)
        {
            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(pathToTpl));
            try
            {
                ParseTpl(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        public void LoadFile(byte[] tplFile)
        {
            MemoryStream memoryStream = new MemoryStream(tplFile);
            try
            {
                ParseTpl(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
            memoryStream.Dispose();
        }

        public void LoadFile(Stream tpl)
        {
            ParseTpl(tpl);
        }

        public void CreateFromImage(
          string pathToImage,
          TPL_TextureFormat tplFormat,
          TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            CreateFromImages(new string[1] { pathToImage }, new TPL_TextureFormat[1]
            {
        tplFormat
            }, new TPL_PaletteFormat[1] { paletteFormat });
        }

        public void CreateFromImage(
          Image img,
          TPL_TextureFormat tplFormat,
          TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            PrivCreateFromImages(new Image[1] { img }, new TPL_TextureFormat[1]
            {
        tplFormat
            }, new TPL_PaletteFormat[1] { paletteFormat });
        }

        public void CreateFromImages(
          string[] imagePaths,
          TPL_TextureFormat[] tplFormats,
          TPL_PaletteFormat[] paletteFormats)
        {
            if (tplFormats.Length < imagePaths.Length)
            {
                throw new Exception("You must specify a format for each image!");
            }

            List<Image> imageList = new List<Image>();
            foreach (string imagePath in imagePaths)
            {
                imageList.Add(Image.FromFile(imagePath));
            }

            PrivCreateFromImages(imageList.ToArray(), tplFormats, paletteFormats);
        }

        public void CreateFromImages(
          Image[] images,
          TPL_TextureFormat[] tplFormats,
          TPL_PaletteFormat[] paletteFormats)
        {
            if (tplFormats.Length < images.Length)
            {
                throw new Exception("You must specify a format for each image!");
            }

            PrivCreateFromImages(images, tplFormats, paletteFormats);
        }

        public void Save(string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            FileStream fileStream = new FileStream(savePath, FileMode.Create);
            try
            {
                WriteToStream(fileStream);
            }
            catch
            {
                fileStream.Dispose();
                throw;
            }
            fileStream.Dispose();
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                WriteToStream(memoryStream);
                return memoryStream;
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
        }

        public byte[] ToByteArray()
        {
            return ToMemoryStream().ToArray();
        }

        public Image ExtractTexture()
        {
            return ExtractTexture(0);
        }

        public Image ExtractTexture(int index)
        {
            byte[] data = tplTextureHeaders[index].TextureFormat switch
            {
                0 => FromI4(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                1 => FromI8(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                2 => FromIA4(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                3 => FromIA8(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                4 => FromRGB565(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                5 => FromRGB5A3(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                6 => FromRGBA8(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                8 => FromCI4(textureData[index], PaletteToRgba(index), tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                9 => FromCI8(textureData[index], PaletteToRgba(index), tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                10 => FromCI14X2(textureData[index], PaletteToRgba(index), tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                14 => FromCMP(textureData[index], tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight),
                _ => throw new FormatException("Unsupported Texture Format!"),
            };
            return RgbaToImage(data, tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight);
        }

        public void ExtractTexture(string savePath)
        {
            ExtractTexture(0, savePath);
        }

        public void ExtractTexture(int index, string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            Image texture = ExtractTexture(index);
            switch (Path.GetExtension(savePath).ToLower())
            {
                case ".tif":
                case ".tiff":
                    texture.Save(savePath, ImageFormat.Tiff);
                    break;
                case ".bmp":
                    texture.Save(savePath, ImageFormat.Bmp);
                    break;
                case ".gif":
                    texture.Save(savePath, ImageFormat.Gif);
                    break;
                case ".jpg":
                case ".jpeg":
                    texture.Save(savePath, ImageFormat.Jpeg);
                    break;
                default:
                    texture.Save(savePath, ImageFormat.Png);
                    break;
            }
        }

        public Image[] ExtractAllTextures()
        {
            List<Image> imageList = new List<Image>();
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                imageList.Add(ExtractTexture(index));
            }

            return imageList.ToArray();
        }

        public void ExtractAllTextures(string saveDir)
        {
            if (Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                ExtractTexture(index, saveDir + Path.DirectorySeparatorChar.ToString() + "Texture_" + index.ToString("x2") + ".png");
            }
        }

        public void AddTexture(
          string imagePath,
          TPL_TextureFormat tplFormat,
          TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            AddTexture(Image.FromFile(imagePath), tplFormat, paletteFormat);
        }

        public void AddTexture(Image img, TPL_TextureFormat tplFormat, TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
        {
            TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
            TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
            TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
            byte[] numArray1 = ImageToTpl(img, tplFormat);
            byte[] numArray2 = new byte[0];
            tplTextureHeader.TextureHeight = (ushort)img.Height;
            tplTextureHeader.TextureWidth = (ushort)img.Width;
            tplTextureHeader.TextureFormat = (uint)tplFormat;
            if (tplFormat == TPL_TextureFormat.CI4 || tplFormat == TPL_TextureFormat.CI8 || tplFormat == TPL_TextureFormat.CI14X2)
            {
                ColorIndexConverter colorIndexConverter = new ColorIndexConverter(ImageToRgba(img), img.Width, img.Height, tplFormat, paletteFormat);
                numArray1 = colorIndexConverter.Data;
                numArray2 = colorIndexConverter.Palette;
                tplPaletteHeader.NumberOfItems = (ushort)(numArray2.Length / 2);
                tplPaletteHeader.PaletteFormat = (uint)paletteFormat;
            }
            tplTextureEntries.Add(tplTextureEntry);
            tplTextureHeaders.Add(tplTextureHeader);
            tplPaletteHeaders.Add(tplPaletteHeader);
            textureData.Add(numArray1);
            paletteData.Add(numArray2);
            ++tplHeader.NumOfTextures;
        }

        public void RemoveTexture(int index)
        {
            if (tplHeader.NumOfTextures <= index)
            {
                return;
            }

            tplTextureEntries.RemoveAt(index);
            tplTextureHeaders.RemoveAt(index);
            tplPaletteHeaders.RemoveAt(index);
            textureData.RemoveAt(index);
            paletteData.RemoveAt(index);
            --tplHeader.NumOfTextures;
        }

        public TPL_TextureFormat GetTextureFormat(int index)
        {
            return (TPL_TextureFormat)tplTextureHeaders[index].TextureFormat;
        }

        public TPL_PaletteFormat GetPaletteFormat(int index)
        {
            return (TPL_PaletteFormat)tplPaletteHeaders[index].PaletteFormat;
        }

        public Size GetTextureSize(int index)
        {
            return new Size(tplTextureHeaders[index].TextureWidth, tplTextureHeaders[index].TextureHeight);
        }

        private void WriteToStream(Stream writeStream)
        {
            FireDebug("Writing TPL...");
            writeStream.Seek(0L, SeekOrigin.Begin);
            FireDebug("   Writing TPL Header... (Offset: 0x{0})", (object)writeStream.Position);
            tplHeader.Write(writeStream);
            int position1 = (int)writeStream.Position;
            writeStream.Seek(tplHeader.NumOfTextures * 8U, SeekOrigin.Current);
            int num = 0;
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                if (tplTextureHeaders[index].TextureFormat == 8U || tplTextureHeaders[index].TextureFormat == 9U || tplTextureHeaders[index].TextureFormat == 10U)
                {
                    ++num;
                }
            }
            int position2 = (int)writeStream.Position;
            writeStream.Seek(num * 12, SeekOrigin.Current);
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                if (tplTextureHeaders[index].TextureFormat == 8U || tplTextureHeaders[index].TextureFormat == 9U || tplTextureHeaders[index].TextureFormat == 10U)
                {
                    FireDebug("   Writing Palette of Texture #{1}... (Offset: 0x{0})", writeStream.Position, index + 1);
                    writeStream.Seek(Shared.AddPadding(writeStream.Position, 32), SeekOrigin.Begin);
                    tplPaletteHeaders[index].PaletteDataOffset = (uint)writeStream.Position;
                    writeStream.Write(paletteData[index], 0, paletteData[index].Length);
                }
            }
            int position3 = (int)writeStream.Position;
            writeStream.Seek(tplHeader.NumOfTextures * 36U, SeekOrigin.Current);
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                FireDebug("   Writing Texture #{1} of {2}... (Offset: 0x{0})", writeStream.Position, index + 1, tplHeader.NumOfTextures);
                writeStream.Seek(Shared.AddPadding((int)writeStream.Position, 32), SeekOrigin.Begin);
                tplTextureHeaders[index].TextureDataOffset = (uint)writeStream.Position;
                writeStream.Write(textureData[index], 0, textureData[index].Length);
            }
            while (writeStream.Position % 32L != 0L)
            {
                writeStream.WriteByte(0);
            }

            writeStream.Seek(position2, SeekOrigin.Begin);
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                if (tplTextureHeaders[index].TextureFormat == 8U || tplTextureHeaders[index].TextureFormat == 9U || tplTextureHeaders[index].TextureFormat == 10U)
                {
                    FireDebug("   Writing Palette Header of Texture #{1}... (Offset: 0x{0})", writeStream.Position, index + 1);
                    tplTextureEntries[index].PaletteHeaderOffset = (uint)writeStream.Position;
                    tplPaletteHeaders[index].Write(writeStream);
                }
            }
            writeStream.Seek(position3, SeekOrigin.Begin);
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                FireDebug("   Writing Texture Header #{1} of {2}... (Offset: 0x{0})", writeStream.Position, index + 1, tplHeader.NumOfTextures);
                tplTextureEntries[index].TextureHeaderOffset = (uint)writeStream.Position;
                tplTextureHeaders[index].Write(writeStream);
            }
            writeStream.Seek(position1, SeekOrigin.Begin);
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                FireDebug("   Writing Texture Entry #{1} of {2}... (Offset: 0x{0})", writeStream.Position, index + 1, tplHeader.NumOfTextures);
                tplTextureEntries[index].Write(writeStream);
            }
            FireDebug("Writing TPL Finished...");
        }

        private void ParseTpl(Stream tplFile)
        {
            FireDebug("Parsing TPL...");
            tplHeader = new TPL_Header();
            tplTextureEntries = new List<TPL_TextureEntry>();
            tplTextureHeaders = new List<TPL_TextureHeader>();
            tplPaletteHeaders = new List<TPL_PaletteHeader>();
            textureData = new List<byte[]>();
            paletteData = new List<byte[]>();
            tplFile.Seek(0L, SeekOrigin.Begin);
            byte[] buffer1 = new byte[4];
            FireDebug("   Reading TPL Header: Magic... (Offset: 0x{0})", (object)tplFile.Position);
            tplFile.Read(buffer1, 0, 4);
            if ((int)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int)tplHeader.TplMagic)
            {
                FireDebug("    -> Invalid Magic: 0x{0}", (object)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)));
                throw new Exception("TPL Header: Invalid Magic!");
            }
            FireDebug("   Reading TPL Header: NumOfTextures... (Offset: 0x{0})", (object)tplFile.Position);
            tplFile.Read(buffer1, 0, 4);
            tplHeader.NumOfTextures = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
            FireDebug("   Reading TPL Header: Headersize... (Offset: 0x{0})", (object)tplFile.Position);
            tplFile.Read(buffer1, 0, 4);
            if ((int)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int)tplHeader.HeaderSize)
            {
                FireDebug("    -> Invalid Headersize: 0x{0}", (object)Shared.Swap(BitConverter.ToUInt32(buffer1, 0)));
                throw new Exception("TPL Header: Invalid Headersize!");
            }
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                FireDebug("   Reading Texture Entry #{1} of {2}... (Offset: 0x{0})", tplFile.Position, index + 1, tplHeader.NumOfTextures);
                TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
                tplFile.Read(buffer1, 0, 4);
                tplTextureEntry.TextureHeaderOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureEntry.PaletteHeaderOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplTextureEntries.Add(tplTextureEntry);
            }
            for (int index = 0; index < tplHeader.NumOfTextures; ++index)
            {
                FireDebug("   Reading Texture Header #{1} of {2}... (Offset: 0x{0})", tplFile.Position, index + 1, tplHeader.NumOfTextures);
                TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
                TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
                tplFile.Seek(tplTextureEntries[index].TextureHeaderOffset, SeekOrigin.Begin);
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.TextureHeight = Shared.Swap(BitConverter.ToUInt16(buffer1, 0));
                tplTextureHeader.TextureWidth = Shared.Swap(BitConverter.ToUInt16(buffer1, 2));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.TextureFormat = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.TextureDataOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.WrapS = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.WrapT = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.MinFilter = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.MagFilter = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.LodBias = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                tplFile.Read(buffer1, 0, 4);
                tplTextureHeader.EdgeLod = buffer1[0];
                tplTextureHeader.MinLod = buffer1[1];
                tplTextureHeader.MaxLod = buffer1[2];
                tplTextureHeader.Unpacked = buffer1[3];
                if (tplTextureEntries[index].PaletteHeaderOffset != 0U)
                {
                    FireDebug("   Reading Palette Header #{1} of {2}... (Offset: 0x{0})", tplFile.Position, index + 1, tplHeader.NumOfTextures);
                    tplFile.Seek(tplTextureEntries[index].PaletteHeaderOffset, SeekOrigin.Begin);
                    tplFile.Read(buffer1, 0, 4);
                    tplPaletteHeader.NumberOfItems = Shared.Swap(BitConverter.ToUInt16(buffer1, 0));
                    tplPaletteHeader.Unpacked = buffer1[2];
                    tplPaletteHeader.Pad = buffer1[3];
                    tplFile.Read(buffer1, 0, 4);
                    tplPaletteHeader.PaletteFormat = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                    tplFile.Read(buffer1, 0, 4);
                    tplPaletteHeader.PaletteDataOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
                }
                tplFile.Seek(tplTextureHeader.TextureDataOffset, SeekOrigin.Begin);
                byte[] buffer2 = new byte[TextureByteSize((TPL_TextureFormat)tplTextureHeader.TextureFormat, tplTextureHeader.TextureWidth, tplTextureHeader.TextureHeight)];
                byte[] buffer3 = new byte[tplPaletteHeader.NumberOfItems * 2];
                FireDebug("   Reading Texture #{1} of {2}... (Offset: 0x{0})", tplFile.Position, index + 1, tplHeader.NumOfTextures);
                tplFile.Read(buffer2, 0, buffer2.Length);
                if (tplTextureEntries[index].PaletteHeaderOffset != 0U)
                {
                    FireDebug("   Reading Palette #{1} of {2}... (Offset: 0x{0})", tplFile.Position, index + 1, tplHeader.NumOfTextures);
                    tplFile.Seek(tplPaletteHeader.PaletteDataOffset, SeekOrigin.Begin);
                    tplFile.Read(buffer3, 0, buffer3.Length);
                }
                else
                {
                    buffer3 = new byte[0];
                }

                tplTextureHeaders.Add(tplTextureHeader);
                tplPaletteHeaders.Add(tplPaletteHeader);
                textureData.Add(buffer2);
                paletteData.Add(buffer3);
            }
        }

        private int TextureByteSize(TPL_TextureFormat tplFormat, int width, int height)
        {
            return tplFormat switch
            {
                TPL_TextureFormat.I4 => Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2,
                TPL_TextureFormat.I8 or TPL_TextureFormat.IA4 => Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4),
                TPL_TextureFormat.IA8 or TPL_TextureFormat.RGB565 or TPL_TextureFormat.RGB5A3 => Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2,
                TPL_TextureFormat.RGBA8 => Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 4,
                TPL_TextureFormat.CI4 => Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2,
                TPL_TextureFormat.CI8 => Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4),
                TPL_TextureFormat.CI14X2 => Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2,
                TPL_TextureFormat.CMP => width * height,
                _ => throw new FormatException("Unsupported Texture Format!"),
            };
        }

        private void PrivCreateFromImages(
          Image[] images,
          TPL_TextureFormat[] tplFormats,
          TPL_PaletteFormat[] paletteFormats)
        {
            tplHeader = new TPL_Header();
            tplTextureEntries = new List<TPL_TextureEntry>();
            tplTextureHeaders = new List<TPL_TextureHeader>();
            tplPaletteHeaders = new List<TPL_PaletteHeader>();
            textureData = new List<byte[]>();
            paletteData = new List<byte[]>();
            tplHeader.NumOfTextures = (uint)images.Length;
            for (int index = 0; index < images.Length; ++index)
            {
                Image image = images[index];
                TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
                TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
                TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
                byte[] numArray1 = ImageToTpl(image, tplFormats[index]);
                byte[] numArray2 = new byte[0];
                tplTextureHeader.TextureHeight = (ushort)image.Height;
                tplTextureHeader.TextureWidth = (ushort)image.Width;
                tplTextureHeader.TextureFormat = (uint)tplFormats[index];
                if (tplFormats[index] == TPL_TextureFormat.CI4 || tplFormats[index] == TPL_TextureFormat.CI8 || tplFormats[index] == TPL_TextureFormat.CI14X2)
                {
                    ColorIndexConverter colorIndexConverter = new ColorIndexConverter(ImageToRgba(image), image.Width, image.Height, tplFormats[index], paletteFormats[index]);
                    numArray1 = colorIndexConverter.Data;
                    numArray2 = colorIndexConverter.Palette;
                    tplPaletteHeader.NumberOfItems = (ushort)(numArray2.Length / 2);
                    tplPaletteHeader.PaletteFormat = (uint)paletteFormats[index];
                }
                tplTextureEntries.Add(tplTextureEntry);
                tplTextureHeaders.Add(tplTextureHeader);
                tplPaletteHeaders.Add(tplPaletteHeader);
                textureData.Add(numArray1);
                paletteData.Add(numArray2);
            }
        }

        private byte[] ImageToTpl(Image img, TPL_TextureFormat tplFormat)
        {
            return tplFormat switch
            {
                TPL_TextureFormat.I4 => ToI4((Bitmap)img),
                TPL_TextureFormat.I8 => ToI8((Bitmap)img),
                TPL_TextureFormat.IA4 => ToIA4((Bitmap)img),
                TPL_TextureFormat.IA8 => ToIA8((Bitmap)img),
                TPL_TextureFormat.RGB565 => ToRGB565((Bitmap)img),
                TPL_TextureFormat.RGB5A3 => ToRGB5A3((Bitmap)img),
                TPL_TextureFormat.RGBA8 => ToRGBA8((Bitmap)img),
                TPL_TextureFormat.CI4 or TPL_TextureFormat.CI8 or TPL_TextureFormat.CI14X2 => new byte[0],
                _ => throw new FormatException("Format not supported!\nCurrently, images can only be converted to the following formats:\nI4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CI4, CI8 , CI14X2."),
            };
        }

        private uint[] ImageToRgba(Image img)
        {
            Bitmap bitmap = (Bitmap)img;
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] numArray = new byte[bitmapdata.Height * Math.Abs(bitmapdata.Stride)];
            Marshal.Copy(bitmapdata.Scan0, numArray, 0, numArray.Length);
            bitmap.UnlockBits(bitmapdata);
            return Shared.ByteArrayToUIntArray(numArray);
        }

        private Bitmap RgbaToImage(byte[] data, int width, int height)
        {
            if (width == 0)
            {
                width = 1;
            }

            if (height == 0)
            {
                height = 1;
            }

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                Marshal.Copy(data, 0, bitmapdata.Scan0, data.Length);
                bitmap.UnlockBits(bitmapdata);
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private uint[] PaletteToRgba(int index)
        {
            TPL_PaletteFormat paletteFormat = (TPL_PaletteFormat)tplPaletteHeaders[index].PaletteFormat;
            int numberOfItems = tplPaletteHeaders[index].NumberOfItems;
            uint[] numArray = new uint[numberOfItems];
            for (int index1 = 0; index1 < numberOfItems; ++index1)
            {
                if (index1 < numberOfItems)
                {
                    ushort uint16 = BitConverter.ToUInt16(new byte[2]
                    {
            paletteData[index][index1 * 2 + 1],
            paletteData[index][index1 * 2]
                    }, 0);
                    int num1;
                    int num2;
                    int num3;
                    int num4;
                    switch (paletteFormat)
                    {
                        case TPL_PaletteFormat.IA8:
                            num1 = uint16 & byte.MaxValue;
                            num2 = num1;
                            num3 = num1;
                            num4 = uint16 >> 8;
                            break;
                        case TPL_PaletteFormat.RGB565:
                            num2 = (uint16 >> 11 & 31) << 3 & byte.MaxValue;
                            num3 = (uint16 >> 5 & 63) << 2 & byte.MaxValue;
                            num1 = (uint16 & 31) << 3 & byte.MaxValue;
                            num4 = byte.MaxValue;
                            break;
                        default:
                            if ((uint16 & 32768) != 0)
                            {
                                num4 = byte.MaxValue;
                                num2 = (uint16 >> 10 & 31) * byte.MaxValue / 31;
                                num3 = (uint16 >> 5 & 31) * byte.MaxValue / 31;
                                num1 = (uint16 & 31) * byte.MaxValue / 31;
                                break;
                            }
                            num4 = (uint16 >> 12 & 7) * byte.MaxValue / 7;
                            num2 = (uint16 >> 8 & 15) * byte.MaxValue / 15;
                            num3 = (uint16 >> 4 & 15) * byte.MaxValue / 15;
                            num1 = (uint16 & 15) * byte.MaxValue / 15;
                            break;
                    }
                    numArray[index1] = (uint)(num1 | num3 << 8 | num2 << 16 | num4 << 24);
                }
            }
            return numArray;
        }

        private int Avg(int w0, int w1, int c0, int c1)
        {
            int num1 = c0 >> 11;
            int num2 = c1 >> 11;
            int num3 = (w0 * num1 + w1 * num2) / (w0 + w1) << 11 & ushort.MaxValue;
            int num4 = c0 >> 5 & 63;
            int num5 = c1 >> 5 & 63;
            int num6 = (w0 * num4 + w1 * num5) / (w0 + w1) << 5 & ushort.MaxValue;
            int num7 = num3 | num6;
            int num8 = c0 & 31;
            int num9 = c1 & 31;
            int num10 = (w0 * num8 + w1 * num9) / (w0 + w1);
            return num7 | num10;
        }

        private byte[] FromRGBA8(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = 0; index3 < 2; ++index3)
                    {
                        for (int index4 = index1; index4 < index1 + 4; ++index4)
                        {
                            for (int index5 = index2; index5 < index2 + 4; ++index5)
                            {
                                ushort num2 = Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
                                if (index5 < width && index4 < height)
                                {
                                    if (index3 == 0)
                                    {
                                        int num3 = num2 >> 8 & byte.MaxValue;
                                        int num4 = num2 & byte.MaxValue;
                                        array[index5 + index4 * width] |= (uint)(num4 << 16 | num3 << 24);
                                    }
                                    else
                                    {
                                        int num3 = num2 >> 8 & byte.MaxValue;
                                        int num4 = num2 & byte.MaxValue;
                                        array[index5 + index4 * width] |= (uint)(num3 << 8 | num4);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToRGBA8(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int index1 = 0;
            int num1 = 0;
            byte[] numArray1 = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 4];
            uint[] numArray2 = new uint[32];
            uint[] numArray3 = new uint[32];
            uint[] numArray4 = new uint[32];
            uint[] numArray5 = new uint[32];
            for (int index2 = 0; index2 < height; index2 += 4)
            {
                for (int index3 = 0; index3 < width; index3 += 4)
                {
                    for (int index4 = index2; index4 < index2 + 4; ++index4)
                    {
                        for (int index5 = index3; index5 < index3 + 4; ++index5)
                        {
                            uint num2 = index4 >= height || index5 >= width ? 0U : rgba[index5 + index4 * width];
                            numArray2[index1] = num2 >> 16 & byte.MaxValue;
                            numArray3[index1] = num2 >> 8 & byte.MaxValue;
                            numArray4[index1] = num2 & byte.MaxValue;
                            numArray5[index1] = num2 >> 24 & byte.MaxValue;
                            ++index1;
                        }
                    }
                    if (index1 == 16)
                    {
                        for (int index4 = 0; index4 < 16; ++index4)
                        {
                            byte[] numArray6 = numArray1;
                            int index5 = num1;
                            int num2 = index5 + 1;
                            int num3 = (byte)numArray5[index4];
                            numArray6[index5] = (byte)num3;
                            byte[] numArray7 = numArray1;
                            int index6 = num2;
                            num1 = index6 + 1;
                            int num4 = (byte)numArray2[index4];
                            numArray7[index6] = (byte)num4;
                        }
                        for (int index4 = 0; index4 < 16; ++index4)
                        {
                            byte[] numArray6 = numArray1;
                            int index5 = num1;
                            int num2 = index5 + 1;
                            int num3 = (byte)numArray3[index4];
                            numArray6[index5] = (byte)num3;
                            byte[] numArray7 = numArray1;
                            int index6 = num2;
                            num1 = index6 + 1;
                            int num4 = (byte)numArray4[index4];
                            numArray7[index6] = (byte)num4;
                        }
                        index1 = 0;
                    }
                }
            }
            return numArray1;
        }

        private byte[] FromRGB5A3(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            ushort num2 = Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
                            if (index3 < height && index4 < width)
                            {
                                int num3;
                                int num4;
                                int num5;
                                int num6;
                                if ((num2 & 32768) != 0)
                                {
                                    num3 = (num2 >> 10 & 31) * byte.MaxValue / 31;
                                    num4 = (num2 >> 5 & 31) * byte.MaxValue / 31;
                                    num5 = (num2 & 31) * byte.MaxValue / 31;
                                    num6 = byte.MaxValue;
                                }
                                else
                                {
                                    num6 = (num2 >> 12 & 7) * byte.MaxValue / 7;
                                    num3 = (num2 >> 8 & 15) * byte.MaxValue / 15;
                                    num4 = (num2 >> 4 & 15) * byte.MaxValue / 15;
                                    num5 = (num2 & 15) * byte.MaxValue / 15;
                                }
                                array[index3 * width + index4] = (uint)(num5 | num4 << 8 | num3 << 16 | num6 << 24);
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToRGB5A3(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = -1;
            byte[] numArray = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2];
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            int num2;
                            if (index3 >= height || index4 >= width)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                int num3 = (int)rgba[index4 + index3 * width];
                                int num4 = 0;
                                int num5 = num3 >> 16 & byte.MaxValue;
                                int num6 = num3 >> 8 & byte.MaxValue;
                                int num7 = num3 & byte.MaxValue;
                                int num8 = num3 >> 24 & byte.MaxValue;
                                if (num8 <= 218)
                                {
                                    int num9 = num4 & -32769;
                                    int num10 = num5 * 15 / byte.MaxValue & 15;
                                    int num11 = num6 * 15 / byte.MaxValue & 15;
                                    int num12 = num7 * 15 / byte.MaxValue & 15;
                                    int num13 = num8 * 7 / byte.MaxValue & 7;
                                    num2 = num9 | (num13 << 12 | num10 << 8 | num11 << 4 | num12);
                                }
                                else
                                {
                                    num2 = num4 | 32768 | ((num5 * 31 / byte.MaxValue & 31) << 10 | (num6 * 31 / byte.MaxValue & 31) << 5 | num7 * 31 / byte.MaxValue & 31);
                                }
                            }
                            int num14;
                            numArray[num14 = num1 + 1] = (byte)(num2 >> 8);
                            numArray[num1 = num14 + 1] = (byte)(num2 & byte.MaxValue);
                        }
                    }
                }
            }
            return numArray;
        }

        private byte[] FromRGB565(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            ushort num2 = Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
                            if (index3 < height && index4 < width)
                            {
                                int num3 = (num2 >> 11 & 31) << 3 & byte.MaxValue;
                                int num4 = (num2 >> 5 & 63) << 2 & byte.MaxValue;
                                int num5 = (num2 & 31) << 3 & byte.MaxValue;
                                array[index3 * width + index4] = (uint)(num5 | num4 << 8 | num3 << 16 | -16777216);
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToRGB565(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = -1;
            byte[] numArray = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2];
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            ushort num2;
                            if (index3 >= height || index4 >= width)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                int num3 = (int)rgba[index4 + index3 * width];
                                num2 = (ushort)((uint)((int)(((uint)num3 >> 16 & byte.MaxValue) >> 3) << 11 | (int)(((uint)num3 >> 8 & byte.MaxValue) >> 2) << 5) | (uint)(num3 & byte.MaxValue) >> 3);
                            }
                            int num4;
                            numArray[num4 = num1 + 1] = (byte)((uint)num2 >> 8);
                            numArray[num1 = num4 + 1] = (byte)(num2 & (uint)byte.MaxValue);
                        }
                    }
                }
            }
            return numArray;
        }

        private byte[] FromI4(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 8; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; index4 += 2)
                        {
                            int num2 = tpl[num1++];
                            if (index3 < height && index4 < width)
                            {
                                int num3 = (num2 >> 4) * byte.MaxValue / 15;
                                array[index3 * width + index4] = (uint)(num3 | num3 << 8 | num3 << 16 | -16777216);
                                int num4 = (num2 & 15) * byte.MaxValue / 15;
                                if (index3 * width + index4 + 1 < array.Length)
                                {
                                    array[index3 * width + index4 + 1] = (uint)(num4 | num4 << 8 | num4 << 16 | -16777216);
                                }
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToI4(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = 0;
            byte[] numArray = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2];
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 8; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; index4 += 2)
                        {
                            byte num2;
                            if (index4 >= width || index3 >= height)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                int num3 = (int)rgba[index4 + index3 * width];
                                int num4 = (int)(((uint)(num3 & byte.MaxValue) + ((uint)num3 >> 8 & byte.MaxValue) + ((uint)num3 >> 16 & byte.MaxValue)) / 3U) & byte.MaxValue;
                                int num5 = index4 + index3 * width + 1 < rgba.Length ? (int)rgba[index4 + index3 * width + 1] : 0;
                                uint num6 = ((uint)(num5 & byte.MaxValue) + ((uint)num5 >> 8 & byte.MaxValue) + ((uint)num5 >> 16 & byte.MaxValue)) / 3U & byte.MaxValue;
                                num2 = (byte)((int)((uint)(num4 * 15) / byte.MaxValue) << 4 | (int)(num6 * 15U / byte.MaxValue) & 15);
                            }
                            numArray[num1++] = num2;
                        }
                    }
                }
            }
            return numArray;
        }

        private byte[] FromI8(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            int num2 = tpl[num1++];
                            if (index3 < height && index4 < width)
                            {
                                array[index3 * width + index4] = (uint)(num2 | num2 << 8 | num2 << 16 | -16777216);
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToI8(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = 0;
            byte[] numArray = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4)];
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            byte num2;
                            if (index4 >= width || index3 >= height)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                int num3 = (int)rgba[index4 + index3 * width];
                                num2 = (byte)(((uint)(num3 & byte.MaxValue) + ((uint)num3 >> 8 & byte.MaxValue) + ((uint)num3 >> 16 & byte.MaxValue)) / 3U & byte.MaxValue);
                            }
                            numArray[num1++] = num2;
                        }
                    }
                }
            }
            return numArray;
        }

        private byte[] FromIA4(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            int num2 = tpl[num1++];
                            if (index3 < height && index4 < width)
                            {
                                int num3 = (num2 & 15) * byte.MaxValue / 15 & byte.MaxValue;
                                int num4 = (num2 >> 4) * byte.MaxValue / 15 & byte.MaxValue;
                                array[index3 * width + index4] = (uint)(num3 | num3 << 8 | num3 << 16 | num4 << 24);
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToIA4(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = 0;
            byte[] numArray = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4)];
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            byte num2;
                            if (index4 >= width || index3 >= height)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                uint num3 = rgba[index4 + index3 * width];
                                num2 = (byte)((int)((uint)(((int)(((num3 & byte.MaxValue) + (num3 >> 8 & byte.MaxValue) + (num3 >> 16 & byte.MaxValue)) / 3U) & byte.MaxValue) * 15) / byte.MaxValue) & 15 | (int)((num3 >> 24 & byte.MaxValue) * 15U / byte.MaxValue) << 4);
                            }
                            numArray[num1++] = num2;
                        }
                    }
                }
            }
            return numArray;
        }

        private byte[] FromIA8(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            int num2 = Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
                            if (index3 < height && index4 < width)
                            {
                                uint num3 = (uint)(num2 >> 8);
                                uint num4 = (uint)(num2 & byte.MaxValue);
                                array[index3 * width + index4] = (uint)((int)num4 | (int)num4 << 8 | (int)num4 << 16 | (int)num3 << 24);
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] ToIA8(Bitmap img)
        {
            uint[] rgba = ImageToRgba(img);
            int width = img.Width;
            int height = img.Height;
            int num1 = 0;
            byte[] numArray1 = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2];
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            ushort num2;
                            if (index4 >= width || index3 >= height)
                            {
                                num2 = 0;
                            }
                            else
                            {
                                int num3 = (int)rgba[index4 + index3 * width];
                                num2 = (ushort)((uint)(((int)((uint)num3 >> 24) & byte.MaxValue) << 8) | ((uint)(num3 & byte.MaxValue) + ((uint)num3 >> 8 & byte.MaxValue) + ((uint)num3 >> 16 & byte.MaxValue)) / 3U & byte.MaxValue);
                            }
                            Array.Reverse(BitConverter.GetBytes(num2));
                            byte[] numArray2 = numArray1;
                            int index5 = num1;
                            int num4 = index5 + 1;
                            int num5 = (byte)((uint)num2 >> 8);
                            numArray2[index5] = (byte)num5;
                            byte[] numArray3 = numArray1;
                            int index6 = num4;
                            num1 = index6 + 1;
                            int num6 = (byte)(num2 & (uint)byte.MaxValue);
                            numArray3[index6] = (byte)num6;
                        }
                    }
                }
            }
            return numArray1;
        }

        private byte[] FromCI4(byte[] tpl, uint[] paletteData, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 8; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; index4 += 2)
                        {
                            byte num2 = tpl[num1++];
                            if (index3 < height && index4 < width)
                            {
                                array[index3 * width + index4] = paletteData[num2 >> 4];
                                if (index3 * width + index4 + 1 < array.Length)
                                {
                                    array[index3 * width + index4 + 1] = paletteData[num2 & 15];
                                }
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] FromCI8(byte[] tpl, uint[] paletteData, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 8; ++index4)
                        {
                            ushort num2 = tpl[num1++];
                            if (index3 < height && index4 < width)
                            {
                                array[index3 * width + index4] = paletteData[num2];
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] FromCI14X2(byte[] tpl, uint[] paletteData, int width, int height)
        {
            uint[] array = new uint[width * height];
            int num1 = 0;
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = index1; index3 < index1 + 4; ++index3)
                    {
                        for (int index4 = index2; index4 < index2 + 4; ++index4)
                        {
                            ushort num2 = Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
                            if (index3 < height && index4 < width)
                            {
                                array[index3 * width + index4] = paletteData[num2 & 16383];
                            }
                        }
                    }
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private byte[] FromCMP(byte[] tpl, int width, int height)
        {
            uint[] array = new uint[width * height];
            ushort[] numArray1 = new ushort[4];
            int[] numArray2 = new int[4];
            int index1 = 0;
            for (int index2 = 0; index2 < height; ++index2)
            {
                for (int index3 = 0; index3 < width; ++index3)
                {
                    int num1 = Shared.AddPadding(width, 8);
                    int num2 = index3 & 3;
                    int num3 = index3 >> 2 & 1;
                    int num4 = index3 >> 3;
                    int num5 = index2 & 3;
                    int num6 = index2 >> 2 & 1;
                    int num7 = index2 >> 3;
                    int startIndex = 8 * num3 + 16 * num6 + 32 * num4 + 4 * num1 * num7;
                    numArray1[0] = Shared.Swap(BitConverter.ToUInt16(tpl, startIndex));
                    numArray1[1] = Shared.Swap(BitConverter.ToUInt16(tpl, startIndex + 2));
                    if (numArray1[0] > numArray1[1])
                    {
                        numArray1[2] = (ushort)Avg(2, 1, numArray1[0], numArray1[1]);
                        numArray1[3] = (ushort)Avg(1, 2, numArray1[0], numArray1[1]);
                    }
                    else
                    {
                        numArray1[2] = (ushort)Avg(1, 1, numArray1[0], numArray1[1]);
                        numArray1[3] = 0;
                    }
                    uint num8 = Shared.Swap(BitConverter.ToUInt32(tpl, startIndex + 4));
                    int num9 = num2 + 4 * num5;
                    int num10 = numArray1[(int)(num8 >> 30 - 2 * num9) & 3];
                    numArray2[0] = num10 >> 8 & 248;
                    numArray2[1] = num10 >> 3 & 248;
                    numArray2[2] = num10 << 3 & 248;
                    numArray2[3] = byte.MaxValue;
                    if (((int)(num8 >> 30 - 2 * num9) & 3) == 3 && numArray1[0] <= numArray1[1])
                    {
                        numArray2[3] = 0;
                    }

                    array[index1] = (uint)(numArray2[0] << 16 | numArray2[1] << 8 | numArray2[2] | numArray2[3] << 24);
                    ++index1;
                }
            }
            return Shared.UIntArrayToByteArray(array);
        }

        private void FireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = Debug;
            if (debug == null)
            {
                return;
            }

            debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }
    }
}
