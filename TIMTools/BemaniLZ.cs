using System.Runtime.InteropServices;

namespace TIMTools
{
    public class BemaniLZ
    {
        private const int HASHTAB = 4096;
        private const int N = 1024;
        private const int F = 34;
        //private const int F = 34;
        private const int THRESHOLD = 4;

        private byte[] ring_buff = new byte[N + F];
        private int[] next = new int[N + 1 + HASHTAB];
        private int[] prev = new int[N + 1];

        //public BemaniLZ()
        //{
        //    InitTree();
        //}

        public class DecompressWrapper
        {
            [DllImport("WeDecompress.dll", EntryPoint = "DeCompress")]
            public static extern bool DeCompress(ref byte[] BufDest, byte[] BufSrc);
        }

        private void InitTree()
        {
            //Array.Clear(ring_buff, 0, N + F);
            //Array.Fill(next, N);
            //Array.Fill(prev, N);

            uint i;

            for (i = 0; i < N + F; i++)
                ring_buff[i] = 0;

            for (i = 0; i < N + 1 + HASHTAB; i++)
                next[i] = N;

            for (i = 0; i < N + 1; i++)
                prev[i] = N;
        }

        private void InsertNode(int r)
        {
            int next_r, c;

            c = (ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff;
            next_r = next[c + N + 1];
            next[c + N + 1] = r;
            prev[r] = c + N + 1;
            next[r] = next_r;
            if (next_r != N)
                prev[next_r] = r;
        }

        private void DeleteNode(int r)
        {
            if (prev[r] == N)
                return;
            next[prev[r]] = next[r];
            prev[next[r]] = prev[r];
            prev[r] = next[r] = N;
        }

        private void LocateNode(int r, out int match_len, out int match_pos)
        {
            int p, c, i;

            match_len = 0;
            match_pos = 0;
            c = (ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff;

            p = next[c + N + 1];
            i = 0;

            while (p != N)
            {
                for (i = 0; (i < F) && (ring_buff[p + i] == ring_buff[r + i]); i++)
                    if (i > match_len)
                    {
                        match_len = i;
                        match_pos = (r - p) & (N - 1);
                    }

                if (i == F)
                    break;

                p = next[p];
            }

            if (i == F)
                DeleteNode(p);
        }

        /*
        public byte[] Compress(byte[] BufSrc)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(outStream))
                {
                    int r, match_pos, match_len, maxlen, code_buf_ptr;
                    ulong textsize;
                    byte[] code_buf = new byte[17];
                    byte mask, c;
                    int block, index;

                    InitTree();

                    r = 0;
                    textsize = 0;
                    code_buf[0] = 0;
                    code_buf_ptr = 1;
                    mask = 1;

                    index = 0;

                    while (true)
                    {
                        if (textsize + F < (ulong)BufSrc.Length)
                            block = F;
                        else
                            block = BufSrc.Length - (int)textsize;

                        Array.Copy(BufSrc, index, ring_buff, 0, block);
                        Array.Copy(BufSrc, 0, ring_buff, N, block);
                        maxlen = block;
                        textsize += (ulong)block;

                        index += block;
                        if (index >= BufSrc.Length)
                            break;

                        while (maxlen > 0)
                        {
                            LocateNode(r, out match_len, out match_pos);

                            if (match_len > maxlen)
                                match_len = maxlen;

                            if ((match_len < THRESHOLD - 1) || (match_len < THRESHOLD && (match_pos > 16)))
                            {
                                match_len = 1;
                                code_buf[code_buf_ptr++] = ring_buff[r];
                            }
                            else if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17))
                            {
                                code_buf[0] |= mask;
                                code_buf[code_buf_ptr++] = (byte)(((match_len + 7 - 1) << 4) | (match_pos - 1));
                            }
                            else
                            {
                                code_buf[0] |= mask;
                                code_buf[code_buf_ptr++] = (byte)(((match_len - 2 - 1) << 2) | (match_pos >> 8));
                                code_buf[code_buf_ptr++] = (byte)(match_pos & 0xFF);
                            }

                            if ((mask <<= 1) == 0)
                            {
                                writer.Write(code_buf, 0, code_buf_ptr);
                                code_buf[0] = 0;
                                code_buf_ptr = 1;
                                mask = 1;
                            }

                            while (match_len > 0)
                            {
                                DeleteNode((r + F) & (N - 1));
                                maxlen--;
                                if (textsize < (ulong)BufSrc.Length)
                                {
                                    c = BufSrc[index++];
                                    ring_buff[(r + F) & (N - 1)] = c;
                                    if (r + F >= N)
                                        ring_buff[r + F] = c;
                                    textsize++;
                                    maxlen++;
                                }

                                InsertNode(r);
                                r = (r + 1) & (N - 1);
                                match_len--;
                            }
                        }
                    }

                    if (code_buf_ptr > 1)
                    {
                        code_buf[0] |= mask;
                        code_buf[code_buf_ptr++] = 0xFF;
                        code_buf[code_buf_ptr++] = 0x00;
                        writer.Write(code_buf, 0, code_buf_ptr);
                    }
                    else
                    {
                        writer.Write((byte)0x01);
                        writer.Write((byte)0xFF);
                        writer.Write((byte)0x00);
                    }

                    writer.Flush();
                    return outStream.ToArray();
                }
            }
        }
        */
        //public byte[] Compress(byte[] BufSrc)
        //{
        //    using (MemoryStream outStream = new MemoryStream())
        //    using (BinaryWriter writer = new BinaryWriter(outStream))
        //    {
        //        InitTree();

        //        int r = 0;
        //        ulong textsize = 0;
        //        ulong codesize = 0;
        //        byte[] code_buf = new byte[17];
        //        byte mask = 1;
        //        int code_buf_ptr = 1; // Inicialización de code_buf_ptr
        //        int index = 0;
        //        int block = (BufSrc.Length < F) ? BufSrc.Length : F;

        //        Array.Copy(BufSrc, ring_buff, block);
        //        Array.Copy(BufSrc, 0, ring_buff, N, block);
        //        textsize += (ulong)block;

        //        while (block > 0)
        //        {
        //            LocateNode(r, out int match_len, out int match_pos);

        //            if (match_len > block)
        //                match_len = block;

        //            if ((match_len < THRESHOLD - 1) || (match_len < THRESHOLD && (match_pos > 16)))
        //            {
        //                match_len = 1;
        //                code_buf[0] |= mask;
        //                code_buf[code_buf_ptr++] = ring_buff[r];
        //            }
        //            else if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17))
        //            {
        //                code_buf[0] |= mask;
        //                code_buf[code_buf_ptr++] = (byte)(((match_len + 7 - 1) << 4) | (match_pos - 1));
        //            }
        //            else
        //            {
        //                code_buf[0] |= mask;
        //                code_buf[code_buf_ptr++] = (byte)(((match_len - 2 - 1) << 2) | (match_pos >> 8));
        //                code_buf[code_buf_ptr++] = (byte)(match_pos & 0xFF);
        //            }

        //            if ((mask <<= 1) == 0)
        //            {
        //                writer.Write(code_buf, 0, code_buf_ptr);
        //                codesize += (ulong)code_buf_ptr;
        //                code_buf[0] = 0;
        //                code_buf_ptr = 1;
        //                mask = 1;
        //            }

        //            while (match_len > 0)
        //            {
        //                DeleteNode((r + F) & (N - 1));
        //                block--;
        //                if (textsize < (ulong)BufSrc.Length)
        //                {
        //                    byte c = BufSrc[index++];
        //                    ring_buff[(r + F) & (N - 1)] = c;
        //                    if (r + F >= N)
        //                        ring_buff[r + F] = c;
        //                    textsize++;
        //                    block++;
        //                }

        //                InsertNode(r);
        //                r = (r + 1) & (N - 1);
        //                match_len--;
        //            }
        //        }

        //        if (code_buf_ptr > 1)
        //        {
        //            code_buf[0] |= mask;
        //            code_buf[code_buf_ptr++] = 0xFF;
        //            code_buf[code_buf_ptr++] = 0x00;
        //            writer.Write(code_buf, 0, code_buf_ptr);
        //            codesize += (ulong)code_buf_ptr;
        //        }
        //        else
        //        {
        //            writer.Write((byte)0x01);
        //            writer.Write((byte)0xFF);
        //            writer.Write((byte)0x00);
        //            codesize += 3;
        //        }

        //        writer.Flush();
        //        return outStream.ToArray();
        //    }
        //}


        public byte[] Compress(byte[] BufSrc)
        {
            MemoryStream outStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outStream);

            int r, match_pos, match_len, maxlen, code_buf_ptr;
            ulong textsize, codesize;
            byte[] code_buf = new byte[17];
            byte mask, c;
            int block;

            InitTree();

            r = 0;
            textsize = 0;
            codesize = 0;
            code_buf[0] = 0;
            code_buf_ptr = 1;
            mask = 1;

            if (textsize + F < (ulong)BufSrc.Length)
                block = F;
            else
                block = BufSrc.Length - (int)textsize;

            Array.Copy(BufSrc, ring_buff, block);
            Array.Copy(BufSrc, 0, ring_buff, N, block);
            maxlen = block;
            textsize += (ulong)block;

            int index = block;

            while (maxlen > 0)
            {
                LocateNode(r, out match_len, out match_pos);

                if (match_len > maxlen)
                    match_len = maxlen;

                if ((match_len < THRESHOLD - 1) || (match_len < THRESHOLD && (match_pos > 16)))
                {
                    match_len = 1;
                    code_buf[code_buf_ptr++] = ring_buff[r];
                }
                else if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17))
                {
                    code_buf[0] |= mask;
                    code_buf[code_buf_ptr++] = (byte)(((match_len + 7 - 1) << 4) | (match_pos - 1));
                }
                else
                {
                    code_buf[0] |= mask;
                    code_buf[code_buf_ptr++] = (byte)(((match_len - 2 - 1) << 2) | (match_pos >> 8));
                    code_buf[code_buf_ptr++] = (byte)(match_pos & 0xFF);
                }

                if ((mask <<= 1) == 0)
                {
                    writer.Write(code_buf, 0, code_buf_ptr);
                    codesize += (ulong)code_buf_ptr;
                    code_buf[0] = 0;
                    code_buf_ptr = 1;
                    mask = 1;
                }

                while (match_len > 0)
                {
                    DeleteNode((r + F) & (N - 1));
                    maxlen--;
                    if (textsize < (ulong)BufSrc.Length)
                    {
                        c = BufSrc[index++];
                        ring_buff[(r + F) & (N - 1)] = c;
                        if (r + F >= N)
                            ring_buff[r + F] = c;
                        textsize++;
                        maxlen++;
                    }

                    InsertNode(r);
                    r = (r + 1) & (N - 1);
                    match_len--;
                }
            }

