using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TIMTools
{
    public class BMPtoTIMConverter
    {
        private enum TimImageType
        {
            Tim4bppNoPalette = 0x08,
            Tim8bppNoPalette = 0x09,
            Tim16bppNotIndexed = 0x02,
            Tim24bppNotIndexed = 0x03,
            Tim4bppWithPalette = 0x08,
            Tim8bppWithPalette = 0x09
        }
        private const int TimMagic = 0x00000010;
        private const int Tim4bppPaletteSize = 16;
        private const int Tim8bppPaletteSize = 256;
        string bmpPath = string.Empty;

        public string BmpPath { get => bmpPath; set => bmpPath = value; }

        public  void ConvertToTim(string bmpFilePath, string timFilePath)
        {
            Bitmap bmp = new Bitmap(bmpFilePath);
            BmpPath = bmpFilePath;
            if (bmp.PixelFormat != PixelFormat.Format4bppIndexed && bmp.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new ArgumentException("The input BMP must be in 4bpp or 8bpp indexed format.");
            }

            int timImageType = GetTimImageType(bmp.PixelFormat);
            int clutSize = GetClutSize(bmp.PixelFormat);
            int clutColors = GetClutColors(bmp.PixelFormat);

            byte[] imageData = GetImageData(bmp);
            byte[] clutData = GetColorPaletteFromBitmap(bmp);

            int imageWidth = bmp.Width / (timImageType == (int)TimImageType.Tim4bppNoPalette || timImageType == (int)TimImageType.Tim4bppWithPalette ? 4 : 2);
            int imageHeight = bmp.Height;

            using (FileStream fs = new FileStream(timFilePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Write TIM header
                writer.Write(TimMagic);
                writer.Write(timImageType);
                writer.Write((clutSize * 2) + 12);
                writer.Write((short)0x01); // Palette Org X (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)0x100); // Palette Org Y (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)clutColors);
                writer.Write((short)1); // Number of CLUTs (always 1 for TIM 4BPP or TIM 8BPP)

                // Write CLUT data (if applicable)
                writer.Write(clutData);

                // Write image header
                writer.Write(imageData.Length + 12);
                writer.Write((short)0); // Image Org X (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)0); // Image Org Y (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)(imageWidth));
                writer.Write((short)imageHeight);

                // Write image data
                writer.Write(imageData);
            }
        }
        public  void ConvertToTim(Bitmap bmp, string timFilePath)
        {

            if (bmp.PixelFormat != PixelFormat.Format4bppIndexed && bmp.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new ArgumentException("The input BMP must be in 4bpp or 8bpp indexed format.");
            }

            int timImageType = GetTimImageType(bmp.PixelFormat);
            int clutSize = GetClutSize(bmp.PixelFormat);
            int clutColors = GetClutColors(bmp.PixelFormat);

            byte[] imageData = GetImageData(bmp);
            byte[] clutData = GetColorPaletteFromBitmap(bmp);

            int imageWidth = bmp.Width / (timImageType == (int)TimImageType.Tim4bppNoPalette || timImageType == (int)TimImageType.Tim4bppWithPalette ? 4 : 2);
            int imageHeight = bmp.Height;

            using (FileStream fs = new FileStream(timFilePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Write TIM header
                writer.Write(TimMagic);
                writer.Write(timImageType);
                writer.Write((clutSize * 2) + 12);
                writer.Write((short)0x01); // Palette Org X (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)0x100); // Palette Org Y (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)clutColors);
                writer.Write((short)1); // Number of CLUTs (always 1 for TIM 4BPP or TIM 8BPP)

                // Write CLUT data (if applicable)
                writer.Write(clutData);

                // Write image header
                writer.Write(imageData.Length + 12);
                writer.Write((short)0); // Image Org X (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)0); // Image Org Y (not used for TIM 4BPP or TIM 8BPP)
                writer.Write((short)(imageWidth));
                writer.Write((short)imageHeight);

                // Write image data
                writer.Write(imageData);
            }
        }
        private  int GetTimImageType(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format4bppIndexed:
                    return (int)TimImageType.Tim4bppNoPalette;
                case PixelFormat.Format8bppIndexed:
                    return (int)TimImageType.Tim8bppNoPalette;
                default:
                    throw new ArgumentException("The input BMP must be in 4bpp or 8bpp indexed format.");
            }
        }
        private  int GetClutSize(PixelFormat pixelFormat)
        {
            return (pixelFormat == PixelFormat.Format4bppIndexed) ? Tim4bppPaletteSize : Tim8bppPaletteSize;
        }

        private  byte[] GetColorPaletteFromBitmap(Bitmap bmp)
        {
         //   ClsGraphics clsGraphics = new ClsGraphics();
            int clutColors = GetClutColors(bmp.PixelFormat);
           
            Color[] palette = new Color[clutColors];
            palette = ClsGraphics.LoadColors(BmpPath);

            byte[] clutData;
             clutData = new byte[clutColors * 2];
            for (int i = 0; i < clutColors; i++)
            {
                Color color = palette[i];
                int timColor = (color.R >> 3) | ((color.G >> 3) << 5) | ((color.B >> 3) << 10);
                clutData[i * 2] = (byte)timColor;
                clutData[i * 2 + 1] = (byte)(timColor >> 8);
            }
            return clutData;
        }

        private  int GetClutColors(PixelFormat pixelFormat)
        {
            return (pixelFormat == PixelFormat.Format4bppIndexed) ? 16 : 256;
        }
        private  byte[] GetImageData(Bitmap bmp)
        {
            int bytesPerPixel = bmp.PixelFormat == PixelFormat.Format4bppIndexed ? 1 : 2;
            int imageWidth = bmp.Width * bytesPerPixel;
            int imageHeight = bmp.Height;

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            int imageSize = bmpData.Stride * bmpData.Height;
            byte[] imageData = new byte[imageSize];
            Marshal.Copy(bmpData.Scan0, imageData, 0, imageSize);

            bmp.UnlockBits(bmpData);

            return imageData;
        }

        private  byte[] GetClutData(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format4bppIndexed && bmp.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new ArgumentException("The input BMP must be in 4bpp or 8bpp indexed format.");
            }

            int clutColors = GetClutColors(bmp.PixelFormat);
            byte[] clutData = new byte[clutColors * 2];

            for (int i = 0; i < clutColors; i++)
            {
                Color color = bmp.Palette.Entries[i];
                int timColor = (color.R >> 3) | ((color.G >> 3) << 5) | ((color.B >> 3) << 10);
                clutData[i * 2] = (byte)timColor;
                clutData[i * 2 + 1] = (byte)(timColor >> 8);
            }

            return clutData;
        }
        public  void SaveAsTIM(string filePath, byte[] timData)
        {
            File.WriteAllBytes(filePath, timData);
        }

    }
}

