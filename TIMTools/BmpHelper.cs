using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Runtime.InteropServices;

namespace TIMTools
{
    public class BmpHelper
    {
        public void SaveBitmapAs4BitBmp(Bitmap image, Stream stream)
        {
            // Convertir a 4bpp Indexed
            // Convertir a 4bpp Indexed
            using (Image<Rgba32> img = SixLabors.ImageSharp.Image.Load<Rgba32>(ToByteArray(image)))
            {
                img.Mutate(x => x
                    .Resize(image.Width, image.Height)
                    .Quantize()
                );

                // Guardar el archivo
                img.Save(stream, new BmpEncoder { BitsPerPixel = BmpBitsPerPixel.Pixel4 });
            }
        }

        public void SaveBitmapAs8BitBmp(Bitmap image, Stream stream)
        {
            // Convertir a 4bpp Indexed
            // Convertir a 4bpp Indexed
            using (Image<Rgba32> img = SixLabors.ImageSharp.Image.Load<Rgba32>(ToByteArray(image)))
            {
                img.Mutate(x => x
                    .Resize(image.Width, image.Height)
                    .Quantize()
                );

                // Guardar el archivo
                img.Save(stream, new BmpEncoder { BitsPerPixel = BmpBitsPerPixel.Pixel8 });
            }
        }

        private byte[] ToByteArray(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png); // Guardar en formato PNG temporalmente
                return ms.ToArray();
            }
        }

        




            private Rgba32 ConvertColorToRgba32(System.Drawing.Color color)
        {
            return new Rgba32(color.R, color.G, color.B, color.A);
        }
    }

}