            if (code_buf_ptr > 1)
            {
                code_buf[0] |= mask;
                code_buf[code_buf_ptr++] = 0xFF;
                code_buf[code_buf_ptr++] = 0x00;
                writer.Write(code_buf, 0, code_buf_ptr);
                codesize += (ulong)code_buf_ptr;
            }
            else
            {
                writer.Write((byte)0x01);
                writer.Write((byte)0xFF);
                writer.Write((byte)0x00);
                codesize += 3;
            }

            writer.Flush();
            byte[] compressedData = outStream.ToArray();

            return compressedData;
        }
        /*
                public bool Decompress(byte[] BufSrc, out byte[] BufDest)
                {
                    int srcIndex = 0;
                    int destIndex = 0;
                    BufDest = new byte[65536]; // Tamaño arbitrario, ajusta según sea necesario

                    long k3;
                    uint i = 0, j;
                    byte k, k2;

                    while (true)
                    {
                        if ((i & 0x100) == 0) // (i < 0x100) or (0x1000 < i < 0x10FF)
                        {
                            k = BufSrc[srcIndex]; // Get data and do data % FF
                            srcIndex++; // add pointer
                            i = (uint)(k | 0xFF00); // i = 0xFF unido a k
                        }

                        k2 = BufSrc[srcIndex]; // get data

                        if ((i & 1) == 0) // t = 0 si es par, t = 1 si es impar // Literal
                        {
                            BufDest[destIndex] = k2; // res = k2
                            destIndex++; // add pointer
                            srcIndex++;
                        }
                        else
                        {
                            if ((k2 & 0x80) != 0) // k2 & 0x80 != 0 Solo si k2 > 0x80 el control pasa
                            {                      // Caso en el que tenemos 1 solo byte para el comando,
                                                   // nbit move = 4, nbitrepeat = 4
                                srcIndex++; // add pointer

                                if ((k2 & 0x40) != 0) // k2 & 0x40 != 0 Solo si k2 > 0xC0 el control pasa
                                {
                                    k = k2;
                                    k3 = k - 0xB9;
                                    if (k == 0xFF)
                                        break; // salir del ciclo

                                    while (k3-- >= 0) // seguramente k3 > 0
                                    {
                                        k2 = BufSrc[srcIndex]; // get data
                                        srcIndex++; // add pointer
                                        BufDest[destIndex] = k2; // write k2 on ptrRes
                                        destIndex++; // add pointer
                                    }

                                    i >>= 1; // i SHR 1
                                    continue;
                                }

                                j = (uint)((k2 & 0x0F) + 1); // k2 & 0x0F = tomo los últimos 4 bits y sumo 1
                                k3 = (k2 >> 4) - 7; // k2 >> 4 = quito los últimos 4 bits, y quito 7
                            }
                            else
                            {
                                j = (uint)(BufSrc[srcIndex + 1]); // get data (toma el siguiente byte)
                                srcIndex += 2; // add pointer by 2
                                k3 = (k2 >> 2) + 2; // k3 = quita últimos 2 bits + 2
                                j |= (uint)((k2 & 3) << 8); // j = j | (k2 & 3) * 256
                            }

                            // ciclo para repetir k3 veces + 1 el byte a partir de la posición -j
                            for (; k3 >= 0; k3--) // loop until k3 >= 0
                            {
                                BufDest[destIndex] = BufDest[destIndex - (int)j]; // write data from first (far j bytes) for k3 times
                                destIndex++;
                            }
                        }

                        i >>= 1;
                    }

                    return true;
                }
        */
        public  bool Decompress(byte[] BufSrc, out byte[] BufDest, out int decompressedSize)
        {
            int srcIndex = 0;
            int destIndex = 0;
            BufDest = new byte[65536]; // Tamaño arbitrario, ajusta según sea necesario

            long k3;
            uint i = 0, j;
            byte k, k2;

            while (true)
            {
                if ((i & 0x100) == 0) // (i < 0x100) or (0x1000 < i < 0x10FF)
                {
                    k = BufSrc[srcIndex]; // Get data and do data % FF
                    srcIndex++; // add pointer
                    i = (uint)(k | 0xFF00); // i = 0xFF unido a k
                }

                k2 = BufSrc[srcIndex]; // get data

                if ((i & 1) == 0) // t = 0 si es par, t = 1 si es impar // Literal
                {
                    BufDest[destIndex] = k2; // res = k2
                    destIndex++; // add pointer
                    srcIndex++;
                }
                else
                {
                    if ((k2 & 0x80) != 0) // k2 & 0x80 != 0 Solo si k2 > 0x80 el control pasa
                    {                      // Caso en el que tenemos 1 solo byte para el comando,
                                           // nbit move = 4, nbitrepeat = 4
                        srcIndex++; // add pointer

                        if ((k2 & 0x40) != 0) // k2 & 0x40 != 0 Solo si k2 > 0xC0 el control pasa
                        {
                            k = k2;
                            k3 = k - 0xB9;
                            if (k == 0xFF)
                                break; // salir del ciclo

                            while (k3-- >= 0) // seguramente k3 > 0
                            {
                                k2 = BufSrc[srcIndex]; // get data
                                srcIndex++; // add pointer
                                BufDest[destIndex] = k2; // write k2 on ptrRes
                                destIndex++; // add pointer
                            }

                            i >>= 1; // i SHR 1
                            continue;
                        }

                        j = (uint)((k2 & 0x0F) + 1); // k2 & 0x0F = tomo los últimos 4 bits y sumo 1
                        k3 = (k2 >> 4) - 7; // k2 >> 4 = quito los últimos 4 bits, y quito 7
                    }
                    else
                    {
                        j = (uint)(BufSrc[srcIndex + 1]); // get data (toma el siguiente byte)
                        srcIndex += 2; // add pointer by 2
                        k3 = (k2 >> 2) + 2; // k3 = quita últimos 2 bits + 2
                        j |= (uint)((k2 & 3) << 8); // j = j | (k2 & 3) * 256
                    }

                    // ciclo para repetir k3 veces + 1 el byte a partir de la posición -j
                    for (; k3 >= 0; k3--) // loop until k3 >= 0
                    {
                        BufDest[destIndex] = BufDest[destIndex - (int)j]; // write data from first (far j bytes) for k3 times
                        destIndex++;
                    }
                }
                i >>= 1;
            }

            decompressedSize = destIndex; // Asigna el tamaño descomprimido real
            Array.Resize(ref BufDest, decompressedSize);
            return true;
        }

