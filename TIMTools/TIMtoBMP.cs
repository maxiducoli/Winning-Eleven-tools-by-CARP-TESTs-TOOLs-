using decompress;
using System.IO;
using System.Text;
namespace TIMTools
{
    public class TIMtoBMP
    {
        // Creamos un archivo TIM
        public void CrearArchivoTIM(string pathNuevoTIM,string pathGrafico, int offsetGrafico, string pathPaleta,
            int offsetPaleta,int alto,int largo, int TIMFormat)
        {
            byte[] timTEMP = null;
            int timTEMPSize;
            byte[] timHEADER = null;
            try
            {
                // Array de bytes con los datos del TIM RAW
                byte[] timRAW = DescomprimirTIMaBytes(pathGrafico, offsetGrafico);
                // Array de bytes con los datos de la paleta
                byte[] timPALETTE = TIMPalette(pathPaleta, offsetPaleta, TIMFormat);
                // Paleta de colores
                Color[] colores = ConvertTIMBytesToColors(timPALETTE);
                // Header del TIM
                if (TIMFormat == 4)
                {
                     timHEADER = Generate4BPPTIMHeader(colores, colores.Length / 16, 0, 0, largo, alto);
                }
                if (TIMFormat == 8)
                {
                    timHEADER = Generate8BPPTIMHeader(colores,colores.Length / 256,0,0, largo, alto);   
                }

                if (TIMFormat == 4)
                {
                    byte[] timRawTemp = new byte[8192];
                    Array.Copy(timRAW, timRawTemp, 8192);
                    timTEMPSize = timRawTemp.Length + timHEADER.Length;
                }
                else
                {
                    timTEMPSize = timRAW.Length + timHEADER.Length;
                }

                timTEMP = new byte[timTEMPSize];
                using (FileStream fs = new FileStream(pathNuevoTIM,FileMode.OpenOrCreate,FileAccess.Write))
                {
                    // Escribir el encabezado TIM
                    fs.Write(timHEADER, 0, timHEADER.Length);

                    // Escribir los datos RAW del TIM
                    fs.Write(timRAW, 0, timRAW.Length);

                }
            }
            catch (Exception ex) 
            {

                throw new IOException(ex.Message);
            }
        }
        // Convierte el archivo comprimido en un array de bytes con un TIM RAW dentro
        public byte[] DescomprimirTIMaBytes(string pathTIM, int offset)
        { 
            byte[] data = null;
            WeDecompress weDecompress = new WeDecompress();
            try
            {
              data = weDecompress.DescomprimirArchivoTIM(pathTIM, offset);
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }
            return data;
        }
        // Convierte los datos porporcionados en un HEADER TIM
        public byte[] CreateTIMHeader(byte[] timRaw, Color[] palette, int height, int width)
        {
            // Determinar el tipo de imagen (4 bits para 16 colores, 8 bits para 256 colores)
            int imageType = (palette.Length <= 16) ? 1 : 2; // 1 = 4 bits, 2 = 8 bits

            // Calcular el tamaño de la imagen en bytes
            int imageSize = timRaw.Length;

            // Calcular el tamaño del encabezado
            int headerSize = 32 + palette.Length * 2; // Tamaño fijo del encabezado TIM en bytes

            // Crear el array de bytes para el encabezado TIM
            byte[] header = new byte[headerSize];

            // Escribir los datos en el encabezado TIM
            // Magic Number
            header[0] = 0x10;
            header[1] = 0x00;
            header[2] = 0x00;
            header[3] = 0x00;

            // Image Type (1 = 4 bits, 2 = 8 bits)
            header[4] = (byte)imageType;

            // Flags
            header[5] = 0;

            // Clut X
            header[6] = 0;

            // Clut Y
            header[7] = 0;

            // Clut W
            header[8] = (byte)palette.Length;

            // Clut H
            header[9] = 0;

            // Image X
            Array.Copy(BitConverter.GetBytes((ushort)0), 0, header, 10, 2);

            // Image Y
            Array.Copy(BitConverter.GetBytes((ushort)0), 0, header, 12, 2);

            // Image W
            Array.Copy(BitConverter.GetBytes((ushort)width), 0, header, 14, 2);

            // Image H
            Array.Copy(BitConverter.GetBytes((ushort)height), 0, header, 16, 2);

            // Image Size
            Array.Copy(BitConverter.GetBytes((uint)imageSize), 0, header, 20, 4);

            // Padding
            Array.Copy(new byte[] { 0, 0, 0, 0 }, 0, header, 24, 4);

            // Clut
            for (int i = 0; i < palette.Length; i++)
            {
                Color color = palette[i];
                int r = (color.R >> 3) & 0x1F;
                int g = (color.G >> 3) & 0x1F;
                int b = (color.B >> 3) & 0x1F;
                ushort colorData = (ushort)((b << 10) | (g << 5) | r);
                Array.Copy(BitConverter.GetBytes(colorData), 0, header, 32 + i * 2, 2);
            }

            return header;
        }

