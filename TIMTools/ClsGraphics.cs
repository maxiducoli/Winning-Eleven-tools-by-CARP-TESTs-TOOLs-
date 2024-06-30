using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TIMTools
{
    public class ClsGraphics
    {
        public enum BPPType : int
        {
            BMP4bpp = 0,
            BMP8bpp = 1
        }

        public static  void AplicarSombra(string bmpFilePath, string layerFilePath, string outputFilePath)
        {
            using (Bitmap baseImage = new Bitmap(bmpFilePath))
            using (Bitmap layerImage = new Bitmap(layerFilePath))
            {
                // Ensure both images have the same size
                if (baseImage.Width != layerImage.Width || baseImage.Height != layerImage.Height)
                {
                    throw new ArgumentException("Both images must have the same dimensions.");
                }

                // Convert baseImage to direct pixel format
                Bitmap baseImageDirect = baseImage.Clone(new Rectangle(0, 0, baseImage.Width, baseImage.Height), PixelFormat.Format32bppArgb);

                // Set the color that will be considered as transparent (e.g., black in this case)
                Color transparentColor = Color.Black;

                // Set the ImageAttributes object with the color matrix to apply transparency
                float[][] colorMatrixElements = {
                new float[] {1, 0, 0, 0, 0},           // Red
                new float[] {0, 1, 0, 0, 0},           // Green
                new float[] {0, 0, 1, 0, 0},           // Blue
                new float[] {0, 0, 0, 0.75f, 0},        // Alpha (transparency)
                new float[] {0, 0, 0, 0, 1}            // Scaling factor
            };
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Draw the layer with transparency on top of the base image
                using (Graphics g = Graphics.FromImage(baseImageDirect))
                {
                    g.DrawImage(layerImage, new Rectangle(0, 0, baseImageDirect.Width, baseImageDirect.Height), 0, 0, baseImageDirect.Width, baseImageDirect.Height, GraphicsUnit.Pixel, imageAttributes);
                }


                // Save the result to the output file
                baseImageDirect.Save(outputFilePath, ImageFormat.Bmp);
            }
        }

        public static void CrearSombras8BPP(string camisetaPath, string sombreaPath, string camisetaSomreadaPath)
        {
            using (Bitmap baseImage = new Bitmap(camisetaPath))
            using (Bitmap layerImage = new Bitmap(sombreaPath))
            {
                // Ensure both images have the same size
                if (baseImage.Width != layerImage.Width || baseImage.Height != layerImage.Height)
                {
                    throw new ArgumentException("Both images must have the same dimensions.");
                }

                // Convert baseImage to direct pixel format
                Bitmap baseImageDirect = baseImage.Clone(new Rectangle(0, 0, baseImage.Width, baseImage.Height), PixelFormat.Format32bppArgb);

                // Set the color that will be considered as transparent (e.g., black in this case)
                Color transparentColor = Color.Black;

                // Set the ImageAttributes object with the color matrix to apply transparency
                float[][] colorMatrixElements = {
            new float[] {1, 0, 0, 0, 0},           // Red
            new float[] {0, 1, 0, 0, 0},           // Green
            new float[] {0, 0, 1, 0, 0},           // Blue
            new float[] {0, 0, 0, 1.5f, 0},        // Alpha (transparency)
            new float[] {0, 0, 0, 0, 1}            // Scaling factor
        };
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Draw the layer with transparency on top of the base image
                using (Graphics g = Graphics.FromImage(baseImageDirect))
                {
                    g.DrawImage(layerImage, new Rectangle(0, 0, baseImageDirect.Width, baseImageDirect.Height), 0, 0, baseImageDirect.Width, baseImageDirect.Height, GraphicsUnit.Pixel, imageAttributes);
                }

                // Convert the resulting image to an indexed format with a color depth of 8 bits per pixel (8bpp)
                Bitmap result = baseImageDirect.Clone(new Rectangle(0, 0, baseImageDirect.Width, baseImageDirect.Height), PixelFormat.Format8bppIndexed);

                // Save the result to the output file
                result.Save(camisetaSomreadaPath, ImageFormat.Bmp);
            }
        }

        public static void CrearCamisetaConSombras(string camisePath, string sombraPath, string camisetaSombreadaPath)
        {

            using (Bitmap baseImage = new Bitmap(camisePath))
            using (Bitmap layerImage = new Bitmap(sombraPath))
            {
                // Ensure both images have the same size
                if (baseImage.Width != layerImage.Width || baseImage.Height != layerImage.Height)
                {
                    throw new ArgumentException("Both images must have the same dimensions.");
                }

                // Convert baseImage to direct pixel format
                Bitmap baseImageDirect = baseImage.Clone(new Rectangle(0, 0, baseImage.Width, baseImage.Height), PixelFormat.Format32bppArgb);

                // Set the color that will be considered as transparent (e.g., black in this case)
                Color transparentColor = Color.Black;

                // Set the ImageAttributes object with the color matrix to apply transparency
                float[][] colorMatrixElements = {
            new float[] {1, 0, 0, 0, 0},           // Red
            new float[] {0, 1, 0, 0, 0},           // Green
            new float[] {0, 0, 1, 0, 0},           // Blue
            new float[] {0, 0, 0, 0.25f, 0},        // Alpha (transparency)
            new float[] {0, 0, 0, 0, 1}            // Scaling factor
        };
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Draw the layer with transparency on top of the base image
                using (Graphics g = Graphics.FromImage(baseImageDirect))
                {
                    g.DrawImage(layerImage, new Rectangle(0, 0, baseImageDirect.Width, baseImageDirect.Height), 0, 0, baseImageDirect.Width, baseImageDirect.Height, GraphicsUnit.Pixel, imageAttributes);
                }

                // Apply the Floyd-Steinberg dithering algorithm to the resulting image
                Bitmap ditheredImage = FloydSteinbergDither(baseImageDirect);

                // Convert the dithered image to an indexed format with a color depth of 8 bits per pixel (8bpp)
                Bitmap result = ditheredImage.Clone(new Rectangle(0, 0, ditheredImage.Width, ditheredImage.Height), PixelFormat.Format8bppIndexed);

                // Save the result to the output file
                result.Save(camisetaSombreadaPath);
            }
        }

        private static Bitmap FloydSteinbergDither(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            BitmapData data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite,
            PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;

                int remain = data.Stride - data.Width * 3;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r = ptr[2];
                        byte g = ptr[1];
                        byte b = ptr[0];

                        byte aR = (byte)(r & 248);
                        byte aG = (byte)(g & 248);
                        byte aB = (byte)(b & 248);

                        ptr[2] = aR;
                        ptr[1] = aG;
                        ptr[0] = aB;

                        int eR = r - aR;
                        int eG = g - aG;
                        int eB = b - aB;

                        if (x < width - 1)
                        {
                            ptr[5] += (byte)(7 * eR / 16);
                            ptr[4] += (byte)(7 * eG / 16);
                            ptr[3] += (byte)(7 * eB / 16);
                        }

                        if (y < height - 1)
                        {
                            if (x > 1)
                            {
                                ptr[data.Stride + 2] += (byte)(3 * eR / 16);
                                ptr[data.Stride + 1] += (byte)(3 * eG / 16);
                                ptr[data.Stride + 2] += (byte)(3 * eB / 16);
                            }

                            ptr[data.Stride + 2] += (byte)(5 * eR / 16);
                            ptr[data.Stride + 1] += (byte)(5 * eG / 16);
                            ptr[data.Stride + 2] += (byte)(5 * eB / 16);

                            if (x < width - 1)
                            {
                                ptr[data.Stride + 5] += (byte)(eR / 16);
                                ptr[data.Stride + 4] += (byte)(eG / 16);
                                ptr[data.Stride + 3] += (byte)(eB / 16);
                            }
                        }

                        ptr += 3;
                    }

                    ptr += remain;
                }
            }

            image.UnlockBits(data);

            return image;

        }

        private static byte[] GetRGBFromBMP(string bmpPath)
        {
            byte[] result = null;
            const int bppPosition = 28;
            const int palettePosition = 54;
            byte[] tempdata = new byte[1];
            FileStream file = new FileStream(bmpPath, FileMode.Open, FileAccess.Read);
            file.Position = bppPosition;
            file.Read(tempdata, 0, 1);
            if (tempdata[0] == 4)
            {
                result = new byte[64];
            }
            if (tempdata[0] == 8)
            {
                result = new byte[1024];
            }
            file.Position = palettePosition;
            file.Read(result, 0, result.Length);
            file.Close();
            return result;
        }
        
        private static Color BytesToColor(byte[] data) 
        {
            Color color = new Color();

            color = Color.FromArgb(data[3], data[2], data[1], data[0]);

            return color;
        }

        public  static Color[] LoadColors(string bmpPath)
        {
            byte[] data = GetRGBFromBMP(bmpPath);
            byte[] dataTemp = new byte[4];
            Color[] color = new Color[data.Length / 4];
            int colorIndex = 0;
            for (int i = 0; i < data.Length; i += 4)
            {
                dataTemp[0] = data[i];
                dataTemp[1] = data[i + 1];
                dataTemp[2] = data[i + 2];
                dataTemp[3] = 0xFF;
                color[colorIndex] = Color.FromArgb(dataTemp[3], dataTemp[2], dataTemp[1], dataTemp[0]);
                colorIndex++;
            }
            return color;
        }

        public static bool GetBPP(string bmpPath)
        {
            bool result = false;
            const int bppPisition = 28;
            byte[] tempdata = new byte[1];

            FileStream file = new FileStream(bmpPath, FileMode.Open, FileAccess.Read);
            file.Position = bppPisition;
            file.Read(tempdata, 0, 1);
            if (tempdata[0] == 4)
            {
                result = true;
            }
            if (tempdata[0] == 8)
            {
                result = true;
            }
            file.Close();
            return result;
        }

        public static void SetColors(string bmpPath,Color[] colors)
        {
            byte[] c = SetColorToBytes(colors);
            FileStream file = new FileStream(bmpPath,FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            file.Position = 54;
            file.Write(c, 0, colors.Length);
            file.Close();
        }

        private static byte[] SetColorToBytes(Color[] colors)
        {
            byte[] destinationArray = new byte[0];

            foreach (Color color in colors)
            {
            byte[] bytes = new byte[4];

                    bytes[0] = (byte)color.R;
                    bytes[1] = (byte)color.G;
                    bytes[2] = (byte)color.B;
                    bytes[3] = (byte)color.A;

                // Redimensionar el array de destino para tener espacio para los datos del array fuente
                Array.Resize(ref destinationArray, destinationArray.Length + bytes.Length);

                // Copiar los datos del array fuente al array destino
                Array.Copy(bytes, 0, destinationArray, destinationArray.Length - bytes.Length, bytes.Length);

            }
            return destinationArray;
        }

        public bool IsHex(IEnumerable<char> chars)
        {

            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

    }
}