        private long ReadCompressedLength(FileStream file)
        {
            byte[] lengthBytes = new byte[4]; // Suponiendo que la longitud es un entero de 4 bytes
            int bytesRead = file.Read(lengthBytes, 0, lengthBytes.Length);

            if (bytesRead != lengthBytes.Length)
            {
                throw new Exception("Error al leer la longitud comprimida");
            }

            // Convertir bytes a entero en formato little-endian
            long length = BitConverter.ToInt32(lengthBytes, 0);
            return length;
        }

        /* public int FindCompressedLength(byte[] BufSrc)
         {
             int k3, counter;
             uint i, j;
             byte k, k2;

             i = 0;
             counter = 0;

             while (true)
             {
                 if ((i & 0x100) == 0)
                 {
                     k = BufSrc[i++];
                     counter++;
                     if (i >= BufSrc.Length)
                         break;
                 }

                 if ((BufSrc[i] == 0) && (BufSrc[i + 1] == 0) && (BufSrc[i + 2] == 0) && (BufSrc[i + 3] == 0))
                     return 0;

                 k2 = BufSrc[i++];

                 if (((byte)i & 1) == 0)
                 {
                     counter++;
                 }
                 else
                 {
                     if ((k2 & 0x80) != 0)
                     {
                         counter++;

                         if ((k2 & 0x40) != 0)
                         {
                             k = k2;
                             k3 = k - 0xB9;
                             if (k == 0xFF)
                                 break;

                             while (k3-- >= 0)
                             {
                                 k2 = BufSrc[i++];
                                 counter++;
                             }

                             i = i >> 1;
                             continue;
                         }
                     }
                     else
                     {
                         j = BufSrc[i++];
                         j |= (uint)(BufSrc[i++] << 8);
                         counter += 2;
                     }
                 }

                 i = i >> 1;
             }

             return counter;
         }*/

