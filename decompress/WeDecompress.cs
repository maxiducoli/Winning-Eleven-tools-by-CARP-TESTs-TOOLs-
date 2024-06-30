using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace decompress
{
    enum Punteros : uint
    {
        p_0C80 = 0x8000,
        p_0D80 = 0x8000,
        p_0E80 = 0x18000,
        p_0F80 = 0,
        P_1080 = 0x010000,
        P_1180 = 0x020000,
        P_1280 = 0x030000
    }

    enum TipoPuntero : uint
    {
        puntero_0C = 0x800C0000,
        puntero_0D = 2148335616,
        puntero_0E = 2148401152,
        puntero_0F = 2148466688,
        puntero_10 = 2148532224,
        puntero_11 = 2148597760,
        puntero_12 = 2148663296
    }

    public class WeDecompress
    {

        public byte[] DescomprimirBIN(string pathTIM, int offsetTIM, out int tam)
        {
            byte[] timData;
            byte[] buffer = new byte[100000];
            List<byte> datos = new List<byte>();
            int i = 0;
            int k = 0;
            int j = 0;
            byte k2;
            int k3;
            int dataIndex = 0;
            int DestIndex = 10;
            int contador = 0;

            using (FileStream file = new FileStream(pathTIM, FileMode.Open, FileAccess.Read))
            {
                file.Seek(offsetTIM, SeekOrigin.Begin);
                timData = new byte[file.Length - offsetTIM];
                file.Read(timData, 0, timData.Length);
            }

            long l = FindCompressedLength(timData);

            timData = new byte[(int)l];

            using (FileStream file = new FileStream(pathTIM, FileMode.Open, FileAccess.Read))
            {
                file.Seek(offsetTIM, SeekOrigin.Begin);
                file.Read(timData, 0, timData.Length);
            }

            while (true)
            {
                if ((i & 256) == 0)
                {
                    k = timData[dataIndex];
                    dataIndex++;
                    i = k + 65280;
                }
                k2 = timData[dataIndex];

                if ((i & 1) == 0)
                {
                    buffer[DestIndex] = k2;
                    DestIndex++;
                    dataIndex++;
                }
                else
                {
                    if ((k2 & 128) != 0)
                    {
                        dataIndex++;
                        if ((k2 & 64) != 0)
                        {
                            k = k2;
                            k3 = k - 185;
                            if (k == 255)
                            {
                                break;
                            }
                            for (i = k3; i > 0; i--)
                            {
                                k2 = timData[dataIndex];
                                dataIndex++;
                                DestIndex++;
                                buffer[DestIndex - 1] = k2;
                            }

                            i = i >> 1;
                        }
                        j = (k2 & 15) + 1;
                        k3 = (k2 >> 4) - 7;
                    }
                    else
                    {
                        j = timData[dataIndex + 1];
                        dataIndex += 2;

                        k3 = (k2 >> 2) + 2;
                        j = j | (k2 & 3) << 8;
                    }
                    for (i = k3; i > 0; i--)
                    {
                        byte tmp = buffer[DestIndex - j];
                        buffer[DestIndex] = tmp;
                        DestIndex++;
                    }

                }
                i = i >> 1;
            }
            tam = DestIndex;
            return buffer;
        }

        public byte[] DescomprimirArchivoTIM(string inputPath, int offset)
        {
            byte[] timData;
            List<byte> datos = new List<byte>();

            using (FileStream file = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                //file.Position = 0;
                //timData = new byte[file.Length];
                file.Seek(offset, SeekOrigin.Begin);
                timData = new byte[file.Length - offset];
                file.Read(timData, 0, timData.Length);
            }

            long l = FindCompressedLength(timData);

            timData = new byte[(int)l];

            using (FileStream file = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                file.Seek(offset, SeekOrigin.Begin);
                file.Read(timData, 0, timData.Length);
            }


            int modif = 0;
            int i = 0;
            int counter = 0;
            int orig = 0;
            int k3 = 0;
            int j = 0;
            while (true)
            {
                if ((i & 256) == 0)
                {
                    int k = timData[orig];
                    orig++;
                    counter++;
                    i = k + 65280;
                }

                int k2 = timData[orig];

                if ((i & 1) == 0)
                {
                    datos.Add((byte)k2);
                    modif++;
                    orig++;
                    counter++;
                }
                else
                {
                    if ((k2 & 128) != 0)
                    {
                        orig++;
                        counter++;
                        if ((k2 & 64) != 0)
                        {
                            int k = k2;
                            k3 = k - 185;
                            if (k == 255)
                            {
                                return datos.ToArray();
                            }
                            for (int nloop = k3; nloop >= 0; nloop--)
                            {
                                k2 = timData[orig];
                                orig++;
                                modif++;
                                datos.Add((byte)k2);
                            }
                            counter += k3;

                            i >>= 1;
                            continue;
                        }
                        j = (k2 & 15) + 1;
                        k3 = (k2 >> 4) - 7;
                    }
                    else
                    {
                        j = timData[orig + 1];
                        orig += 2;
                        counter += 2;
                        k3 = (k2 >> 2) + 2;
                        j |= (k2 & 3) << 8;
                    }
                    for (int nloop = k3; nloop >= 0; nloop--)
                    {
                        int tmp = datos[modif - j];
                        datos.Add((byte)tmp);
                        modif++;
                    }
                }
                i >>= 1;
            }
        }

        public byte[] DecompressTIMFile(string inputPath, int offset)
        {
            byte[] timData;

            using (FileStream file = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                file.Seek(offset, SeekOrigin.Begin);
                timData = new byte[file.Length - offset];
                file.Read(timData, 0, timData.Length);
            }

            byte[] decompressedData = new byte[timData.Length * 4];  // Tamaño final esperado según tus requerimientos
            if (DeCompress(ref decompressedData, timData))
                return decompressedData;
            else
                return null;
        }

        public bool DeCompress(ref byte[] BufDest, byte[] BufSrc)
        {
            BufDest = new byte[1];
            // Verifica que los buffers no sean nulos
            if (BufDest == null)
            {
                throw new ArgumentNullException(nameof(BufDest), "BufDest cannot be null.");
            }

            if (BufSrc == null)
            {
                throw new ArgumentNullException(nameof(BufSrc), "BufSrc cannot be null.");
            }

            // Inicializa los punteros y otros índices necesarios
            int ptrRes = 0;  // Índice en el buffer de destino
            int bufSrcIdx = 0;  // Índice en el buffer de origen

            long k3;  // Variable de control para bucles
            uint i, j;
            byte k, k2;

            i = 0;  // Inicialización de la variable i

            // Bucle principal de descompresión
            while (true)
            {
                // Si i & 0x100 es 0, lee el siguiente byte de BufSrc y ajusta i
                if ((i & 0x100) == 0)
                {
                    if (bufSrcIdx >= BufSrc.Length)
                    {
                        throw new IndexOutOfRangeException("BufSrc index out of range.");
                    }

                    k = BufSrc[bufSrcIdx++];
                    i = (uint)(k | 0xFF00);  // Ajusta i con el byte leído
                }

                // Lee el siguiente byte de BufSrc
                if (bufSrcIdx >= BufSrc.Length)
                {
                    throw new IndexOutOfRangeException("BufSrc index out of range.");
                }

                k2 = BufSrc[bufSrcIdx++];

                // Si el bit menos significativo de i es 0, copia el byte directamente
                if ((i & 1) == 0)
                {
                    // Si el índice ptrRes está fuera de los límites de BufDest, redimensiona BufDest
                    if (ptrRes >= BufDest.Length)
                    {
                        Array.Resize(ref BufDest, BufDest.Length + 1024); // Incremento dinámico del tamaño del buffer de destino
                    }

                    BufDest[ptrRes++] = k2;  // Copia el byte leído a BufDest
                }
                else
                {
                    // Si k2 & 0x80 es diferente de 0
                    if ((k2 & 0x80) != 0)
                    {
                        // Si k2 & 0x40 es diferente de 0
                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            k3 = k - 0xB9;  // Ajusta k3
                            if (k == 0xFF) break;  // Si k es 0xFF, termina el bucle

                            // Bucle para copiar varios bytes desde BufSrc a BufDest
                            while (k3-- >= 0)
                            {
                                if (bufSrcIdx >= BufSrc.Length)
                                {
                                    throw new IndexOutOfRangeException("BufSrc index out of range.");
                                }

                                k2 = BufSrc[bufSrcIdx++];

                                if (ptrRes >= BufDest.Length)
                                {
                                    Array.Resize(ref BufDest, BufDest.Length + 1024);
                                }

                                BufDest[ptrRes++] = k2;  // Copia el byte leído a BufDest
                            }

                            i >>= 1;  // Desplaza i a la derecha 1 bit
                            continue;
                        }

                        // Ajusta j y k3 para un caso particular de compresión
                        j = (uint)((k2 & 0x0F) + 1);
                        k3 = (k2 >> 4) - 7;
                    }
                    else
                    {
                        // Ajusta j y k3 para otro caso particular de compresión
                        if (bufSrcIdx + 1 >= BufSrc.Length)
                        {
                            throw new IndexOutOfRangeException("BufSrc index out of range.");
                        }

                        j = (uint)(BufSrc[bufSrcIdx + 1]);
                        bufSrcIdx += 2;
                        k3 = (k2 >> 2) + 2;
                        j |= (uint)((k2 & 3) << 8);
                    }

                    // Copia una secuencia de bytes en BufDest, utilizando una referencia a los bytes anteriores en BufDest
                    for (; k3 >= 0; k3--)
                    {
                        if (ptrRes >= BufDest.Length)
                        {
                            Array.Resize(ref BufDest, BufDest.Length + 1024);
                        }

                        // Verificación adicional para evitar índices fuera de rango
                        if (ptrRes - (int)j < 0 || ptrRes - (int)j >= BufDest.Length)
                        {
                            throw new IndexOutOfRangeException("Invalid reference in BufDest array.");
                        }

                        BufDest[ptrRes] = BufDest[ptrRes - (int)j];
                        ptrRes++;
                    }
                }

                i >>= 1;  // Desplaza i a la derecha 1 bit
            }

            // Ajusta el tamaño del buffer de destino al tamaño real de los datos descomprimidos
            Array.Resize(ref BufDest, ptrRes);

            return true;  // Retorna true si la descompresión es exitosa
        }

        public bool DeCompressRaw(ref byte[] bufDest, byte[] bufSrc)
        {
            List<byte> ptrRes = new List<byte>();
            int bufSrcIndex = 0;

            long k3;
            ulong i = 0;
            byte k, k2;

            while (true)
            {
                if ((i & 0x100) == 0) // (i < 0x100) or (0x1000 <i < 0x10FF) ...
                {
                    k = bufSrc[bufSrcIndex]; // Get data and do data % FF
                    bufSrcIndex++; // add pointer
                    i = (ulong)(k | 0xFF00); // i = 0xFF unito a k
                }

                k2 = bufSrc[bufSrcIndex]; // get data

                if (((byte)i & 1) == 0) // t = 0 se i pari, t = 1 se i dispari
                {                        // Literal
                    ptrRes.Add(k2); // res = k2
                    bufSrcIndex++;
                }
                else
                {
                    if ((k2 & 0x80) != 0) // k2 & 0x80 != 0 Solo se k2 > 0x80 il controllo passa
                    {                     // Caso in cui abbiamo 1 solo byte per il comando, 
                                          // nbit move = 4, nbitrepeat = 4
                        bufSrcIndex++; // add pointer

                        if ((k2 & 0x40) != 0) // k2 & 0x40 != 0 Solo se k2 > 0xC0 il controllo passa
                        {
                            // blocco per copiare i bytes in plain mode
                            // nrepeatmax = 0xFE-0xB9 = 69 7 bit
                            // nrepeatmin = 0xC0-0xB9 = 7

                            k = k2; // k = k2 & 0xFF
                            k3 = k - 0xB9; // k3 = k - 0xB9  k3 = numero ottenuto sottraendo k2 fatto passare
                            if (k == 0xFF)
                                break; // esci dal ciclo

                            // ciclo che copia i k3 bytes in plain mode
                            while (k3-- >= 0) // sicuramente k3 > 0
                            {
                                k2 = bufSrc[bufSrcIndex]; // get data
                                bufSrcIndex++; // add pointer
                                ptrRes.Add(k2); // write k2 on ptrRes
                            }

                            i >>= 1; // i SHR 1
                            continue;
                        }

                        // questo j mi dirà quanto dovrò spostarmi indietro per ripetere k3+1 volte il 
                        // ciclo (max 16)
                        // max j = 16, k3 = 4 con BF
                        int j = (k2 & 0x0F) + 1; // k2 & 0x0F = prendo gli ultimi 4 bit e sommo 1 
                        k3 = (k2 >> 4) - 7; // k2 >> 4 = tolgo gli ultimi 4 bit,e tolgo 7;
                                            // k3 mi dice quante volte devo ripetere il byte + 1 volta del ciclo + 1 messa in precedenza
                                            // es. 90 -> j = 1 [1 byte indietro], k3 = 90 >> 4 - 7 = 9 - 7 = 2 
                                            // -> 2 + 1 + 1 = 4 volte

                        // ciclo per ripetere k3 volte + 1 il byte a partire dalla posizione -j
                        for (; k3 >= 0; k3--) // loop until k3>=0
                        {
                            ptrRes.Add(ptrRes[ptrRes.Count - j]); // write data from first (far j bytes) for k3 times
                        }
                    }
                    else
                    {
                        // 2 bytes per il comando tipo 24 01
                        // nbit move = 14, nbitrepeat = 6

                        int j = bufSrc[bufSrcIndex + 1]; // get data (prendi il byte successivo)
                        bufSrcIndex += 2; // add pointer by 2
                        k3 = (k2 >> 2) + 2; // k3 = togli ultimi 2 bit + 2 (24 = 11 volte infatti)
                        j |= (k2 & 3) << 8; // j = j | (k2 & 3)*256
                                            // max j = 1024, k3 = 33 con 7F FF
                                            // numero di bytes da arretrare per poi scrivere k3 volte i bytes ritrovati
                                            // es 24 01 -> j = 01 | (24 & 3) << 8 = 1 | 0 = 1, k3 = 24 >> 2 + 2 = 9 + 2 = 11
                                            // -> 11 + 1 + 1 = 13 volte

                        // ciclo per ripetere k3 volte + 1 il byte a partire dalla posizione -j
                        for (; k3 >= 0; k3--) // loop until k3>=0
                        {
                            ptrRes.Add(ptrRes[ptrRes.Count - j]); // write data from first (far j bytes) for k3 times
                        }
                    }
                }

                i >>= 1;
            }

            bufDest = ptrRes.ToArray();
            return true;
        }


        public bool DeCompressBing(ref byte[] BufDest, byte[] BufSrc)
        {

            List<byte> ptrRes = new List<byte>();
            int ptrResIndex = 0;
            int bufSrcIndex = 0;
            int k3;
            UInt16 i, j;
            byte k, k2;
            i = 0;
            while (true)
            {
                if ((i & 0x100) == 0)
                {
                    k = BufSrc[bufSrcIndex];
                    bufSrcIndex++;
                    //BufSrc = BufSrc.Skip(1).ToArray();
                    i = (UInt16)(k | 0xFF00);
                }
                k2 = BufSrc[bufSrcIndex];
                if (((byte)i & 1) == 0)
                {
                    ptrRes.Add(k2);
                    //BufDest[ptrResIndex] = k2;
                    ptrResIndex++;
                    bufSrcIndex++;
                    //BufSrc = BufSrc.Skip(1).ToArray();
                }
                else
                {
                    if ((k2 & 0x80) != 0)
                    {
                        bufSrcIndex++;
                        //BufSrc = BufSrc.Skip(1).ToArray();
                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            k3 = k - 0xB9;
                            if (k == 0xFF)
                                break;
                            while (k3-- >= 0)
                            {
                                k2 = BufSrc[bufSrcIndex];
                                bufSrcIndex++;
                                //                                BufSrc = BufSrc.Skip(1).ToArray();
                                ptrResIndex++;
                                //byte b 
                                //ptrRes[ptrResIndex - 1] = k2;
                                ptrRes.Add(k2);
                                //BufDest[ptrResIndex - 1] = k2;
                            }
                            i >>= 1;
                            continue;
                        }
                        j = (UInt16)((k2 & 0x0F) + 1);
                        k3 = (k2 >> 4) - 7;
                    }
                    else
                    {
                        j = BufSrc[bufSrcIndex + 1];
                        bufSrcIndex = bufSrcIndex + 2;
                        //BufSrc = BufSrc.Skip(2).ToArray();
                        k3 = (k2 >> 2) + 2;
                        j |= (UInt16)((k2 & 3) << 8);
                    }
                    for (; k3 >= 0; k3--)
                    {
                        ptrRes.Add(ptrRes[ptrResIndex - (int)j]);
                        //BufDest[ptrResIndex] = BufDest[ptrResIndex - (int)j];
                        ptrResIndex++;
                    };
                }
                i >>= 1;
            }
            BufDest = ptrRes.ToArray();
            return true;
        }

        public bool DeCompress(byte[] BufSrc, out byte[] BufDest)
        {
            int i = 0;
            byte k, k2;
            long k3 = 0;
            using (MemoryStream memoryStream = new MemoryStream())
            {

                while (true)
                {
                    if ((i & 0x100) == 0)
                    {
                        k = BufSrc[0];
                        BufSrc = BufSrc.Skip(1).ToArray();
                        i = k | 0xFF00;
                    }
                    k2 = BufSrc[0];
                    if (((byte)i & 1) == 0)
                    {
                        memoryStream.WriteByte(k2);
                        BufSrc = BufSrc.Skip(1).ToArray();
                    }
                    else
                    {
                        if ((k2 & 0x80) != 0)
                        {
                            BufSrc = BufSrc.Skip(1).ToArray();
                            if ((k2 & 0x40) != 0)
                            {
                                k = k2;
                                k3 = k - 0xB9;
                                if (k == 0xFF)
                                    break;
                                while (k3-- >= 0)
                                {
                                    k2 = BufSrc[0];
                                    BufSrc = BufSrc.Skip(1).ToArray();
                                    memoryStream.WriteByte(k2);
                                }
                                i >>= 1;
                                continue;
                            }
                            uint j = (uint)((k2 & 0x0F) + 1);
                            k3 = (k2 >> 4) - 7;
                            while (k3-- >= 0)
                            {
                                byte b = memoryStream.ToArray()[memoryStream.ToArray().Length - j];
                                memoryStream.WriteByte(b);
                            }
                        }
                        else
                        {
                            uint j = BufSrc[1];
                            BufSrc = BufSrc.Skip(2).ToArray();
                            k3 = (k2 >> 2) + 2;
                            j |= (uint)((k2 & 3) << 8);
                            while (k3-- >= 0)
                            {
                                byte b = memoryStream.ToArray()[memoryStream.ToArray().Length - j];
                                memoryStream.WriteByte(b);
                            }
                        }
                    }
                    i >>= 1;
                }
                BufDest = memoryStream.ToArray();
            }
            return true;
        }


        /* public byte[] DescomprimirArchivoTIM(string inputPath, int offset)
         {
             List<byte> datos = new List<byte>();

             using (FileStream file = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
             {
                 file.Seek(offset, SeekOrigin.Begin);
                 byte[] timData = new byte[file.Length - offset];
                 file.Read(timData, 0, timData.Length);

                 int orig = 0;
                 int modif = 0;
                 int i = 0;
                 int bytesDescomprimidos = 0;

                 while (orig < timData.Length && bytesDescomprimidos < 8192)
                 {
                     if ((i & 256) == 0)
                     {
                         int k = timData[orig++];
                         i = k + 65280;
                     }

                     int k2 = timData[orig++];

                     if ((i & 1) == 0)
                     {
                         if (bytesDescomprimidos < 8192)
                         {
                             datos.Add((byte)k2);
                             modif++;
                             bytesDescomprimidos++;
                         }
                     }
                     else
                     {
                         if ((k2 & 128) != 0)
                         {
                             if ((k2 & 64) != 0)
                             {
                                 int k = k2;
                                 int k3 = k - 185;
                                 if (k == 255)
                                 {
                                     break; // Fin de la descompresión
                                 }
                                 for (int nloop = k3; nloop >= 0 && bytesDescomprimidos < 8192; nloop--)
                                 {
                                     if (bytesDescomprimidos < 8192)
                                     {
                                         k2 = timData[orig++];
                                         datos.Add((byte)k2);
                                         modif++;
                                         bytesDescomprimidos++;
                                     }
                                 }
                             }
                             else
                             {
                                 int j = (k2 & 15) + 1;
                                 int k3 = (k2 >> 4) - 7;

                                 for (int nloop = k3; nloop >= 0 && bytesDescomprimidos < 8192; nloop--)
                                 {
                                     if (bytesDescomprimidos < 8192)
                                     {
                                         int tmp = datos[modif - j];
                                         datos.Add((byte)tmp);
                                         modif++;
                                         bytesDescomprimidos++;
                                     }
                                 }
                             }
                         }
                         else
                         {
                             int j = timData[orig++];
                             int k3 = (k2 >> 2) + 2;
                             j |= (k2 & 3) << 8;

                             for (int nloop = k3; nloop >= 0 && bytesDescomprimidos < 8192; nloop--)
                             {
                                 if (bytesDescomprimidos < 8192)
                                 {
                                     int tmp = datos[modif - j];
                                     datos.Add((byte)tmp);
                                     modif++;
                                     bytesDescomprimidos++;
                                 }
                             }
                         }
                     }
                     i >>= 1;
                 }
             }

             // Ajustamos la longitud del arreglo datos a 8192 bytes si fuera necesario
             if (datos.Count < 8192)
             {
                 datos.AddRange(new byte[8192 - datos.Count]);
             }
             else if (datos.Count > 8192)
             {
                 datos.RemoveRange(8192, datos.Count - 8192);
             }

             return datos.ToArray();
         }
        */
        private Color[] LeerPaletaTIM(string rutaPaleta, int offset, int bits)
        {
            byte[] paletaBytes = PaletaTIM(rutaPaleta, offset, bits);
            return ConvertTimPaletteToBmpPalette(paletaBytes);
        }

        private byte[] PaletaTIM(string rutaPaleta, int offset, int bits)
        {
            // Validar los parámetros de entrada
            if (string.IsNullOrEmpty(rutaPaleta))
                throw new ArgumentException("La ruta de la paleta no puede ser nula o vacía", nameof(rutaPaleta));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "El offset no puede ser negativo");

            if (bits != 4 && bits != 8)
                throw new ArgumentException("Los bits deben ser 4 u 8", nameof(bits));

            byte[] result;

            try
            {
                using (FileStream archivo = new FileStream(rutaPaleta, FileMode.Open, FileAccess.Read))
                {
                    archivo.Seek(offset, SeekOrigin.Begin);
                    if (bits == 4)
                    {
                        result = new byte[32];
                    }
                    else if (bits == 8)
                    {
                        result = new byte[512];
                    }
                    else
                    {
                        throw new ArgumentException("Formato de bits no soportado");
                    }

                    archivo.Read(result, 0, result.Length);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Error al leer la paleta del archivo", ex);
            }

            return result;
        }

        private Color[] ConvertTimPaletteToBmpPalette(byte[] timPalette)
        {
            if (timPalette.Length % 2 != 0)
            {
                throw new ArgumentException("Longitud de paleta TIM no válida.");
            }

            Color[] bmpPalette = new Color[timPalette.Length / 2];

            for (int i = 0; i < bmpPalette.Length; i++)
            {
                byte r = (byte)(timPalette[i * 2] & 0x1F);
                byte g = (byte)((timPalette[i * 2 + 1] >> 5) & 0x1F);
                byte b = (byte)((timPalette[i * 2 + 1]) & 0x1F);

                r = (byte)(r * 8);
                g = (byte)(g * 8);
                b = (byte)(b * 8);
                // byte a = (byte)0xFF;

                bmpPalette[i] = Color.FromArgb(r, g, b);
            }

            return bmpPalette;
        }

        public byte[] ConvertTimPaletteToBmpPalette(byte[] timPalette, int bitDepth)
        {
            int numColors = bitDepth == 4 ? 16 : 256;
            byte[] bmpPalette = new byte[numColors * 3];

            for (int i = 0; i < numColors; i++)
            {
                ushort color = BitConverter.ToUInt16(timPalette, i * 2);
                byte r = (byte)((color >> 10) & 0x1F);
                byte g = (byte)((color >> 5) & 0x1F);
                byte b = (byte)(color & 0x1F);

                bmpPalette[i * 3 + 0] = (byte)((r * 255) / 31); // Red
                bmpPalette[i * 3 + 1] = (byte)((g * 255) / 31); // Green
                bmpPalette[i * 3 + 2] = (byte)((b * 255) / 31); // Blue
            }

            return bmpPalette;
        }

        public Color[] ConvertTimPaletteToColorArray(byte[] timPalette, int bitDepth)
        {
            int numColors = bitDepth == 4 ? 16 : 256;
            // Asumiendo que cada color en la paleta TIM está representado por 2 bytes
            //int colorsCount = timPalette.Length / 2;
            Color[] colorArray = new Color[numColors];

            for (int i = 0; i < numColors; i++)
            {
                // Los colores en la paleta TIM están en formato BGR555
                ushort color = BitConverter.ToUInt16(timPalette, i * 2);
                int blue = (color & 0x7C00) >> 10;
                int green = (color & 0x03E0) >> 5;
                int red = color & 0x001F;

                // Convertir de BGR555 a RGB888
                red = (red << 3) | (red >> 2);
                green = (green << 3) | (green >> 2);
                blue = (blue << 3) | (blue >> 2);

                colorArray[i] = Color.FromArgb(red, green, blue);
            }

            return colorArray;
        }

        public void CrearBMP(string rutaNuevoArchivoBMP, string rutaBIN, int offsetGrafico, string rutaPaleta, int offsetPaleta, int alto, int ancho, int bits, out string rutaNueva)
        {
            byte[] grafico = DescomprimirArchivoTIM(rutaBIN, offsetGrafico);
            byte[] paleta = PaletaTIM(rutaPaleta, offsetPaleta, bits);
            //Array.Reverse(paleta);
            Color[] colores = ConvertTimPaletteToColorArray(paleta,bits);
            Bitmap bmp = null;
            Random contador = new Random();

            try
            {
                rutaNueva = string.Empty;
                string path = Path.GetDirectoryName(rutaNuevoArchivoBMP);
                string name = Path.GetFileNameWithoutExtension(rutaNuevoArchivoBMP);
                string extension = Path.GetExtension(rutaNuevoArchivoBMP);

                // Crear un nuevo objeto Bitmap
                switch (bits)
                {
                    case 4:
                        bmp = new Bitmap( alto ,ancho * 2, PixelFormat.Format4bppIndexed);
                        break;
                    case 8:
                        bmp = new Bitmap(alto,ancho, PixelFormat.Format8bppIndexed);
                        break;
                    default:
                        throw new ArgumentException("Formato de bits no soportado");
                }

                // Validar que el tamaño del Bitmap sea el esperado
                //if (bmp.Width != alto || bmp.Height != ancho)
                //{
                //    throw new ArgumentException("El tamaño del Bitmap creado no coincide con el tamaño especificado.");
                //}

                // Configurar la paleta de colores
                ColorPalette colorPalette = bmp.Palette;
                for (int i = 0; i < colores.Length; i++)
                {
                    colorPalette.Entries[i] = colores[i];
                }
                bmp.Palette = colorPalette;

                // Bloque de datos de la imagen RAW
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int offset = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        // Calcula la posición en el array grafico
                        int index = y * bmpData.Stride + (x * bits / 8);

                        // Verifica si aún hay datos en el array grafico
                        if (offset < grafico.Length)
                        {
                            // Copia un byte del array grafico al bloque de datos de la imagen
                            byte pixelValue = grafico[offset++];
                            Marshal.WriteByte(bmpData.Scan0, index, pixelValue);
                        }
                        else
                        {
                            // Si no hay más datos, puedes decidir qué hacer (por ejemplo, salir del bucle)
                            break;
                        }
                    }
                }

                bmp.UnlockBits(bmpData);

                // Generar un nombre de archivo único si el archivo ya existe
                rutaNueva = rutaNuevoArchivoBMP;
                while (File.Exists(rutaNueva))
                {
                    rutaNueva = Path.Combine(path, $"{name}_{contador.Next()}{extension}");
                }

                // Guardar el archivo BMP en disco
                bmp.Save(rutaNueva, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                // Manejar la excepción y mostrar el mensaje
                MessageBox.Show($"Ocurrió un error: {ex.Message}");
                throw;
            }
            finally
            {
                // Liberar recursos siempre
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }

            


        }

        public void CrearBMP(string rutaNuevoArchivoBMP, byte[] data, int offsetGrafico, string rutaPaleta, int offsetPaleta, int alto, int ancho, int bits, out string rutaNueva)
        {
            /*byte[] grafico = DescomprimirArchivoTIM(rutaBIN, offsetGrafico);
            byte[] paleta = PaletaTIM(rutaPaleta, offsetPaleta, bits);
            //Array.Reverse(paleta);
            Color[] colores = ConvertTimPaletteToColorArray(paleta,bits);
            Bitmap bmp = null;
            Random contador = new Random();

            try
            {
                rutaNueva = string.Empty;
                string path = Path.GetDirectoryName(rutaNuevoArchivoBMP);
                string name = Path.GetFileNameWithoutExtension(rutaNuevoArchivoBMP);
                string extension = Path.GetExtension(rutaNuevoArchivoBMP);

                // Crear un nuevo objeto Bitmap
                switch (bits)
                {
                    case 4:
                        bmp = new Bitmap( alto ,ancho, PixelFormat.Format4bppIndexed);
                        break;
                    case 8:
                        bmp = new Bitmap(alto,ancho, PixelFormat.Format8bppIndexed);
                        break;
                    default:
                        throw new ArgumentException("Formato de bits no soportado");
                }

                // Validar que el tamaño del Bitmap sea el esperado
                //if (bmp.Width != alto || bmp.Height != ancho)
                //{
                //    throw new ArgumentException("El tamaño del Bitmap creado no coincide con el tamaño especificado.");
                //}

                // Configurar la paleta de colores
                ColorPalette colorPalette = bmp.Palette;
                for (int i = 0; i < colores.Length; i++)
                {
                    colorPalette.Entries[i] = colores[i];
                }
                bmp.Palette = colorPalette;

                // Bloque de datos de la imagen RAW
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int offset = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        // Calcula la posición en el array grafico
                        int index = y * bmpData.Stride + (x * bits / 8);

                        // Verifica si aún hay datos en el array grafico
                        if (offset < grafico.Length)
                        {
                            // Copia un byte del array grafico al bloque de datos de la imagen
                            byte pixelValue = grafico[offset++];
                            Marshal.WriteByte(bmpData.Scan0, index, pixelValue);
                        }
                        else
                        {
                            // Si no hay más datos, puedes decidir qué hacer (por ejemplo, salir del bucle)
                            break;
                        }
                    }
                }

                bmp.UnlockBits(bmpData);

                // Generar un nombre de archivo único si el archivo ya existe
                rutaNueva = rutaNuevoArchivoBMP;
                while (File.Exists(rutaNueva))
                {
                    rutaNueva = Path.Combine(path, $"{name}_{contador.Next()}{extension}");
                }

                // Guardar el archivo BMP en disco
                bmp.Save(rutaNueva, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                // Manejar la excepción y mostrar el mensaje
                MessageBox.Show($"Ocurrió un error: {ex.Message}");
                throw;
            }
            finally
            {
                // Liberar recursos siempre
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }*/

            byte[] grafico = data;// DescomprimirArchivoTIM(rutaBIN, offsetGrafico);
            byte[] paleta = PaletaTIM(rutaPaleta, offsetPaleta, bits);
            Color[] colores = ConvertTimPaletteToColorArray(paleta, bits);
            Bitmap bmp = null;
            Random contador = new Random();

            try
            {
                rutaNueva = string.Empty;
                string path = Path.GetDirectoryName(rutaNuevoArchivoBMP);
                string name = Path.GetFileNameWithoutExtension(rutaNuevoArchivoBMP);
                string extension = Path.GetExtension(rutaNuevoArchivoBMP);

                // Crear un nuevo objeto Bitmap
                switch (bits)
                {
                    case 4:
                        bmp = new Bitmap(ancho, alto, PixelFormat.Format4bppIndexed);
                        break;
                    case 8:
                        bmp = new Bitmap(ancho, alto, PixelFormat.Format8bppIndexed);
                        break;
                    default:
                        throw new ArgumentException("Formato de bits no soportado");
                }

                // Configurar la paleta de colores
                ColorPalette colorPalette = bmp.Palette;
                for (int i = 0; i < colores.Length; i++)
                {
                    colorPalette.Entries[i] = colores[i];
                }
                bmp.Palette = colorPalette;

                // Bloque de datos de la imagen RAW
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int bytesPerPixel = bits / 8;
                int stride = bmpData.Stride;
                IntPtr ptr = bmpData.Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        // Calcula la posición en el array grafico
                        int index = y * stride + x * bytesPerPixel;

                        // Verifica si aún hay datos en el array grafico
                        if (index < grafico.Length)
                        {
                            // Copia un byte del array grafico al bloque de datos de la imagen
                            byte pixelValue = grafico[y * bmp.Width + x];
                            Marshal.WriteByte(ptr, index, pixelValue);
                        }
                    }
                }

                bmp.UnlockBits(bmpData);

                // Generar un nombre de archivo único si el archivo ya existe
                rutaNueva = rutaNuevoArchivoBMP;
                while (File.Exists(rutaNueva))
                {
                    rutaNueva = Path.Combine(path, $"{name}_{contador.Next()}{extension}");
                }

                // Guardar el archivo BMP en disco
                bmp.Save(rutaNueva, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                // Manejar la excepción y mostrar el mensaje
                MessageBox.Show($"Ocurrió un error: {ex.Message}");
                throw;
            }
            finally
            {
                // Liberar recursos siempre
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }


        }

        private void WriteBmpFileHeader(FileStream fs, int width, int height, int bpp)
        {
            // Calcula el tamaño del archivo BMP
            int fileSize = 54 + (width * height * bpp / 8);

            // Encabezado de archivo BMP
            byte[] bfType = Encoding.ASCII.GetBytes("BM");
            byte[] bfSize = BitConverter.GetBytes(fileSize);
            byte[] bfReserved1 = new byte[2];
            byte[] bfReserved2 = new byte[2];
            byte[] bfOffBits = BitConverter.GetBytes(54); // Tamaño del encabezado + tamaño del encabezado de DIB

            // Encabezado de DIB
            byte[] biSize = BitConverter.GetBytes(40);
            byte[] biWidth = BitConverter.GetBytes(width);
            byte[] biHeight = BitConverter.GetBytes(height);
            byte[] biPlanes = BitConverter.GetBytes((ushort)1);
            byte[] biBitCount = BitConverter.GetBytes((ushort)bpp);
            byte[] biCompression = new byte[4];
            byte[] biSizeImage = new byte[4];
            byte[] biXPelsPerMeter = new byte[4];
            byte[] biYPelsPerMeter = new byte[4];
            byte[] biClrUsed = new byte[4];
            byte[] biClrImportant = new byte[4];

            // Escribir en el archivo
            fs.Write(bfType, 0, 2);
            fs.Write(bfSize, 0, 4);
            fs.Write(bfReserved1, 0, 2);
            fs.Write(bfReserved2, 0, 2);
            fs.Write(bfOffBits, 0, 4);

            fs.Write(biSize, 0, 4);
            fs.Write(biWidth, 0, 4);
            fs.Write(biHeight, 0, 4);
            fs.Write(biPlanes, 0, 2);
            fs.Write(biBitCount, 0, 2);
            fs.Write(biCompression, 0, 4);
            fs.Write(biSizeImage, 0, 4);
            fs.Write(biXPelsPerMeter, 0, 4);
            fs.Write(biYPelsPerMeter, 0, 4);
            fs.Write(biClrUsed, 0, 4);
            fs.Write(biClrImportant, 0, 4);
        }

        public byte[] DescomprimirArchivo(string inputPath, int offset)
        {
            List<byte> datos = new List<byte>();

            using (FileStream file = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                file.Seek(offset, SeekOrigin.Begin);
                byte[] timData = new byte[file.Length - offset];
                file.Read(timData, 0, timData.Length);

                int index = 0;
                int outputIndex = 0;

                while (index < timData.Length)
                {
                    byte controlByte = timData[index++];
                    int count;
                    int offsetBack;

                    if ((controlByte & 0x80) == 0)
                    {
                        count = 1;
                        offsetBack = 1;
                    }
                    else if ((controlByte & 0xC0) == 0x80)
                    {
                        count = (controlByte & 0x3F) + 2;
                        offsetBack = ((controlByte >> 6) & 3) + 1;

                        if (offsetBack == 4)
                        {
                            offsetBack = timData[index++] + 1;
                        }
                    }
                    else
                    {
                        count = (controlByte & 0x3F) + 1;
                        offsetBack = ((controlByte >> 6) & 3) + 1;

                        if (offsetBack == 4)
                        {
                            offsetBack = timData[index++] + 1;
                        }

                        offsetBack = (offsetBack << 8) | timData[index++];
                    }

                    int startPos = outputIndex - offsetBack;
                    int endPos = startPos + count;

                    for (int i = startPos; i < endPos; i++)
                    {
                        datos.Add(datos[i]);
                        outputIndex++;
                    }
                }
            }

            return datos.ToArray();
        }


        public void CrearBMPs(string rutaNuevoArchivoBMP, byte[] raw, int offsetGrafico, string rutaPaleta, int offsetPaleta, int alto, int ancho, int bits)
        {
            byte[] grafico = raw;
            byte[] paleta = PaletaTIM(rutaPaleta, offsetPaleta, bits);
            Color[] colores = ConvertTimPaletteToColorArray(paleta, bits);
            Bitmap bmp = null;
            Random contador = new Random();

            try
            {
                rutaNuevoArchivoBMP = string.Empty;
                string path = Path.GetDirectoryName(rutaNuevoArchivoBMP);
                string name = Path.GetFileNameWithoutExtension(rutaNuevoArchivoBMP);
                string extension = Path.GetExtension(rutaNuevoArchivoBMP);

                // Crear un nuevo objeto Bitmap
                switch (bits)
                {
                    case 4:
                        bmp = new Bitmap(ancho, alto, PixelFormat.Format4bppIndexed);
                        break;
                    case 8:
                        bmp = new Bitmap(ancho, alto, PixelFormat.Format8bppIndexed);
                        break;
                    default:
                        throw new ArgumentException("Formato de bits no soportado");
                }

                // Configurar la paleta de colores
                ColorPalette colorPalette = bmp.Palette;
                for (int i = 0; i < colores.Length; i++)
                {
                    colorPalette.Entries[i] = colores[i];
                }
                bmp.Palette = colorPalette;

                // Bloque de datos de la imagen RAW
                BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int bytesPerPixel = bits / 8; // 1 byte por píxel en este caso
                int stride = bmpData.Stride;
                IntPtr ptr = bmpData.Scan0;

                // Tamaño del bloque de datos
                int size = bmpData.Stride * bmp.Height;

                // Población de la gama de píxeles
                byte[] pixels = new byte[size];
                int index = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        byte pixelValue = grafico[index / 2];

                        // El primer píxel ocupa los 4 bits más significativos de cada byte
                        // El segundo píxel ocupa los 4 bits menos significativos de cada byte
                        if (index % 2 == 0)
                        {
                            pixelValue = (byte)(pixelValue >> 4);
                        }
                        else
                        {
                            pixelValue = (byte)(pixelValue & 0x0F);
                            index++;
                        }

                        pixels[y * stride + x] = pixelValue;
                    }
                }

                // Copia de los datos de píxeles en el bloque de datos de la imagen
                Marshal.Copy(pixels, 0, ptr, size);

                bmp.UnlockBits(bmpData);

                // Generar un nombre de archivo único si el archivo ya existe
                
                while (File.Exists(rutaNuevoArchivoBMP))
                {
                    rutaNuevoArchivoBMP = Path.Combine(path, $"{name}_{contador.Next()}{extension}");
                }

                // Guardar el archivo BMP en disco
                bmp.Save(rutaNuevoArchivoBMP, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                // Manejar la excepción y mostrar el mensaje
                MessageBox.Show($"Ocurrió un error: {ex.Message}");
                throw;
            }
            finally
            {
                // Liberar recursos siempre
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }
        }



        // Obtenemos el tamaño del header del BIN
        public int HeaderSize(string filePath)
        {
            int result = 0;
            int offsetIndice = 0;
            int offsetDatos = 0;
            byte[] datos = new byte[4];
            byte[] offset = new byte[16];
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Leemos los datos de la cabecera
                    fs.Position = 0;
                    fs.Read(datos, 0, datos.Length);
                    // Vamos al offset del índice
                    offsetIndice = Puntero(datos);
                    fs.Position = offsetIndice;
                    fs.Read(offset, 0, offset.Length);
                    datos[0] = offset[12];
                    datos[1] = offset[13];
                    datos[2] = offset[14];
                    datos[3] = offset[15];

                }

                result = Puntero(datos);
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }
            return result;
        }

        // Obtenemos el offset del array de bytes
        public int Puntero(byte[] datos)
        {
            int result = 0;
            byte[] bytes = new byte[4];
            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = datos[2];
            bytes[3] = datos[3];
            TipoPuntero tipoPuntero = (TipoPuntero)BitConverter.ToInt32(bytes, 0);
            int pt = BitConverter.ToInt32(bytes, 0);
            Array.Clear(bytes, 0, bytes.Length - 1);
            try
            {
                bytes[0] = datos[0];
                bytes[1] = datos[1];
                bytes[2] = 0;
                bytes[3] = 0;
                switch (tipoPuntero)
                {
                    case TipoPuntero.puntero_0C:
                        result = BitConverter.ToInt32(bytes) - (int)Punteros.p_0C80;
                        break;
                    case TipoPuntero.puntero_0D:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.p_0D80;
                        break;
                    case TipoPuntero.puntero_0E:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.p_0E80;
                        break;
                    case TipoPuntero.puntero_0F:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.p_0F80;
                        break;
                    case TipoPuntero.puntero_10:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.P_1080;
                        break;
                    case TipoPuntero.puntero_11:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.P_1180;
                        break;
                    case TipoPuntero.puntero_12:
                        result = BitConverter.ToInt32(bytes) + (int)Punteros.P_1280;
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }

            return result;
        }

        // Retorna el listado de offsets que apuntan al índice del archivo BIN
        public List<int> ObtenerListadoOffset(string pathArchivo, int HeaderSize)
        {
            List<int> result = new List<int>();
            int offset = 0;
            int contador = 0;
            byte[] buffer = new byte[4];
            try
            {
                using (FileStream fs = new FileStream(pathArchivo, FileMode.Open, FileAccess.Read))
                {
                    while (contador <= HeaderSize)
                    {
                        fs.Position = contador;
                        fs.Read(buffer, 0, buffer.Length);
                        offset = Puntero(buffer);
                        if (offset > 0)
                        {
                            result.Add(offset);
                        }
                        contador = contador + 4;
                    }
                }
            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }

            return result;
        }

        // Retorna el listado de gráficos y paletas del BIN
        //public List<byte[]> ListadoDeDatos(string pathArchivo)
        //{
        //    List<byte[]> result = new List<byte[]>();
        //    List<byte[]> r = new List<byte[]>();
        //    // Tamaño del header
        //    int header = 0;
        //    // Listado de offsets de los índices
        //    List<int> listOffsetsIndices = new List<int>();
        //    // buffer de punteros
        //    byte[] buffer = new byte[16];
        //    byte[] arrayCorte = new byte[4];
        //    byte[] arrayCorte2 = { 0xFF, 0x00, 0x00, 0x00 };
        //    int offsetTemp = 0;
        //    try
        //    {
        //        header = HeaderSize(pathArchivo);
        //        listOffsetsIndices = ObtenerListadoOffset(pathArchivo, header);

        //        foreach (int offset in listOffsetsIndices)
        //        {
        //            offsetTemp = offset;
        //            using (FileStream fs = new FileStream(pathArchivo, FileMode.Open, FileAccess.Read))
        //            {
        //                fs.Position = offsetTemp;
        //                fs.Read(buffer, 0, buffer.Length);
        //                arrayCorte[0] = buffer[0];
        //                arrayCorte[1] = buffer[1];
        //                arrayCorte[2] = buffer[2];
        //                arrayCorte[3] = buffer[3];


        //                while (!arrayCorte.SequenceEqual(arrayCorte2)) 
        //                {
        //                    r.Add(buffer);
        //                    offsetTemp += 16;
        //                    fs.Position = offsetTemp;
        //                    fs.Read(buffer, 0, buffer.Length);
        //                    arrayCorte[0] = buffer[0];
        //                    arrayCorte[1] = buffer[1];
        //                    arrayCorte[2] = buffer[2];
        //                    arrayCorte[3] = buffer[3];
        //                }
        //            }
        //        }
        //        result = r;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw new IOException(ex.Message);
        //    }

        //    return result;
        //}

        public List<byte[]> ListadoDeDatos(string pathArchivo)
        {
            List<byte[]> result = new List<byte[]>();
            // Tamaño del header
            int header = 0;
            // Listado de offsets de los índices
            List<int> listOffsetsIndices = new List<int>();
            byte[] arrayCorte = new byte[4];
            byte[] arrayCorte2 = { 0xFF, 0x00, 0x00, 0x00 };
            int offsetTemp = 0;
            try
            {
                header = HeaderSize(pathArchivo);
                listOffsetsIndices = ObtenerListadoOffset(pathArchivo, header);

                foreach (int offset in listOffsetsIndices)
                {
                    offsetTemp = offset;
                    using (FileStream fs = new FileStream(pathArchivo, FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = offsetTemp;
                        int bytesRead = fs.Read(arrayCorte, 0, arrayCorte.Length);

                        while (bytesRead == arrayCorte.Length && !arrayCorte.SequenceEqual(arrayCorte2))
                        {
                            byte[] buffer = new byte[16];
                            Buffer.BlockCopy(arrayCorte, 0, buffer, 0, arrayCorte.Length);
                            fs.Position = offsetTemp + arrayCorte.Length;
                            bytesRead = fs.Read(buffer, arrayCorte.Length, buffer.Length - arrayCorte.Length);

                            result.Add(buffer);
                            offsetTemp += buffer.Length;
                            fs.Position = offsetTemp;
                            bytesRead = fs.Read(arrayCorte, 0, arrayCorte.Length);
                        }
                    }
                }
                result = result;
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }

            return result;
        }

        public long FindCompressedLength(byte[] bufSrc)
        {
            long counter = 0;
            ulong i = 0;
            byte k, k2;

            int index = 0;

            while (true)
            {
                if ((i & 0x100) == 0)
                {
                    k = bufSrc[index];
                    index++; // add pointer
                    counter++; // counter
                    i = (ulong)(k | 0xFF00);
                }

                if (bufSrc[index] == 0 && bufSrc[index + 1] == 0 && bufSrc[index + 2] == 0 && bufSrc[index + 3] == 0)
                    return 0; // exit invalid compressed block

                k2 = bufSrc[index];

                if (((byte)i & 1) == 0)
                {
                    index++;
                    counter++; // counter
                }
                else
                {
                    if ((k2 & 0x80) != 0)
                    {
                        index++; // add pointer
                        counter++; // counter

                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            long k3 = k - 0xB9;
                            if (k == 0xFF)
                                break; // exit

                            while (k3-- >= 0)
                            {
                                k2 = bufSrc[index];
                                index++; // add pointer
                                counter++; // counter
                            }

                            i >>= 1; // i SHR 1
                            continue;
                        }
                    }
                    else
                    {
                        byte j = bufSrc[index + 1];
                        index += 2; // add pointer by 2
                        counter += 2; // counter
                    }
                }

                i >>= 1;
            }

            return counter;
        }

        public long FindCompressedLength_1(byte[] bufSrc)
        {
            long counter = 0;
            ulong i = 0;
            byte k, k2;

            int index = 0;

            while (true)
            {
                if ((i & 0x100) == 0)
                {
                    k = bufSrc[index];
                    index++; // add pointer
                    counter++; // counter
                    i = (ulong)(k | 0xFF00);
                }

                if (bufSrc[index] == 0 && bufSrc[index + 1] == 0 && bufSrc[index + 2] == 0 && bufSrc[index + 3] == 0)
                    return 0; // exit invalid compressed block

                k2 = bufSrc[index];

                if (((byte)i & 1) == 0)
                {
                    index++;
                    counter++; // counter
                }
                else
                {
                    if ((k2 & 0x80) != 0)
                    {
                        index++; // add pointer
                        counter++; // counter

                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            long k3 = k - 0xB9;
                            if (k == 0xFF)
                                break; // exit

                            while (k3-- >= 0)
                            {
                                k2 = bufSrc[index];
                                index++; // add pointer
                                counter++; // counter
                            }

                            i >>= 1; // i SHR 1
                            continue;
                        }
                    }
                    else
                    {
                        byte j = bufSrc[index + 1];
                        index += 2; // add pointer by 2
                        counter += 2; // counter
                    }
                }

                i >>= 1;
            }

            return counter;
        }

    }
}