        // TIM Header para PSX
        private static byte[] GenerateTIMHeader(int paletteSize, int height, int width)
        {

            byte[] header = new byte[64]; // Ajustamos el tamaño del encabezado a 64 bytes

            // Magic Number: Identifica el archivo como un TIM
            byte[] magicNumber = {0x10,0x00,0x00,0x00};
            Array.Copy(magicNumber, 0, header, 0, 4);

            // Tipo de Imagen: 8 bits (256 colores) si la paleta tiene más de 16 colores, de lo contrario 4 bits (16 colores)
            int imageType = (paletteSize > 16) ? 0x00000008 : 0x00000004;
            byte[] imageTypeBytes = BitConverter.GetBytes(imageType);
            Array.Copy(imageTypeBytes, 0, header, 4, 4);

            // Flags: 0x00000000 (sin flags)
            byte[] flags = BitConverter.GetBytes(0x00000000);
            Array.Copy(flags, 0, header, 8, 4);

            // Ancho de la imagen
            byte[] imageWidth = BitConverter.GetBytes(width);
            Array.Copy(imageWidth, 0, header, 12, 4);

            // Alto de la imagen
            byte[] imageHeight = BitConverter.GetBytes(height);
            Array.Copy(imageHeight, 0, header, 16, 4);

            // Tamaño de la imagen en bytes
            int imageSize = width * height;
            byte[] imageSizeBytes = BitConverter.GetBytes(imageSize);
            Array.Copy(imageSizeBytes, 0, header, 20, 4);

            // Reservado (Padding)
            Array.Copy(BitConverter.GetBytes(0x00000000), 0, header, 24, 4);

            // Clut X
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 28, 2);

            // Clut Y
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 30, 2);

            // Clut W (Ancho de la paleta)
            byte[] clutWidthBytes = BitConverter.GetBytes((ushort)paletteSize);
            Array.Copy(clutWidthBytes, 0, header, 32, 2);

