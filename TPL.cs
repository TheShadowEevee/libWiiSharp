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

    public int NumOfTextures => (int) this.tplHeader.NumOfTextures;

    public event EventHandler<MessageEventArgs> Debug;

    ~TPL() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !this.isDisposed)
      {
        this.tplHeader = (TPL_Header) null;
        this.tplTextureEntries.Clear();
        this.tplTextureEntries = (List<TPL_TextureEntry>) null;
        this.tplTextureHeaders.Clear();
        this.tplTextureHeaders = (List<TPL_TextureHeader>) null;
        this.tplPaletteHeaders.Clear();
        this.tplPaletteHeaders = (List<TPL_PaletteHeader>) null;
        this.textureData.Clear();
        this.textureData = (List<byte[]>) null;
        this.paletteData.Clear();
        this.paletteData = (List<byte[]>) null;
      }
      this.isDisposed = true;
    }

    public static TPL Load(string pathToTpl)
    {
      TPL tpl = new TPL();
      MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(pathToTpl));
      try
      {
        tpl.parseTpl((Stream) memoryStream);
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
        tpl.parseTpl((Stream) memoryStream);
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
      tpl1.parseTpl(tpl);
      return tpl1;
    }

    public static TPL FromImage(
      string pathToImage,
      TPL_TextureFormat tplFormat,
      TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      return TPL.FromImages(new string[1]{ pathToImage }, new TPL_TextureFormat[1]
      {
        tplFormat
      }, new TPL_PaletteFormat[1]{ paletteFormat });
    }

    public static TPL FromImage(
      Image img,
      TPL_TextureFormat tplFormat,
      TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      return TPL.FromImages(new Image[1]{ img }, new TPL_TextureFormat[1]
      {
        tplFormat
      }, new TPL_PaletteFormat[1]{ paletteFormat });
    }

    public static TPL FromImages(
      string[] imagePaths,
      TPL_TextureFormat[] tplFormats,
      TPL_PaletteFormat[] paletteFormats)
    {
      if (tplFormats.Length < imagePaths.Length)
        throw new Exception("You must specify a format for each image!");
      List<Image> imageList = new List<Image>();
      foreach (string imagePath in imagePaths)
        imageList.Add(Image.FromFile(imagePath));
      TPL tpl = new TPL();
      tpl.createFromImages(imageList.ToArray(), tplFormats, paletteFormats);
      return tpl;
    }

    public static TPL FromImages(
      Image[] images,
      TPL_TextureFormat[] tplFormats,
      TPL_PaletteFormat[] paletteFormats)
    {
      if (tplFormats.Length < images.Length)
        throw new Exception("You must specify a format for each image!");
      TPL tpl = new TPL();
      tpl.createFromImages(images, tplFormats, paletteFormats);
      return tpl;
    }

    public void LoadFile(string pathToTpl)
    {
      MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(pathToTpl));
      try
      {
        this.parseTpl((Stream) memoryStream);
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
        this.parseTpl((Stream) memoryStream);
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
      memoryStream.Dispose();
    }

    public void LoadFile(Stream tpl) => this.parseTpl(tpl);

    public void CreateFromImage(
      string pathToImage,
      TPL_TextureFormat tplFormat,
      TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      this.CreateFromImages(new string[1]{ pathToImage }, new TPL_TextureFormat[1]
      {
        tplFormat
      }, new TPL_PaletteFormat[1]{ paletteFormat });
    }

    public void CreateFromImage(
      Image img,
      TPL_TextureFormat tplFormat,
      TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      this.CreateFromImages(new Image[1]{ img }, new TPL_TextureFormat[1]
      {
        tplFormat
      }, new TPL_PaletteFormat[1]{ paletteFormat });
    }

    public void CreateFromImages(
      string[] imagePaths,
      TPL_TextureFormat[] tplFormats,
      TPL_PaletteFormat[] paletteFormats)
    {
      if (tplFormats.Length < imagePaths.Length)
        throw new Exception("You must specify a format for each image!");
      List<Image> imageList = new List<Image>();
      foreach (string imagePath in imagePaths)
        imageList.Add(Image.FromFile(imagePath));
      this.createFromImages(imageList.ToArray(), tplFormats, paletteFormats);
    }

    public void CreateFromImages(
      Image[] images,
      TPL_TextureFormat[] tplFormats,
      TPL_PaletteFormat[] paletteFormats)
    {
      if (tplFormats.Length < images.Length)
        throw new Exception("You must specify a format for each image!");
      this.createFromImages(images, tplFormats, paletteFormats);
    }

    public void Save(string savePath)
    {
      if (File.Exists(savePath))
        File.Delete(savePath);
      FileStream fileStream = new FileStream(savePath, FileMode.Create);
      try
      {
        this.writeToStream((Stream) fileStream);
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
        this.writeToStream((Stream) memoryStream);
        return memoryStream;
      }
      catch
      {
        memoryStream.Dispose();
        throw;
      }
    }

    public byte[] ToByteArray() => this.ToMemoryStream().ToArray();

    public Image ExtractTexture() => this.ExtractTexture(0);

    public Image ExtractTexture(int index)
    {
      byte[] data;
      switch (this.tplTextureHeaders[index].TextureFormat)
      {
        case 0:
          data = this.fromI4(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 1:
          data = this.fromI8(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 2:
          data = this.fromIA4(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 3:
          data = this.fromIA8(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 4:
          data = this.fromRGB565(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 5:
          data = this.fromRGB5A3(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 6:
          data = this.fromRGBA8(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 8:
          data = this.fromCI4(this.textureData[index], this.paletteToRgba(index), (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 9:
          data = this.fromCI8(this.textureData[index], this.paletteToRgba(index), (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 10:
          data = this.fromCI14X2(this.textureData[index], this.paletteToRgba(index), (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        case 14:
          data = this.fromCMP(this.textureData[index], (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
          break;
        default:
          throw new FormatException("Unsupported Texture Format!");
      }
      return (Image) this.rgbaToImage(data, (int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);
    }

    public void ExtractTexture(string savePath) => this.ExtractTexture(0, savePath);

    public void ExtractTexture(int index, string savePath)
    {
      if (File.Exists(savePath))
        File.Delete(savePath);
      Image texture = this.ExtractTexture(index);
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
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
        imageList.Add(this.ExtractTexture(index));
      return imageList.ToArray();
    }

    public void ExtractAllTextures(string saveDir)
    {
      if (Directory.Exists(saveDir))
        Directory.CreateDirectory(saveDir);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
        this.ExtractTexture(index, saveDir + Path.DirectorySeparatorChar.ToString() + "Texture_" + index.ToString("x2") + ".png");
    }

    public void AddTexture(
      string imagePath,
      TPL_TextureFormat tplFormat,
      TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      this.AddTexture(Image.FromFile(imagePath), tplFormat, paletteFormat);
    }

    public void AddTexture(Image img, TPL_TextureFormat tplFormat, TPL_PaletteFormat paletteFormat = TPL_PaletteFormat.RGB5A3)
    {
      TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
      TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
      TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
      byte[] numArray1 = this.imageToTpl(img, tplFormat);
      byte[] numArray2 = new byte[0];
      tplTextureHeader.TextureHeight = (ushort) img.Height;
      tplTextureHeader.TextureWidth = (ushort) img.Width;
      tplTextureHeader.TextureFormat = (uint) tplFormat;
      if (tplFormat == TPL_TextureFormat.CI4 || tplFormat == TPL_TextureFormat.CI8 || tplFormat == TPL_TextureFormat.CI14X2)
      {
        ColorIndexConverter colorIndexConverter = new ColorIndexConverter(this.imageToRgba(img), img.Width, img.Height, tplFormat, paletteFormat);
        numArray1 = colorIndexConverter.Data;
        numArray2 = colorIndexConverter.Palette;
        tplPaletteHeader.NumberOfItems = (ushort) (numArray2.Length / 2);
        tplPaletteHeader.PaletteFormat = (uint) paletteFormat;
      }
      this.tplTextureEntries.Add(tplTextureEntry);
      this.tplTextureHeaders.Add(tplTextureHeader);
      this.tplPaletteHeaders.Add(tplPaletteHeader);
      this.textureData.Add(numArray1);
      this.paletteData.Add(numArray2);
      ++this.tplHeader.NumOfTextures;
    }

    public void RemoveTexture(int index)
    {
      if ((long) this.tplHeader.NumOfTextures <= (long) index)
        return;
      this.tplTextureEntries.RemoveAt(index);
      this.tplTextureHeaders.RemoveAt(index);
      this.tplPaletteHeaders.RemoveAt(index);
      this.textureData.RemoveAt(index);
      this.paletteData.RemoveAt(index);
      --this.tplHeader.NumOfTextures;
    }

    public TPL_TextureFormat GetTextureFormat(int index) => (TPL_TextureFormat) this.tplTextureHeaders[index].TextureFormat;

    public TPL_PaletteFormat GetPaletteFormat(int index) => (TPL_PaletteFormat) this.tplPaletteHeaders[index].PaletteFormat;

    public Size GetTextureSize(int index) => new Size((int) this.tplTextureHeaders[index].TextureWidth, (int) this.tplTextureHeaders[index].TextureHeight);

    private void writeToStream(Stream writeStream)
    {
      this.fireDebug("Writing TPL...");
      writeStream.Seek(0L, SeekOrigin.Begin);
      this.fireDebug("   Writing TPL Header... (Offset: 0x{0})", (object) writeStream.Position);
      this.tplHeader.Write(writeStream);
      int position1 = (int) writeStream.Position;
      writeStream.Seek((long) (this.tplHeader.NumOfTextures * 8U), SeekOrigin.Current);
      int num = 0;
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        if (this.tplTextureHeaders[index].TextureFormat == 8U || this.tplTextureHeaders[index].TextureFormat == 9U || this.tplTextureHeaders[index].TextureFormat == 10U)
          ++num;
      }
      int position2 = (int) writeStream.Position;
      writeStream.Seek((long) (num * 12), SeekOrigin.Current);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        if (this.tplTextureHeaders[index].TextureFormat == 8U || this.tplTextureHeaders[index].TextureFormat == 9U || this.tplTextureHeaders[index].TextureFormat == 10U)
        {
          this.fireDebug("   Writing Palette of Texture #{1}... (Offset: 0x{0})", (object) writeStream.Position, (object) (index + 1));
          writeStream.Seek(Shared.AddPadding(writeStream.Position, 32), SeekOrigin.Begin);
          this.tplPaletteHeaders[index].PaletteDataOffset = (uint) writeStream.Position;
          writeStream.Write(this.paletteData[index], 0, this.paletteData[index].Length);
        }
      }
      int position3 = (int) writeStream.Position;
      writeStream.Seek((long) (this.tplHeader.NumOfTextures * 36U), SeekOrigin.Current);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        this.fireDebug("   Writing Texture #{1} of {2}... (Offset: 0x{0})", (object) writeStream.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        writeStream.Seek((long) Shared.AddPadding((int) writeStream.Position, 32), SeekOrigin.Begin);
        this.tplTextureHeaders[index].TextureDataOffset = (uint) writeStream.Position;
        writeStream.Write(this.textureData[index], 0, this.textureData[index].Length);
      }
      while (writeStream.Position % 32L != 0L)
        writeStream.WriteByte((byte) 0);
      writeStream.Seek((long) position2, SeekOrigin.Begin);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        if (this.tplTextureHeaders[index].TextureFormat == 8U || this.tplTextureHeaders[index].TextureFormat == 9U || this.tplTextureHeaders[index].TextureFormat == 10U)
        {
          this.fireDebug("   Writing Palette Header of Texture #{1}... (Offset: 0x{0})", (object) writeStream.Position, (object) (index + 1));
          this.tplTextureEntries[index].PaletteHeaderOffset = (uint) writeStream.Position;
          this.tplPaletteHeaders[index].Write(writeStream);
        }
      }
      writeStream.Seek((long) position3, SeekOrigin.Begin);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        this.fireDebug("   Writing Texture Header #{1} of {2}... (Offset: 0x{0})", (object) writeStream.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        this.tplTextureEntries[index].TextureHeaderOffset = (uint) writeStream.Position;
        this.tplTextureHeaders[index].Write(writeStream);
      }
      writeStream.Seek((long) position1, SeekOrigin.Begin);
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        this.fireDebug("   Writing Texture Entry #{1} of {2}... (Offset: 0x{0})", (object) writeStream.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        this.tplTextureEntries[index].Write(writeStream);
      }
      this.fireDebug("Writing TPL Finished...");
    }

    private void parseTpl(Stream tplFile)
    {
      this.fireDebug("Parsing TPL...");
      this.tplHeader = new TPL_Header();
      this.tplTextureEntries = new List<TPL_TextureEntry>();
      this.tplTextureHeaders = new List<TPL_TextureHeader>();
      this.tplPaletteHeaders = new List<TPL_PaletteHeader>();
      this.textureData = new List<byte[]>();
      this.paletteData = new List<byte[]>();
      tplFile.Seek(0L, SeekOrigin.Begin);
      byte[] buffer1 = new byte[4];
      this.fireDebug("   Reading TPL Header: Magic... (Offset: 0x{0})", (object) tplFile.Position);
      tplFile.Read(buffer1, 0, 4);
      if ((int) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int) this.tplHeader.TplMagic)
      {
        this.fireDebug("    -> Invalid Magic: 0x{0}", (object) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)));
        throw new Exception("TPL Header: Invalid Magic!");
      }
      this.fireDebug("   Reading TPL Header: NumOfTextures... (Offset: 0x{0})", (object) tplFile.Position);
      tplFile.Read(buffer1, 0, 4);
      this.tplHeader.NumOfTextures = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
      this.fireDebug("   Reading TPL Header: Headersize... (Offset: 0x{0})", (object) tplFile.Position);
      tplFile.Read(buffer1, 0, 4);
      if ((int) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)) != (int) this.tplHeader.HeaderSize)
      {
        this.fireDebug("    -> Invalid Headersize: 0x{0}", (object) Shared.Swap(BitConverter.ToUInt32(buffer1, 0)));
        throw new Exception("TPL Header: Invalid Headersize!");
      }
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        this.fireDebug("   Reading Texture Entry #{1} of {2}... (Offset: 0x{0})", (object) tplFile.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
        tplFile.Read(buffer1, 0, 4);
        tplTextureEntry.TextureHeaderOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        tplFile.Read(buffer1, 0, 4);
        tplTextureEntry.PaletteHeaderOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        this.tplTextureEntries.Add(tplTextureEntry);
      }
      for (int index = 0; (long) index < (long) this.tplHeader.NumOfTextures; ++index)
      {
        this.fireDebug("   Reading Texture Header #{1} of {2}... (Offset: 0x{0})", (object) tplFile.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
        TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
        tplFile.Seek((long) this.tplTextureEntries[index].TextureHeaderOffset, SeekOrigin.Begin);
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
        if (this.tplTextureEntries[index].PaletteHeaderOffset != 0U)
        {
          this.fireDebug("   Reading Palette Header #{1} of {2}... (Offset: 0x{0})", (object) tplFile.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
          tplFile.Seek((long) this.tplTextureEntries[index].PaletteHeaderOffset, SeekOrigin.Begin);
          tplFile.Read(buffer1, 0, 4);
          tplPaletteHeader.NumberOfItems = Shared.Swap(BitConverter.ToUInt16(buffer1, 0));
          tplPaletteHeader.Unpacked = buffer1[2];
          tplPaletteHeader.Pad = buffer1[3];
          tplFile.Read(buffer1, 0, 4);
          tplPaletteHeader.PaletteFormat = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
          tplFile.Read(buffer1, 0, 4);
          tplPaletteHeader.PaletteDataOffset = Shared.Swap(BitConverter.ToUInt32(buffer1, 0));
        }
        tplFile.Seek((long) tplTextureHeader.TextureDataOffset, SeekOrigin.Begin);
        byte[] buffer2 = new byte[this.textureByteSize((TPL_TextureFormat) tplTextureHeader.TextureFormat, (int) tplTextureHeader.TextureWidth, (int) tplTextureHeader.TextureHeight)];
        byte[] buffer3 = new byte[(int) tplPaletteHeader.NumberOfItems * 2];
        this.fireDebug("   Reading Texture #{1} of {2}... (Offset: 0x{0})", (object) tplFile.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
        tplFile.Read(buffer2, 0, buffer2.Length);
        if (this.tplTextureEntries[index].PaletteHeaderOffset != 0U)
        {
          this.fireDebug("   Reading Palette #{1} of {2}... (Offset: 0x{0})", (object) tplFile.Position, (object) (index + 1), (object) this.tplHeader.NumOfTextures);
          tplFile.Seek((long) tplPaletteHeader.PaletteDataOffset, SeekOrigin.Begin);
          tplFile.Read(buffer3, 0, buffer3.Length);
        }
        else
          buffer3 = new byte[0];
        this.tplTextureHeaders.Add(tplTextureHeader);
        this.tplPaletteHeaders.Add(tplPaletteHeader);
        this.textureData.Add(buffer2);
        this.paletteData.Add(buffer3);
      }
    }

    private int textureByteSize(TPL_TextureFormat tplFormat, int width, int height)
    {
      switch (tplFormat)
      {
        case TPL_TextureFormat.I4:
          return Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2;
        case TPL_TextureFormat.I8:
        case TPL_TextureFormat.IA4:
          return Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4);
        case TPL_TextureFormat.IA8:
        case TPL_TextureFormat.RGB565:
        case TPL_TextureFormat.RGB5A3:
          return Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2;
        case TPL_TextureFormat.RGBA8:
          return Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 4;
        case TPL_TextureFormat.CI4:
          return Shared.AddPadding(width, 8) * Shared.AddPadding(height, 8) / 2;
        case TPL_TextureFormat.CI8:
          return Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4);
        case TPL_TextureFormat.CI14X2:
          return Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2;
        case TPL_TextureFormat.CMP:
          return width * height;
        default:
          throw new FormatException("Unsupported Texture Format!");
      }
    }

    private void createFromImages(
      Image[] images,
      TPL_TextureFormat[] tplFormats,
      TPL_PaletteFormat[] paletteFormats)
    {
      this.tplHeader = new TPL_Header();
      this.tplTextureEntries = new List<TPL_TextureEntry>();
      this.tplTextureHeaders = new List<TPL_TextureHeader>();
      this.tplPaletteHeaders = new List<TPL_PaletteHeader>();
      this.textureData = new List<byte[]>();
      this.paletteData = new List<byte[]>();
      this.tplHeader.NumOfTextures = (uint) images.Length;
      for (int index = 0; index < images.Length; ++index)
      {
        Image image = images[index];
        TPL_TextureEntry tplTextureEntry = new TPL_TextureEntry();
        TPL_TextureHeader tplTextureHeader = new TPL_TextureHeader();
        TPL_PaletteHeader tplPaletteHeader = new TPL_PaletteHeader();
        byte[] numArray1 = this.imageToTpl(image, tplFormats[index]);
        byte[] numArray2 = new byte[0];
        tplTextureHeader.TextureHeight = (ushort) image.Height;
        tplTextureHeader.TextureWidth = (ushort) image.Width;
        tplTextureHeader.TextureFormat = (uint) tplFormats[index];
        if (tplFormats[index] == TPL_TextureFormat.CI4 || tplFormats[index] == TPL_TextureFormat.CI8 || tplFormats[index] == TPL_TextureFormat.CI14X2)
        {
          ColorIndexConverter colorIndexConverter = new ColorIndexConverter(this.imageToRgba(image), image.Width, image.Height, tplFormats[index], paletteFormats[index]);
          numArray1 = colorIndexConverter.Data;
          numArray2 = colorIndexConverter.Palette;
          tplPaletteHeader.NumberOfItems = (ushort) (numArray2.Length / 2);
          tplPaletteHeader.PaletteFormat = (uint) paletteFormats[index];
        }
        this.tplTextureEntries.Add(tplTextureEntry);
        this.tplTextureHeaders.Add(tplTextureHeader);
        this.tplPaletteHeaders.Add(tplPaletteHeader);
        this.textureData.Add(numArray1);
        this.paletteData.Add(numArray2);
      }
    }

    private byte[] imageToTpl(Image img, TPL_TextureFormat tplFormat)
    {
      switch (tplFormat)
      {
        case TPL_TextureFormat.I4:
          return this.toI4((Bitmap) img);
        case TPL_TextureFormat.I8:
          return this.toI8((Bitmap) img);
        case TPL_TextureFormat.IA4:
          return this.toIA4((Bitmap) img);
        case TPL_TextureFormat.IA8:
          return this.toIA8((Bitmap) img);
        case TPL_TextureFormat.RGB565:
          return this.toRGB565((Bitmap) img);
        case TPL_TextureFormat.RGB5A3:
          return this.toRGB5A3((Bitmap) img);
        case TPL_TextureFormat.RGBA8:
          return this.toRGBA8((Bitmap) img);
        case TPL_TextureFormat.CI4:
        case TPL_TextureFormat.CI8:
        case TPL_TextureFormat.CI14X2:
          return new byte[0];
        default:
          throw new FormatException("Format not supported!\nCurrently, images can only be converted to the following formats:\nI4, I8, IA4, IA8, RGB565, RGB5A3, RGBA8, CI4, CI8 , CI14X2.");
      }
    }

    private uint[] imageToRgba(Image img)
    {
      Bitmap bitmap = (Bitmap) img;
      BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      byte[] numArray = new byte[bitmapdata.Height * Math.Abs(bitmapdata.Stride)];
      Marshal.Copy(bitmapdata.Scan0, numArray, 0, numArray.Length);
      bitmap.UnlockBits(bitmapdata);
      return Shared.ByteArrayToUIntArray(numArray);
    }

    private Bitmap rgbaToImage(byte[] data, int width, int height)
    {
      if (width == 0)
        width = 1;
      if (height == 0)
        height = 1;
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

    private uint[] paletteToRgba(int index)
    {
      TPL_PaletteFormat paletteFormat = (TPL_PaletteFormat) this.tplPaletteHeaders[index].PaletteFormat;
      int numberOfItems = (int) this.tplPaletteHeaders[index].NumberOfItems;
      uint[] numArray = new uint[numberOfItems];
      for (int index1 = 0; index1 < numberOfItems; ++index1)
      {
        if (index1 < numberOfItems)
        {
          ushort uint16 = BitConverter.ToUInt16(new byte[2]
          {
            this.paletteData[index][index1 * 2 + 1],
            this.paletteData[index][index1 * 2]
          }, 0);
          int num1;
          int num2;
          int num3;
          int num4;
          switch (paletteFormat)
          {
            case TPL_PaletteFormat.IA8:
              num1 = (int) uint16 & (int) byte.MaxValue;
              num2 = num1;
              num3 = num1;
              num4 = (int) uint16 >> 8;
              break;
            case TPL_PaletteFormat.RGB565:
              num2 = ((int) uint16 >> 11 & 31) << 3 & (int) byte.MaxValue;
              num3 = ((int) uint16 >> 5 & 63) << 2 & (int) byte.MaxValue;
              num1 = ((int) uint16 & 31) << 3 & (int) byte.MaxValue;
              num4 = (int) byte.MaxValue;
              break;
            default:
              if (((int) uint16 & 32768) != 0)
              {
                num4 = (int) byte.MaxValue;
                num2 = ((int) uint16 >> 10 & 31) * (int) byte.MaxValue / 31;
                num3 = ((int) uint16 >> 5 & 31) * (int) byte.MaxValue / 31;
                num1 = ((int) uint16 & 31) * (int) byte.MaxValue / 31;
                break;
              }
              num4 = ((int) uint16 >> 12 & 7) * (int) byte.MaxValue / 7;
              num2 = ((int) uint16 >> 8 & 15) * (int) byte.MaxValue / 15;
              num3 = ((int) uint16 >> 4 & 15) * (int) byte.MaxValue / 15;
              num1 = ((int) uint16 & 15) * (int) byte.MaxValue / 15;
              break;
          }
          numArray[index1] = (uint) (num1 | num3 << 8 | num2 << 16 | num4 << 24);
        }
      }
      return numArray;
    }

    private int avg(int w0, int w1, int c0, int c1)
    {
      int num1 = c0 >> 11;
      int num2 = c1 >> 11;
      int num3 = (w0 * num1 + w1 * num2) / (w0 + w1) << 11 & (int) ushort.MaxValue;
      int num4 = c0 >> 5 & 63;
      int num5 = c1 >> 5 & 63;
      int num6 = (w0 * num4 + w1 * num5) / (w0 + w1) << 5 & (int) ushort.MaxValue;
      int num7 = num3 | num6;
      int num8 = c0 & 31;
      int num9 = c1 & 31;
      int num10 = (w0 * num8 + w1 * num9) / (w0 + w1);
      return num7 | num10;
    }

    private byte[] fromRGBA8(byte[] tpl, int width, int height)
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
                    int num3 = (int) num2 >> 8 & (int) byte.MaxValue;
                    int num4 = (int) num2 & (int) byte.MaxValue;
                    array[index5 + index4 * width] |= (uint) (num4 << 16 | num3 << 24);
                  }
                  else
                  {
                    int num3 = (int) num2 >> 8 & (int) byte.MaxValue;
                    int num4 = (int) num2 & (int) byte.MaxValue;
                    array[index5 + index4 * width] |= (uint) (num3 << 8 | num4);
                  }
                }
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toRGBA8(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
              numArray2[index1] = num2 >> 16 & (uint) byte.MaxValue;
              numArray3[index1] = num2 >> 8 & (uint) byte.MaxValue;
              numArray4[index1] = num2 & (uint) byte.MaxValue;
              numArray5[index1] = num2 >> 24 & (uint) byte.MaxValue;
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
              int num3 = (int) (byte) numArray5[index4];
              numArray6[index5] = (byte) num3;
              byte[] numArray7 = numArray1;
              int index6 = num2;
              num1 = index6 + 1;
              int num4 = (int) (byte) numArray2[index4];
              numArray7[index6] = (byte) num4;
            }
            for (int index4 = 0; index4 < 16; ++index4)
            {
              byte[] numArray6 = numArray1;
              int index5 = num1;
              int num2 = index5 + 1;
              int num3 = (int) (byte) numArray3[index4];
              numArray6[index5] = (byte) num3;
              byte[] numArray7 = numArray1;
              int index6 = num2;
              num1 = index6 + 1;
              int num4 = (int) (byte) numArray4[index4];
              numArray7[index6] = (byte) num4;
            }
            index1 = 0;
          }
        }
      }
      return numArray1;
    }

    private byte[] fromRGB5A3(byte[] tpl, int width, int height)
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
                if (((int) num2 & 32768) != 0)
                {
                  num3 = ((int) num2 >> 10 & 31) * (int) byte.MaxValue / 31;
                  num4 = ((int) num2 >> 5 & 31) * (int) byte.MaxValue / 31;
                  num5 = ((int) num2 & 31) * (int) byte.MaxValue / 31;
                  num6 = (int) byte.MaxValue;
                }
                else
                {
                  num6 = ((int) num2 >> 12 & 7) * (int) byte.MaxValue / 7;
                  num3 = ((int) num2 >> 8 & 15) * (int) byte.MaxValue / 15;
                  num4 = ((int) num2 >> 4 & 15) * (int) byte.MaxValue / 15;
                  num5 = ((int) num2 & 15) * (int) byte.MaxValue / 15;
                }
                array[index3 * width + index4] = (uint) (num5 | num4 << 8 | num3 << 16 | num6 << 24);
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toRGB5A3(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                int num3 = (int) rgba[index4 + index3 * width];
                int num4 = 0;
                int num5 = num3 >> 16 & (int) byte.MaxValue;
                int num6 = num3 >> 8 & (int) byte.MaxValue;
                int num7 = num3 & (int) byte.MaxValue;
                int num8 = num3 >> 24 & (int) byte.MaxValue;
                if (num8 <= 218)
                {
                  int num9 = num4 & -32769;
                  int num10 = num5 * 15 / (int) byte.MaxValue & 15;
                  int num11 = num6 * 15 / (int) byte.MaxValue & 15;
                  int num12 = num7 * 15 / (int) byte.MaxValue & 15;
                  int num13 = num8 * 7 / (int) byte.MaxValue & 7;
                  num2 = num9 | (num13 << 12 | num10 << 8 | num11 << 4 | num12);
                }
                else
                  num2 = num4 | 32768 | ((num5 * 31 / (int) byte.MaxValue & 31) << 10 | (num6 * 31 / (int) byte.MaxValue & 31) << 5 | num7 * 31 / (int) byte.MaxValue & 31);
              }
              int num14;
              numArray[num14 = num1 + 1] = (byte) (num2 >> 8);
              numArray[num1 = num14 + 1] = (byte) (num2 & (int) byte.MaxValue);
            }
          }
        }
      }
      return numArray;
    }

    private byte[] fromRGB565(byte[] tpl, int width, int height)
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
                int num3 = ((int) num2 >> 11 & 31) << 3 & (int) byte.MaxValue;
                int num4 = ((int) num2 >> 5 & 63) << 2 & (int) byte.MaxValue;
                int num5 = ((int) num2 & 31) << 3 & (int) byte.MaxValue;
                array[index3 * width + index4] = (uint) (num5 | num4 << 8 | num3 << 16 | -16777216);
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toRGB565(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                num2 = (ushort) 0;
              }
              else
              {
                int num3 = (int) rgba[index4 + index3 * width];
                num2 = (ushort) ((uint) ((int) (((uint) num3 >> 16 & (uint) byte.MaxValue) >> 3) << 11 | (int) (((uint) num3 >> 8 & (uint) byte.MaxValue) >> 2) << 5) | (uint) (num3 & (int) byte.MaxValue) >> 3);
              }
              int num4;
              numArray[num4 = num1 + 1] = (byte) ((uint) num2 >> 8);
              numArray[num1 = num4 + 1] = (byte) ((uint) num2 & (uint) byte.MaxValue);
            }
          }
        }
      }
      return numArray;
    }

    private byte[] fromI4(byte[] tpl, int width, int height)
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
              int num2 = (int) tpl[num1++];
              if (index3 < height && index4 < width)
              {
                int num3 = (num2 >> 4) * (int) byte.MaxValue / 15;
                array[index3 * width + index4] = (uint) (num3 | num3 << 8 | num3 << 16 | -16777216);
                int num4 = (num2 & 15) * (int) byte.MaxValue / 15;
                if (index3 * width + index4 + 1 < array.Length)
                  array[index3 * width + index4 + 1] = (uint) (num4 | num4 << 8 | num4 << 16 | -16777216);
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toI4(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                num2 = (byte) 0;
              }
              else
              {
                int num3 = (int) rgba[index4 + index3 * width];
                int num4 = (int) (((uint) (num3 & (int) byte.MaxValue) + ((uint) num3 >> 8 & (uint) byte.MaxValue) + ((uint) num3 >> 16 & (uint) byte.MaxValue)) / 3U) & (int) byte.MaxValue;
                int num5 = index4 + index3 * width + 1 < rgba.Length ? (int) rgba[index4 + index3 * width + 1] : 0;
                uint num6 = ((uint) (num5 & (int) byte.MaxValue) + ((uint) num5 >> 8 & (uint) byte.MaxValue) + ((uint) num5 >> 16 & (uint) byte.MaxValue)) / 3U & (uint) byte.MaxValue;
                num2 = (byte) ((int) ((uint) (num4 * 15) / (uint) byte.MaxValue) << 4 | (int) (num6 * 15U / (uint) byte.MaxValue) & 15);
              }
              numArray[num1++] = num2;
            }
          }
        }
      }
      return numArray;
    }

    private byte[] fromI8(byte[] tpl, int width, int height)
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
              int num2 = (int) tpl[num1++];
              if (index3 < height && index4 < width)
                array[index3 * width + index4] = (uint) (num2 | num2 << 8 | num2 << 16 | -16777216);
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toI8(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                num2 = (byte) 0;
              }
              else
              {
                int num3 = (int) rgba[index4 + index3 * width];
                num2 = (byte) (((uint) (num3 & (int) byte.MaxValue) + ((uint) num3 >> 8 & (uint) byte.MaxValue) + ((uint) num3 >> 16 & (uint) byte.MaxValue)) / 3U & (uint) byte.MaxValue);
              }
              numArray[num1++] = num2;
            }
          }
        }
      }
      return numArray;
    }

    private byte[] fromIA4(byte[] tpl, int width, int height)
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
              int num2 = (int) tpl[num1++];
              if (index3 < height && index4 < width)
              {
                int num3 = (num2 & 15) * (int) byte.MaxValue / 15 & (int) byte.MaxValue;
                int num4 = (num2 >> 4) * (int) byte.MaxValue / 15 & (int) byte.MaxValue;
                array[index3 * width + index4] = (uint) (num3 | num3 << 8 | num3 << 16 | num4 << 24);
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toIA4(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                num2 = (byte) 0;
              }
              else
              {
                uint num3 = rgba[index4 + index3 * width];
                num2 = (byte) ((int) ((uint) (((int) (((num3 & (uint) byte.MaxValue) + (num3 >> 8 & (uint) byte.MaxValue) + (num3 >> 16 & (uint) byte.MaxValue)) / 3U) & (int) byte.MaxValue) * 15) / (uint) byte.MaxValue) & 15 | (int) ((num3 >> 24 & (uint) byte.MaxValue) * 15U / (uint) byte.MaxValue) << 4);
              }
              numArray[num1++] = num2;
            }
          }
        }
      }
      return numArray;
    }

    private byte[] fromIA8(byte[] tpl, int width, int height)
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
              int num2 = (int) Shared.Swap(BitConverter.ToUInt16(tpl, num1++ * 2));
              if (index3 < height && index4 < width)
              {
                uint num3 = (uint) (num2 >> 8);
                uint num4 = (uint) (num2 & (int) byte.MaxValue);
                array[index3 * width + index4] = (uint) ((int) num4 | (int) num4 << 8 | (int) num4 << 16 | (int) num3 << 24);
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] toIA8(Bitmap img)
    {
      uint[] rgba = this.imageToRgba((Image) img);
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
                num2 = (ushort) 0;
              }
              else
              {
                int num3 = (int) rgba[index4 + index3 * width];
                num2 = (ushort) ((uint) (((int) ((uint) num3 >> 24) & (int) byte.MaxValue) << 8) | ((uint) (num3 & (int) byte.MaxValue) + ((uint) num3 >> 8 & (uint) byte.MaxValue) + ((uint) num3 >> 16 & (uint) byte.MaxValue)) / 3U & (uint) byte.MaxValue);
              }
              Array.Reverse((Array) BitConverter.GetBytes(num2));
              byte[] numArray2 = numArray1;
              int index5 = num1;
              int num4 = index5 + 1;
              int num5 = (int) (byte) ((uint) num2 >> 8);
              numArray2[index5] = (byte) num5;
              byte[] numArray3 = numArray1;
              int index6 = num4;
              num1 = index6 + 1;
              int num6 = (int) (byte) ((uint) num2 & (uint) byte.MaxValue);
              numArray3[index6] = (byte) num6;
            }
          }
        }
      }
      return numArray1;
    }

    private byte[] fromCI4(byte[] tpl, uint[] paletteData, int width, int height)
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
                array[index3 * width + index4] = paletteData[(int) num2 >> 4];
                if (index3 * width + index4 + 1 < array.Length)
                  array[index3 * width + index4 + 1] = paletteData[(int) num2 & 15];
              }
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] fromCI8(byte[] tpl, uint[] paletteData, int width, int height)
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
              ushort num2 = (ushort) tpl[num1++];
              if (index3 < height && index4 < width)
                array[index3 * width + index4] = paletteData[(int) num2];
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] fromCI14X2(byte[] tpl, uint[] paletteData, int width, int height)
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
                array[index3 * width + index4] = paletteData[(int) num2 & 16383];
            }
          }
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private byte[] fromCMP(byte[] tpl, int width, int height)
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
          if ((int) numArray1[0] > (int) numArray1[1])
          {
            numArray1[2] = (ushort) this.avg(2, 1, (int) numArray1[0], (int) numArray1[1]);
            numArray1[3] = (ushort) this.avg(1, 2, (int) numArray1[0], (int) numArray1[1]);
          }
          else
          {
            numArray1[2] = (ushort) this.avg(1, 1, (int) numArray1[0], (int) numArray1[1]);
            numArray1[3] = (ushort) 0;
          }
          uint num8 = Shared.Swap(BitConverter.ToUInt32(tpl, startIndex + 4));
          int num9 = num2 + 4 * num5;
          int num10 = (int) numArray1[(int) (num8 >> 30 - 2 * num9) & 3];
          numArray2[0] = num10 >> 8 & 248;
          numArray2[1] = num10 >> 3 & 248;
          numArray2[2] = num10 << 3 & 248;
          numArray2[3] = (int) byte.MaxValue;
          if (((int) (num8 >> 30 - 2 * num9) & 3) == 3 && (int) numArray1[0] <= (int) numArray1[1])
            numArray2[3] = 0;
          array[index1] = (uint) (numArray2[0] << 16 | numArray2[1] << 8 | numArray2[2] | numArray2[3] << 24);
          ++index1;
        }
      }
      return Shared.UIntArrayToByteArray(array);
    }

    private void fireDebug(string debugMessage, params object[] args)
    {
      EventHandler<MessageEventArgs> debug = this.Debug;
      if (debug == null)
        return;
      debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
    }
  }
}
