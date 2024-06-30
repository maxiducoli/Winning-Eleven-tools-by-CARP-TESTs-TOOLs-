using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIMTools
{
    public class BytesToTIM
    {
        public void CreateTIM(string outputPath, int width, int height, int palX, int palY, int VRAMx, int VRAMy, byte[] rawImage, byte[] palette)
        {
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Escribir el encabezado TIM
                WriteTIMHeader(writer, width, height, palX, palY, VRAMx, VRAMy,palette);
                long l = fs.Length;               
                // Escribir la imagen comprimida (rawImage)
                writer.Write(rawImage);
                long f = fs.Length;
                writer.Close();
            }
        }

        private void WriteTIMHeader(BinaryWriter writer, int width, int height, int paletteSize)
        {
            // Magic number y otras constantes del encabezado TIM
            ushort magic = 0x10; // Por ejemplo, 0x10 para TIM de 4 bits
            ushort flags = 0x08; // Modo 4 bits

            writer.Write(magic);
            writer.Write(flags);
            writer.Write((ushort)0); // Clut x, y
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            writer.Write((ushort)((paletteSize / 2) | ((paletteSize / 2) << 12))); // Clut colors
            writer.Write((ushort)0); // Image x, y
            writer.Write((ushort)0); // Image w, h
        }

        private void WriteTIMHeader(BinaryWriter writer, int largo, int alto, int palX, int palY,int VRAMx,int VRAMy, byte[] paleta)
        {

            if (paleta.Length == 32)
            {
                largo = largo * 2;
            }


            // Array con los datos de la cabecera
            byte[] header;

            // Comienzo de la cabecera de 4 bits
            byte[] header4 = { 0x10, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00 };
            // Segunda parde de la cabecera de 4 bits
            byte[] header4_2 = { 0x10, 0x00, 0x01, 0x00 };
            // Largo 4 bits
            ushort largo4 = (ushort)(largo / 4);


            // Comienzo de la cabecer de 8 bits
            byte[] header8 = { 0x10, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00 };
            // Segunda parde de la cabecera de 8 bits
            byte[] header8_2 = { 0x00, 0x01, 0x01, 0x00 };
            //Largo 8 bits
            ushort largo8 = (ushort)(largo / 2);
            

            /***********************************************/
            /* Datos compartidos de ambos tipos de formato *\
            /***********************************************/

            // relleno de ceros
            byte[] relleno = { 0x00, 0x00 };

            // Tamaño del TIM
            ushort dimension = (ushort)((largo * alto) + 12);

            // Paletax
            ushort palx = (ushort)palX;
            
            // Paletay
            ushort paly = (ushort)palY;
            
            // VRAMx
            ushort vramX = (ushort)VRAMx;
            
            //VRAMy
            ushort vramY = (ushort)VRAMy;

            // Alto
            ushort altura = (ushort)alto;

            try
            {
                /*
                    01 - header4 o header8
                    02 - palx
                    03 - paly
                    04 - header4_2 o header8_2
                    05 - dimension
                    06 - relleno
                    07 - VRAMx
                    08 - VRAMy
                    09 - Largo4 o Largo8
                    10 - altura
                 */


                if (paleta.Length == 32) // 16 Colores
                {
                    writer.Write(header4);
                    writer.Write(palx);
                    writer.Write(paly);
                    writer.Write(header4_2);
                    writer.Write(paleta);
                    writer.Write(dimension);
                    writer.Write(relleno);
                    writer.Write(vramX);
                    writer.Write(vramY);
                    writer.Write(largo4);
                    writer.Write(altura);
                }

                if (paleta.Length == 512) // 256 colores
                {
                    writer.Write(header8);
                    writer.Write(palx);
                    writer.Write(paly);
                    writer.Write(header8_2);
                    writer.Write(paleta);
                    writer.Write(dimension);
                    writer.Write(relleno);
                    writer.Write(vramX);
                    writer.Write(vramY);
                    writer.Write(largo8);
                    writer.Write(altura);
                }

            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }

        }

        public byte[] Palette(string path, int offset, int length)
        {
            byte[] palette = null;
            if (length == 4)
            {
                palette = new byte[32];
            }
            else
            {
                palette = new byte[512];
            }
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fs.Position = offset;
                    fs.Read(palette, 0, palette.Length);
                }
            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }

            return palette;
        }
    }
}