            // Clut H (Alto de la paleta)
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 34, 2);

            // Posición en la imagen X
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 36, 2);

            // Posición en la imagen Y
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 38, 2);

            // Tamaño de la imagen
            Array.Copy(BitConverter.GetBytes(0x000018FF), 0, header, 40, 4);

            // Reservado
            Array.Copy(BitConverter.GetBytes(0x00000000), 0, header, 44, 4);

            // Clut X
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 48, 2);

            // Clut Y
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 50, 2);

            // Clut W (Ancho de la paleta)
            Array.Copy(BitConverter.GetBytes(0x00000001), 0, header, 52, 4);

            // Clut H (Alto de la paleta)
            Array.Copy(BitConverter.GetBytes(0x0000), 0, header, 56, 4);

            // Clut Size
            Array.Copy(BitConverter.GetBytes(0x00000110), 0, header, 60, 4);

            return header;
        }


        private static byte[] Generate4BPPTIMHeader(Color[] palette, int cluts, int imageOrgX, int imageOrgY, int imageWidth, int imageHeight)
        {
            byte[] header = new byte[64]; // Tamaño del encabezado 4BPP TIM

            // ID Tag para TIM Format
            byte[] magicNumber = Encoding.ASCII.GetBytes("10 00 00 00");
            Array.Copy(magicNumber, 0, header, 0, 4);

            // ID Tag para 4BPP
            byte[] bppTag = BitConverter.GetBytes(0x08);
            Array.Copy(bppTag, 0, header, 4, 4);

            // Paleta Org X y Paleta Org Y
            byte[] paletteOrgX = BitConverter.GetBytes(0x0000);
            Array.Copy(paletteOrgX, 0, header, 12, 2);

            byte[] paletteOrgY = BitConverter.GetBytes(0x0000);
            Array.Copy(paletteOrgY, 0, header, 14, 2);

            // Number of CLUTs
            byte[] numCluts = BitConverter.GetBytes(cluts);
            Array.Copy(numCluts, 0, header, 18, 2);

            // CLUT Data (16 Colors per CLUT, 32 bytes per CLUT)
            byte[] clutData = ConvertPaletteTo4BPPCLUTData(palette);
            Array.Copy(clutData, 0, header, 20, clutData.Length);

            // Image Org X, Image Org Y
            byte[] imageOrgXBytes = BitConverter.GetBytes(imageOrgX);
            Array.Copy(imageOrgXBytes, 0, header, 24, 2);

            byte[] imageOrgYBytes = BitConverter.GetBytes(imageOrgY);
            Array.Copy(imageOrgYBytes, 0, header, 26, 2);

            // Image Width (Multiply by 4 to get actual width), Image Height
            byte[] imageWidthBytes = BitConverter.GetBytes(imageWidth / 4); // Se divide por 4 porque es 4BPP
            Array.Copy(imageWidthBytes, 0, header, 28, 2);

            byte[] imageHeightBytes = BitConverter.GetBytes(imageHeight);
            Array.Copy(imageHeightBytes, 0, header, 30, 2);

            return header;
        }
        private static byte[] ConvertPaletteTo4BPPCLUTData(Color[] palette)
        {
            // Cada CLUT tiene 16 colores, y cada color es de 32 bytes (4 bytes por color)
            int numCluts = palette.Length / 16;
            byte[] clutData = new byte[numCluts * 32];

            for (int i = 0; i < numCluts; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int index = i * 16 + j;
                    clutData[i * 32 + j * 2] = palette[index].R;
                    clutData[i * 32 + j * 2 + 1] = palette[index].G;
                    clutData[i * 32 + j * 2 + 16] = palette[index].B;
                    clutData[i * 32 + j * 2 + 17] = palette[index].A;
                }
            }

            return clutData;
        }

        // 8 BITS
        private static byte[] Generate8BPPTIMHeader(Color[] palette, int cluts, int imageOrgX, int imageOrgY, int imageWidth, int imageHeight)
        {
            byte[] header = new byte[64]; // Tamaño del encabezado 8BPP TIM

            // ID Tag para TIM Format
            byte[] magicNumber = Encoding.ASCII.GetBytes("10 00 00 00");
            Array.Copy(magicNumber, 0, header, 0, 4);

            // ID Tag para 8BPP
            byte[] bppTag = BitConverter.GetBytes(0x09);
            Array.Copy(bppTag, 0, header, 4, 4);

            // Paleta Org X y Paleta Org Y
            byte[] paletteOrgX = BitConverter.GetBytes(0x0000);
            Array.Copy(paletteOrgX, 0, header, 12, 2);

            byte[] paletteOrgY = BitConverter.GetBytes(0x0000);
            Array.Copy(paletteOrgY, 0, header, 14, 2);

            // Number of CLUTs
            byte[] numCluts = BitConverter.GetBytes(cluts);
            Array.Copy(numCluts, 0, header, 18, 2);

            // CLUT Data (256 Colors per CLUT, 512 bytes per CLUT)
            byte[] clutData = ConvertPaletteTo8BPPCLUTData(palette);
            Array.Copy(clutData, 0, header, 20, clutData.Length);

            // Image Org X, Image Org Y
            byte[] imageOrgXBytes = BitConverter.GetBytes(imageOrgX);
            Array.Copy(imageOrgXBytes, 0, header, 24, 2);

            byte[] imageOrgYBytes = BitConverter.GetBytes(imageOrgY);
            Array.Copy(imageOrgYBytes, 0, header, 26, 2);

            // Image Width (Multiply by 2 to get actual width), Image Height
            byte[] imageWidthBytes = BitConverter.GetBytes(imageWidth / 2); // Se divide por 2 porque es 8BPP
            Array.Copy(imageWidthBytes, 0, header, 28, 2);

            byte[] imageHeightBytes = BitConverter.GetBytes(imageHeight);
            Array.Copy(imageHeightBytes, 0, header, 30, 2);

            return header;
        }
        private static byte[] ConvertPaletteTo8BPPCLUTData(Color[] palette)
        {
            // Cada CLUT tiene 256 colores, y cada color es de 512 bytes (4 bytes por color)
            int numCluts = palette.Length / 256;
            byte[] clutData = new byte[numCluts * 512];

            for (int i = 0; i < numCluts; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    int index = i * 256 + j;
                    clutData[i * 512 + j * 2] = palette[index].R;
                    clutData[i * 512 + j * 2 + 1] = palette[index].G;
                    clutData[i * 512 + j * 2 + 256] = palette[index].B;
                    clutData[i * 512 + j * 2 + 257] = palette[index].A;
                }
            }

            return clutData;
        }
        // Convierte un array de colores a un array de bytes
        public byte[] ConvertColorArrayToBytes(Color[] colors)
        {
            int colorSize = 3; // Tamaño de cada color en bytes (RGB)

            // Calcular el tamaño total del array de bytes
            int totalSize = colors.Length * colorSize;

            // Crear el array de bytes
            byte[] bytes = new byte[totalSize];

            // Iterar sobre cada color y convertirlo a bytes
            for (int i = 0; i < colors.Length; i++)
            {
                bytes[i * colorSize + 0] = colors[i].R;
                bytes[i * colorSize + 1] = colors[i].G;
                bytes[i * colorSize + 2] = colors[i].B;
            }

            return bytes;
        }

        // Convierte un array de colores a un array de bytes de paleta TIM
        public byte[] ConvertColorsToTIMPalette(Color[] colors)
        {
            // Calcular el tamaño de la paleta en bytes
            int paletteSize = colors.Length * 2;

            // Crear un array de bytes para la paleta TIM
            byte[] paletteData = new byte[paletteSize];

            // Convertir cada color a su representación en la paleta TIM
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];

                // Convertir RGB de 8 bits a 5 bits
                int r = (color.R >> 3) & 0x1F;
                int g = (color.G >> 3) & 0x1F;
                int b = (color.B >> 3) & 0x1F;

                // Combinar los componentes RGB en un solo ushort (16 bits)
                ushort colorData = (ushort)((b << 10) | (g << 5) | r);

                // Guardar el color en el array de bytes
                BitConverter.GetBytes(colorData).CopyTo(paletteData, i * 2);
            }

            return paletteData;
        }

        // Convierte un array de bytes de paleta TIM a un array de colores
        public Color[] ConvertTIMBytesToColors(byte[] timPaletteData)
        {
            // Calcular el número de colores en la paleta TIM
            int numColors = timPaletteData.Length / 2;

            // Crear un array de Color para almacenar los colores
            Color[] colors = new Color[numColors];

            // Convertir cada color de la paleta TIM a Color
            for (int i = 0; i < numColors; i++)
            {
                // Extraer los componentes R, G, B del ushort de la paleta TIM
                ushort colorData = BitConverter.ToUInt16(timPaletteData, i * 2);

                // Convertir de 16 bits (TIM) a 24 bits (Color)
                int r = (colorData & 0x1F) << 3;   // 5 bits de rojo
                int g = (colorData >> 5 & 0x1F) << 3; // 5 bits de verde
                int b = (colorData >> 10 & 0x1F) << 3; // 5 bits de azul

                // Crear el color y agregarlo al array de colors
                colors[i] = Color.FromArgb(r, g, b);
            }

            return colors;
        }

        // Leemos la paleta del color
        private byte[] TIMPalette(string timPalette, int offsetTTMPalette, int TIMFormat)
        { 
            byte[] paletteData = null ;

            if (TIMFormat < 4 || TIMFormat > 8)
                return paletteData;

            if (TIMFormat == 4)
                paletteData = new byte[32];

            if (TIMFormat == 8)
                paletteData = new byte[512];

            try
            {
                using (FileStream fs = new FileStream(timPalette,FileMode.Open,FileAccess.Read))
                {
                    fs.Position = offsetTTMPalette;
                    fs.Read(paletteData, 0, paletteData.Length);
                }
            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }

            return paletteData;
        }

        public Bitmap CreateBitmapFromRawData(byte[] rawData, Color[] palette, int width, int height, int bitsPerPixel)
        {

            if (bitsPerPixel == 4)
            {
                width = width * 2;
            }
            Bitmap bitmap = new Bitmap(width, height);
            int pixelsPerByte = 8 / bitsPerPixel;
            int dataLength = rawData.Length * pixelsPerByte;

            for (int i = 0; i < dataLength; i++)
            {
                int x = i % width;
                int y = i / width;

                // Asegúrate de que x e y estén dentro de los límites del bitmap
                if (x < width && y < height)
                {
                    int paletteIndex;

                    if (bitsPerPixel == 8)
                    {
                        paletteIndex = rawData[i];
                    }
                    else // bitsPerPixel == 4
                    {
                        int shift = (i % 2) * 4;
                        paletteIndex = (rawData[i / 2] >> shift) & 0x0F;
                    }

                    Color color = palette[paletteIndex];
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;

        }
    }
}