        public long FindCompressedLength(byte[] bufSrc)
        {
            int orig = 0;
            int counter = 0;
            int k = 0;
            int k2 = 0;
            int j = 0;
            int k3 = 0;
            int i = 0;

            while (true)
            {
                if ((i & 256) == 0) // Check if i is even (equivalent to i And 256)
                {
                    k = bufSrc[orig] & 255; // Combine parsed string with 0xFF
                    orig++;
                    counter++;
                    i = k | 65280; // Combine k with 0xFF00 (equivalent to i := (k Or 65280))
                }
                 k2 = bufSrc[orig] & 255; // Parse string and cast to byte
                if ((i & 1) == 0) // Check if i is even (equivalent to i And 1)
                {
                    orig++;
                    counter++;
                }
                else
                {
                    if ((k2 & 128) != 0) // Check if k2's highest bit is set (equivalent to k2 And 128)
                    {
                        orig++;
                        counter++;

                        if ((k2 & 64) != 0) // Check if k2's 6th bit is set (equivalent to k2 And 64)
                        {
                            k = k2 & 255;
                            k3 = k - 185;

                            if (k == 255)
                            {
                                return counter;
                            }
                            do
                            {
                                k2 = bufSrc[orig] & 255;
                                orig++;
                                counter++;
                                k3--;
                            } while (k3 >= 0);
                            i >>= 1;
                            continue;
                        }

                        j = (k2 & 15) + 1; // Get the lower 4 bits of k2 and add 1 (equivalent to j := (k2 And 15) + 1)
                        k3 = (k2 >> 4) - 7; // Shift k2 right by 4 bits and subtract 7 (equivalent to k3 := (k2 shr 4) - 7)
                    }
                    else
                    {
                        j = bufSrc[orig + 1] & 255;
                        orig += 2;
                        counter += 2;
                        k3 = (k2 >> 2) + 2; // Shift k2 right by 2 bits and add 2 (equivalent to k3 := (k2 shr 2) + 2)
                        j |= (k2 & 3) << 8; // Combine lower 2 bits of k2 with j shifted left by 8 (equivalent to j := j Or (k2 And 3) shl 8)

                        do
                        {
                            k3--;
                        } while (k3 >= 0);
                    }
                    i = i >> 1;
                }
            return counter;
            }
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


        /*          CODIGO QUE MAS O MENOS ANDABA
             * 
             * private const int HASHTAB = 4096;
            private const int N = 1024;
            private const int F = 34;
            private const int THRESHOLD = 3;

            private byte[] ring_buff = new byte[N + F];
            private int[] next = new int[N + 1 + HASHTAB];
            private int[] prev = new int[N + 1];

            public BemaniLZ()
            {
                InitTree();
            }

            private void InitTree()
            {
                for (int i = 0; i < N + F; i++)
                    ring_buff[i] = 0;

                for (int i = 0; i < N + 1 + HASHTAB; i++)
                    next[i] = N;

                for (int i = 0; i < N + 1; i++)
                    prev[i] = N;
            }

            private void InsertNode(int r)
            {
                int next_r, c;

                c = (ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff;
                next_r = next[c + N + 1];
                next[c + N + 1] = r;
                prev[r] = c + N + 1;
                next[r] = next_r;
                if (next_r != N)
                    prev[next_r] = r;
            }

            private void DeleteNode(int r)
            {
                if (prev[r] == N)
                    return;
                next[prev[r]] = next[r];
                prev[next[r]] = prev[r];
                prev[r] = next[r] = N;
            }

            private void LocateNode(int r, out int match_len, out int match_pos)
            {
                int p, c, i;

                match_len = 0;
                match_pos = 0;
                c = (ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff;

                p = next[c + N + 1];
                i = 0;

                while (p != N)
                {
                    for (i = 0; (i < F) && (ring_buff[p + i] == ring_buff[r + i]); i++)
                        ;

                    if (i > match_len)
                    {
                        match_len = i;
                        match_pos = (r - p) & (N - 1);
                    }

                    if (i == F)
                        break;

                    p = next[p];
                }

                if (i == F)
                    DeleteNode(p);
            }

            public byte[] Compress(byte[] BufSrc)
            {
                MemoryStream outStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(outStream);

                int r, match_pos, match_len, maxlen, code_buf_ptr;
                ulong textsize, codesize;
                byte[] code_buf = new byte[17];
                byte mask, c;
                int block;

                InitTree();

                r = 0;
                textsize = 0;
                codesize = 0;
                code_buf[0] = 0;
                code_buf_ptr = 1;
                mask = 1;

                if (textsize + F < (ulong)BufSrc.Length)
                    block = F;
                else
                    block = BufSrc.Length - (int)textsize;

                Array.Copy(BufSrc, ring_buff, block);
                Array.Copy(BufSrc, 0, ring_buff, N, block);
                maxlen = block;
                textsize += (ulong)block;

                int index = block;

                while (maxlen > 0)
                {
                    LocateNode(r, out match_len, out match_pos);

                    if (match_len > maxlen)
                        match_len = maxlen;

                    if ((match_len < THRESHOLD - 1) || (match_len < THRESHOLD && (match_pos > 16)))
                    {
                        match_len = 1;
                        code_buf[code_buf_ptr++] = ring_buff[r];
                    }
                    else if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17))
                    {
                        code_buf[0] |= mask;
                        code_buf[code_buf_ptr++] = (byte)(((match_len + 7 - 1) << 4) | (match_pos - 1));
                    }
                    else
                    {
                        code_buf[0] |= mask;
                        code_buf[code_buf_ptr++] = (byte)(((match_len - 2 - 1) << 2) | (match_pos >> 8));
                        code_buf[code_buf_ptr++] = (byte)(match_pos & 0xFF);
                    }

                    if ((mask <<= 1) == 0)
                    {
                        writer.Write(code_buf, 0, code_buf_ptr);
                        codesize += (ulong)code_buf_ptr;
                        code_buf[0] = 0;
                        code_buf_ptr = 1;
                        mask = 1;
                    }

                    while (match_len > 0)
                    {
                        DeleteNode((r + F) & (N - 1));
                        maxlen--;
                        if (textsize < (ulong)BufSrc.Length)
                        {
                            c = BufSrc[index++];
                            ring_buff[(r + F) & (N - 1)] = c;
                            if (r + F >= N)
                                ring_buff[r + F] = c;
                            textsize++;
                            maxlen++;
                        }

                        InsertNode(r);
                        r = (r + 1) & (N - 1);
                        match_len--;
                    }
                }

                if (code_buf_ptr > 1)
                {
                    code_buf[0] |= mask;
                    code_buf[code_buf_ptr++] = 0xFF;
                    code_buf[code_buf_ptr++] = 0x00;
                    writer.Write(code_buf, 0, code_buf_ptr);
                    codesize += (ulong)code_buf_ptr;
                }
                else
                {
                    writer.Write((byte)0x01);
                    writer.Write((byte)0xFF);
                    writer.Write((byte)0x00);
                    codesize += 3;
                }

                writer.Flush();
                byte[] compressedData = outStream.ToArray();

                return compressedData;
            }

            public byte[] DeCompressLOG(byte[] BufSrc, int uncompressedLength)
            {
                MemoryStream outStream = new MemoryStream(uncompressedLength);
                int k3;
                uint i, j;
                byte k, k2;

                i = 0;

                while (true)
                {
                    if ((i & 0x100) == 0)
                    {
                        k = BufSrc[i++];
                        if (i >= BufSrc.Length)
                            break;
                    }

                    k2 = BufSrc[i++];

                    if (((byte)i & 1) == 0)
                    {
                        outStream.WriteByte(k2);
                    }
                    else
                    {
                        if ((k2 & 0x80) != 0)
                        {
                            if ((k2 & 0x40) != 0)
                            {
                                k = k2;
                                k3 = k - 0xB9;
                                if (k == 0xFF)
                                    break;

                                while (k3-- >= 0)
                                {
                                    if (i >= BufSrc.Length)
                                        break;

                                    k2 = BufSrc[i++];
                                    outStream.WriteByte(k2);
                                }

                                i = i >> 1;
                                continue;
                            }

                            j = (uint)(k2 & 0x0F) + 1;
                            k3 = (k2 >> 4) - 7;
                        }
                        else
                        {
                            j = BufSrc[i++];
                            j |= (uint)(BufSrc[i++] << 8);
                            k3 = (k2 >> 2) + 2;
                        }

                        // Debugging: Print out compression details
                        Console.WriteLine($"Compressed: k2={k2}, j={j}, k3={k3}");

                        for (; k3 >= 0; k3--)
                        {
                            if (j >= BufSrc.Length)
                            {
                                Console.WriteLine($"Out of bounds: j={j}, BufSrc.Length={BufSrc.Length}");
                                break;
                            }

                            byte b = BufSrc[j++];
                            outStream.WriteByte(b);
                        }
                    }

                    i = i >> 1;
                }

                // Convert MemoryStream to byte array
                byte[] decompressedData = outStream.ToArray();

                // Debugging: Print out the final decompressed data length
                Console.WriteLine($"Decompressed data length: {decompressedData.Length}");

                return decompressedData;
            }

            public byte[] DeCompress(byte[] BufSrc, int uncompressedLength)
            {
                MemoryStream outStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(outStream);

                int k3;
                uint i, j;
                byte k, k2;

                i = 0;

                while (true)
                {
                    if ((i & 0x100) == 0)
                    {
                        k = BufSrc[i++];
                        if (i >= BufSrc.Length)
                            break;
                    }

                    k2 = BufSrc[i++];

                    if (((byte)i & 1) == 0)
                    {
                        writer.Write(k2);
                    }
                    else
                    {
                        if ((k2 & 0x80) != 0)
                        {
                            if ((k2 & 0x40) != 0)
                            {
                                k = k2;
                                k3 = k - 0xB9;
                                if (k == 0xFF)
                                    break;

                                while (k3-- >= 0)
                                {
                                    k2 = BufSrc[i++];
                                    writer.Write(k2);
                                }

                                i = i >> 1;
                                continue;
                            }

                            j = (uint)(k2 & 0x0F) + 1;
                            k3 = (k2 >> 4) - 7;
                        }
                        else
                        {
                            j = BufSrc[i++];
                            j |= (uint)(BufSrc[i++] << 8);
                            k3 = (k2 >> 2) + 2;
                        }

                        for (; k3 >= 0; k3--)
                        {
                            writer.Write(BufSrc[j]);
                        }
                    }

                    i = i >> 1;
                }

                writer.Flush();
                byte[] decompressedData = outStream.ToArray();

                return decompressedData;
            }

            public int FindCompressedLength(byte[] BufSrc)
            {
                int k3, counter;
                uint i, j;
                byte k, k2;

                i = 0;
                counter = 0;

                while (true)
                {
                    if ((i & 0x100) == 0)
                    {
                        k = BufSrc[i++];
                        counter++;
                        if (i >= BufSrc.Length)
                            break;
                    }

                    if ((BufSrc[i] == 0) && (BufSrc[i + 1] == 0) && (BufSrc[i + 2] == 0) && (BufSrc[i + 3] == 0))
                        return 0;

                    k2 = BufSrc[i++];

                    if (((byte)i & 1) == 0)
                    {
                        counter++;
                    }
                    else
                    {
                        if ((k2 & 0x80) != 0)
                        {
                            counter++;

                            if ((k2 & 0x40) != 0)
                            {
                                k = k2;
                                k3 = k - 0xB9;
                                if (k == 0xFF)
                                    break;

                                while (k3-- >= 0)
                                {
                                    k2 = BufSrc[i++];
                                    counter++;
                                }

                                i = i >> 1;
                                continue;
                            }
                        }
                        else
                        {
                            j = BufSrc[i++];
                            j |= (uint)(BufSrc[i++] << 8);
                            counter += 2;
                        }
                    }

                    i = i >> 1;
                }

                return counter;
            }*/
    }
}