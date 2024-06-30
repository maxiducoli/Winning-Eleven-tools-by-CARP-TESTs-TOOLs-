using System.Drawing.Imaging;

namespace GraphicsUtils
{

    public class GraphicsTool
    {
        const byte BI_RGB = 0;
        const byte BI_RLE4 = 2;
        const byte BI_RLE8 = 1;

        public void ReadAndDecodeBMP(string filePath, out byte[] arrayDest)
        {
            arrayDest = null; 
            byte[] BufDest = null;
            try
            {
                using (Bitmap bmp = new Bitmap(filePath))
                {
                    // Obtener información necesaria para la decodificación
                    byte[] bmpBytes = ImageToByteArray(bmp);
                    uint xsize = (uint)bmp.Width;
                    uint ysize = (uint)bmp.Height;
                    byte depth = (byte)Image.GetPixelFormatSize(bmp.PixelFormat);
                    byte ComprFlag = GetCompressionFlag(bmp);

                    // Llamar a DecodeImage con los parámetros obtenidos
                    BufDest = new byte[xsize * ysize]; // Crear el arreglo de destino
                    if (DecodeImage(bmpBytes, out BufDest, xsize, ysize, depth, ComprFlag))
                    {
                        Console.WriteLine("Decodificación exitosa.");
                        // Puedes trabajar con BufDest aquí
                       
                    }
                    else
                    {
                        Console.WriteLine("Error al decodificar la imagen.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo BMP: {ex.Message}");
            }
            arrayDest = new byte[BufDest.Length];
            Array.Copy(BufDest, arrayDest, BufDest.Length);
        }

        private byte[] ImageToByteArray(Bitmap bmp)
        {
            // Convierte la imagen BMP a un arreglo de bytes
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(bmp, typeof(byte[]));
        }

        private byte GetCompressionFlag(Bitmap bmp)
        {
            // Determina el tipo de compresión del archivo BMP
            if (bmp.PixelFormat == PixelFormat.Format4bppIndexed)
            {
                return BI_RLE4;
            }
            else if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                return BI_RLE8;
            }
            else
            {
                return BI_RGB; // Por defecto, sin compresión
            }
        }

        private  bool DecodeImage(byte[] BufSrc, out byte[] BufDest, uint xsize, uint ysize, byte depth, byte ComprFlag)
        {
            uint xtemp;
            uint j = 0;
            byte[] pDest;
            try
            {

           
            if (depth == 4)
            {
                // 4 bit
                xtemp = xsize / 2;
                if (ComprFlag == BI_RGB)
                {
                    BufDest = new byte[xsize * ysize];
                    pDest = BufDest;

                    for (long y1 = ysize - 1; y1 >= 0; y1--)
                    {
                        for (uint x1 = 0; x1 < xtemp; x1++)
                        {
                            pDest[j] = (byte)(BufSrc[(y1 * xtemp) + x1] >> 4);
                            pDest[j] |= (byte)((BufSrc[(y1 * xtemp) + x1] << 4) & 0xF0);
                            j++;
                        }
                    }
                }
                else
                {
                    // BI_RLE4
                    bool bEOB = false;
                    byte[] pTempData = new byte[xtemp * ysize];
                    pDest = pTempData;

                    int code1, code2, i, k;
                    bool hi = false;
                    int abs_cou = 0, adj_cou = 0;

                    byte[] sta_ptr = pDest;

                    for (i = 0; i < BufSrc.Length && !bEOB; i += 2)
                    {
                        code1 = BufSrc[i];
                        code2 = BufSrc[i + 1];

                        if (abs_cou > 0)
                        {
                            if (hi)
                                pDest[j] |= (byte)(code1 >> 4);
                            else
                                pDest[j] = (byte)(code1 & 0xF0);
                            abs_cou--;
                            hi = !hi;

                            if (abs_cou > 0)
                            {
                                if (hi)
                                    pDest[j] |= (byte)(code1 & 0x0F);
                                else
                                    pDest[j] = (byte)(code1 << 4);
                                abs_cou--;
                                hi = !hi;
                            }

                            if (abs_cou > 0)
                            {
                                if (hi)
                                    pDest[j] |= (byte)(code2 >> 4);
                                else
                                    pDest[j] = (byte)(code2 & 0xF0);
                                abs_cou--;
                                hi = !hi;
                            }

                            if (abs_cou > 0)
                            {
                                if (hi)
                                    pDest[j] |= (byte)(code2 & 0x0F);
                                else
                                    pDest[j] = (byte)(code2 << 4);
                                abs_cou--;
                                hi = !hi;
                            }
                            continue;
                        }

                        if (code1 == 0)  // RLE_COMMAND
                        {
                            switch (code2)
                            {
                                case 0: // End of line escape EOL
                                    if (adj_cou == 0) adj_cou = 3 - ((pDest.Length - (sta_ptr.Length + 3)) % 4);
                                    for (i = 0; i < adj_cou; i++) pDest[j++] = 0;
                                    continue;
                                case 1: // End of block escape EOB
                                    if (adj_cou == 0) adj_cou = 3 - ((pDest.Length - (sta_ptr.Length + 3)) % 4);
                                    for (i = 0; i < adj_cou; i++) pDest[j++] = 0;
                                    bEOB = true;
                                    break;
                                case 2: // Delta escape. RLE_DELTA
                                    break;
                                default: // Literal packet
                                    abs_cou = code2;
                                    break;
                            }
                            continue;
                        }

                        if (!bEOB) // Literal
                        {
                            for (k = 0; k < code1 / 2; k++)
                            {
                                if (hi)
                                {
                                    pDest[j] |= (byte)(code2 >> 4);
                                    pDest[j] = (byte)(code2 & 0x0F);
                                }
                                else
                                    pDest[j++] = (byte)code2;
                            }

                            if (code1 % 2 != 0)
                            {
                                if (hi)
                                    pDest[j++] |= (byte)(code2 >> 4);
                                else
                                    pDest[j] = (byte)(code2 & 0xF0);
                                hi = !hi;
                            }
                        }
                    }

                    BufDest = new byte[xsize * ysize];
                    pDest = BufDest;

                    for (long y1 = ysize - 1; y1 >= 0; y1--)
                    {
                        for (uint x1 = 0; x1 < xtemp; x1++)
                        {
                            pDest[j] = (byte)(pTempData[(y1 * xtemp) + x1] >> 4);
                            pDest[j] |= (byte)((pTempData[(y1 * xtemp) + x1] << 4) & 0xF0);
                            j++;
                        }
                    }

                    // Liberar memoria del buffer temporal
                    pTempData = null;
                }
            }
            else
            {
                // 8 bit

                xtemp = xsize;
                if (ComprFlag == BI_RGB)
                {
                    BufDest = new byte[xsize * ysize];
                    pDest = BufDest;

                    for (long y1 = ysize - 1; y1 >= 0; y1--)
                    {
                        for (uint x1 = 0; x1 < xtemp; x1++)
                            pDest[j++] = BufSrc[(y1 * xtemp) + x1];
                    }
                }
                else
                {
                    // BI_RLE8
                    bool bEOB = false;
                    byte[] pTempData = new byte[xtemp * ysize];
                    pDest = pTempData;

                    int code1, code2, i, k;
                    int abs_cou = 0, adj_cou = 0;

                    byte[] sta_ptr = pDest;

                    for (i = 0; i < BufSrc.Length && !bEOB; i += 2)
                    {
                        code1 = BufSrc[i];
                        code2 = BufSrc[i + 1];

                        if (abs_cou > 0)
                        {
                            pDest[j++] = (byte)code1;
                            abs_cou--;
                            if (abs_cou > 0)
                            {
                                pDest[j++] = (byte)code2;
                                abs_cou--;
                            }
                            continue;
                        }

                        if (code1 == 0)  // RLE_COMMAND
                        {
                            switch (code2)
                            {
                                case 0: // End of line escape EOL
                                    if (adj_cou == 0) adj_cou = 3 - ((pDest.Length - (sta_ptr.Length + 3)) % 4);
                                    for (i = 0; i < adj_cou; i++) pDest[j++] = 0;
                                    continue;
                                case 1: // End of block escape EOB
                                    if (adj_cou == 0) adj_cou = 3 - ((pDest.Length - (sta_ptr.Length + 3)) % 4);
                                    for (i = 0; i < adj_cou; i++) pDest[j++] = 0;
                                    bEOB = true;
                                    break;
                                case 2: // Delta escape. RLE_DELTA
                                    break;
                                default: // Literal packet
                                    abs_cou = code2;
                                    break;
                            }
                            continue;
                        }

                        if (!bEOB) // Literal
                            for (k = 0; k < code1; k++)
                                pDest[j++] = (byte)code2;
                    }

                    BufDest = new byte[xsize * ysize];
                    pDest = BufDest;

                    for (long y1 = ysize - 1; y1 >= 0; y1--)
                    {
                        for (uint x1 = 0; x1 < xtemp; x1++)
                            pDest[j++] = pTempData[(y1 * xtemp) + x1];
                    }

                    // Liberar memoria del buffer temporal
                    pTempData = null;
                }
            }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
            pDest = BufDest;
            return true;
        }

        public  void ReadBMPInfo(string filePath)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(filePath))
                {
                    // Obtener el tipo de compresión del archivo BMP
                    ImageCodecInfo codecInfo = GetCodecInfo(ImageFormat.Bmp);
                    if (codecInfo != null)
                    {
                        Console.WriteLine($"Formato del archivo BMP: {codecInfo.FormatDescription}");
                    }

                    // Obtener información adicional del archivo BMP si es necesario
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                    Console.WriteLine($"Ancho: {bmpData.Width}, Alto: {bmpData.Height}");
                    Console.WriteLine($"Profundidad de bits por píxel: {Image.GetPixelFormatSize(bmp.PixelFormat)}");
                    Console.WriteLine($"Tamaño de imagen en bytes: {bmpData.Stride * bmpData.Height}");

                    // Liberar recursos
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo BMP: {ex.Message}");
            }
        }

        private  ImageCodecInfo GetCodecInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}