using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIMTools
{
    public class LZ77Compressor
    {
        private const int N = 1024;  // Tamaño del buffer circular
        private const int F = 34;    // Tamaño de la ventana de búsqueda
        private const int THRESHOLD = 3; // Umbral mínimo para la longitud de la coincidencia
        private const int HASHTAB = 4096; // Tamaño de la tabla hash

        private byte[] ringBuff;
        private uint[] next;
        private uint[] prev;

        public LZ77Compressor()
        {
            ringBuff = new byte[N + F];
            next = new uint[N + 1 + HASHTAB];
            prev = new uint[N + 1];
        }

        //private void InitTree()
        //{
        //    // Inicializar el buffer circular y las tablas hash
        //    for (int i = 0; i < N + F; i++)
        //        ringBuff[i] = 0;

        //    for (int i = 0; i < N + 1 + HASHTAB; i++)
        //        next[i] = N;

        //    for (int i = 0; i < N + 1; i++)
        //        prev[i] = N;
        //}

        //private void InsertNode(uint r)
        //{
        //    uint nextR, c;

        //    c = (uint)((ringBuff[r] + (ringBuff[r + 1] << 8)) & 0xfff); // hash func
        //    nextR = next[c + N + 1];
        //    next[c + N + 1] = r;
        //    prev[r] = c + N + 1;
        //    next[r] = nextR;
        //    if (nextR != N)
        //        prev[nextR] = r;
        //}

        //private void DeleteNode(uint r)
        //{
        //    if (prev[r] == N)
        //        return;
        //    next[prev[r]] = next[r];
        //    if (next[r] != N)
        //        prev[next[r]] = prev[r];
        //    prev[r] = next[r] = N;
        //}

        //private void LocateNode(uint r, out uint matchLen, out uint matchPos)
        //{
        //    uint p, c, i;

        //    matchLen = 0;
        //    matchPos = 0;
        //    c = (uint)((ringBuff[r] + (ringBuff[r + 1] << 8)) & 0xfff); // hash func

        //    p = next[c + N + 1];
        //    i = 0;

        //    while (p != N)
        //    {
        //        for (i = 0; (i < F) && (ringBuff[p + i] == ringBuff[r + i]); i++) ;

        //        if (i > matchLen)
        //        {
        //            matchLen = i;
        //            matchPos = (r - p) & (N - 1);
        //        }

        //        if (i == F)
        //            break;

        //        p = next[p];
        //    }

        //    if (i == F)
        //        DeleteNode(p);
        //}











        //private const int N = 4096;  // Tamaño del buffer circular
        //private const int F = 18;    // Tamaño de la ventana de búsqueda
        //private const int THRESHOLD = 2; // Umbral mínimo para la longitud de la coincidencia
        //private const int HASHTAB = 4096; // Tamaño de la tabla hash

        //private byte[] ringBuff;
        //private uint[] next;
        //private uint[] prev;

        //public LZ77Compressor()
        //{
        //    ringBuff = new byte[N + F];
        //    next = new uint[N + 1 + HASHTAB];
        //    prev = new uint[N + 1];
        //}

        private void InitTree()
        {
            // Inicializar el buffer circular y las tablas hash
            for (int i = 0; i < N + F; i++)
                ringBuff[i] = 0;

            for (int i = 0; i < N + 1 + HASHTAB; i++)
                next[i] = N;

            for (int i = 0; i < N + 1; i++)
                prev[i] = N;
        }

        private void InsertNode(uint r)
        {
            uint nextR, c;

            c = (uint)((ringBuff[r] + (ringBuff[r + 1] << 8)) & 0xfff); // Función hash
            nextR = next[c + N + 1];
            next[c + N + 1] = r;
            prev[r] = c + N + 1;
            next[r] = nextR;
            if (nextR != N)
                prev[nextR] = r;
        }

        private void DeleteNode(uint r)
        {
            if (prev[r] == N)
                return;
            next[prev[r]] = next[r];
            if (next[r] != N)
                prev[next[r]] = prev[r];
            prev[r] = next[r] = N;
        }

        private void LocateNode(uint r, out uint matchLen, out uint matchPos)
        {
            uint p, c, i;

            matchLen = 0;
            matchPos = 0;
            c = (uint)((ringBuff[r] + (ringBuff[r + 1] << 8)) & 0xfff); // Función hash

            p = next[c + N + 1];
            i = 0;

            while (p != N)
            {
                for (i = 0; (i < F) && (ringBuff[p + i] == ringBuff[r + i]); i++) ;

                if (i > matchLen)
                {
                    matchLen = i;
                    matchPos = (r - p) & (N - 1);
                }

                if (i == F)
                    break;

                p = next[p];
            }

            if (i == F)
                DeleteNode(p);
        }

        public bool Compress(ref List<byte> bufDest, byte[] bufSrc, ref ulong sizeResult, ulong sizeSrc)
        {
            bufDest.Clear(); // Limpiar lista de destino antes de usarla

            ulong textsize, codesize;
            byte[] codeBuf = new byte[17];
            byte mask, c;
            uint r, matchPos, matchLen, maxlen, codeBufPtr;
            uint block;

            InitTree(); // Inicializar árboles

            r = 0;
            textsize = codesize = 0;
            codeBuf[0] = 0;
            codeBufPtr = 1;
            mask = 1;

            if (textsize + F < sizeSrc)
                block = F;
            else
                block = (uint)(sizeSrc - textsize);

            Array.Copy(bufSrc, 0, ringBuff, 0, block);
            Array.Copy(bufSrc, 0, ringBuff, N, (int)block);
            maxlen = block;
            textsize += block;
            bufSrc = bufSrc.Skip((int)block).ToArray();

            while (maxlen > 0)
            {
                LocateNode(r, out matchLen, out matchPos);
                if (matchLen > maxlen)
                    matchLen = maxlen;
                if ((matchLen < THRESHOLD - 1) || ((matchLen < THRESHOLD) && (matchPos > 16)))
                {
                    matchLen = 1; // No hay suficiente coincidencia. Envía un byte sin comprimir.
                    codeBuf[codeBufPtr++] = ringBuff[r]; // Envía sin comprimir.
                }
                else if ((matchLen > 2 - 1) && (matchLen < 6) && (matchPos < 17))
                {
                    codeBuf[0] |= mask; // Bandera 'enviar un byte'
                    codeBuf[codeBufPtr++] = (byte)(((matchLen + 7 - 1) << 4) | (matchPos - 1));
                }
                else
                {
                    codeBuf[0] |= mask; // Bandera 'enviar un byte'
                    codeBuf[codeBufPtr++] = (byte)(((matchLen - 2 - 1) << 2) | (matchPos >> 8));
                    codeBuf[codeBufPtr++] = (byte)(matchPos & 0xFF);
                }

                if ((mask <<= 1) == 0)
                {
                    bufDest.AddRange(codeBuf.Take((int)codeBufPtr)); // Agregar hasta 8 unidades de código juntas
                    codesize += codeBufPtr;
                    codeBuf[0] = 0;
                    codeBufPtr = 1;
                    mask = 1;
                }

                while (matchLen-- > 0)
                {
                    DeleteNode((r + F) & (N - 1));
                    maxlen--;
                    if (textsize < sizeSrc)
                    {
                        c = bufSrc[0];
                        bufSrc = bufSrc.Skip(1).ToArray();
                        ringBuff[(r + F) & (N - 1)] = c;
                        if (r + F >= N)
                            ringBuff[r + F] = c;
                        textsize++;
                        maxlen++;
                    }

                    InsertNode(r);
                    r = (r + 1) & (N - 1);
                }
            }

            if (codeBufPtr > 1)
            {
                codeBuf[0] |= mask; // Bandera 'enviar un byte'
                codeBuf[codeBufPtr++] = 0xFF;
                codeBuf[codeBufPtr++] = 0x00;
                bufDest.AddRange(codeBuf.Take((int)codeBufPtr));
                codesize += codeBufPtr;
            }
            else
            {
                bufDest.Add(0x01);
                bufDest.Add(0xFF);
                bufDest.Add(0x00);
            }

            sizeResult = codesize;

            return true;
        }







        //public bool Compress(ref List<byte> bufDest, byte[] bufSrc, ref ulong sizeResult, ulong sizeSrc)
        //{
        //    bufDest.Clear(); // Limpiar lista de destino antes de usarla

        //    ulong textsize, codesize;
        //    byte[] codeBuf = new byte[17];
        //    byte mask, c;
        //    uint r, matchPos, matchLen, maxlen, codeBufPtr;
        //    uint block;

        //    InitTree(); // Inicializar árboles

        //    r = 0;
        //    textsize = codesize = 0;
        //    codeBuf[0] = 0;
        //    codeBufPtr = 1;
        //    mask = 1;

        //    if (textsize + F < sizeSrc)
        //        block = F;
        //    else
        //        block = (uint)(sizeSrc - textsize);

        //    Array.Copy(bufSrc, 0, ringBuff, 0, block);
        //    Array.Copy(bufSrc, 0, ringBuff, N, (int)block);
        //    maxlen = block;
        //    textsize += block;
        //    bufSrc = bufSrc.Skip((int)block).ToArray();

        //    while (maxlen > 0)
        //    {
        //        LocateNode(r, out matchLen, out matchPos);
        //        if (matchLen > maxlen)
        //            matchLen = maxlen;
        //        if ((matchLen < THRESHOLD - 1) || ((matchLen < THRESHOLD) && (matchPos > 16)))
        //        {
        //            matchLen = 1; // No hay suficiente coincidencia. Envía un byte sin comprimir.
        //            codeBuf[codeBufPtr++] = ringBuff[r]; // Envía sin comprimir.
        //        }
        //        else if ((matchLen > 2 - 1) && (matchLen < 6) && (matchPos < 17))
        //        {
        //            codeBuf[0] |= mask; // Bandera 'enviar un byte'
        //            codeBuf[codeBufPtr++] = (byte)(((matchLen + 7 - 1) << 4) | (matchPos - 1));
        //        }
        //        else
        //        {
        //            codeBuf[0] |= mask; // Bandera 'enviar un byte'
        //            codeBuf[codeBufPtr++] = (byte)(((matchLen - 2 - 1) << 2) | (matchPos >> 8));
        //            codeBuf[codeBufPtr++] = (byte)(matchPos & 0xFF);
        //        }

        //        if ((mask <<= 1) == 0)
        //        {
        //            bufDest.AddRange(codeBuf.Take((int)codeBufPtr)); // Agregar hasta 8 unidades de código juntas
        //            codesize += codeBufPtr;
        //            codeBuf[0] = 0;
        //            codeBufPtr = 1;
        //            mask = 1;
        //        }

        //        while (matchLen-- > 0)
        //        {
        //            DeleteNode((r + F) & (N - 1));
        //            maxlen--;
        //            if (textsize < sizeSrc)
        //            {
        //                c = bufSrc[0];
        //                bufSrc = bufSrc.Skip(1).ToArray();
        //                ringBuff[(r + F) & (N - 1)] = c;
        //                if (r + F >= N)
        //                    ringBuff[r + F] = c;
        //                textsize++;
        //                maxlen++;
        //            }

        //            InsertNode(r);
        //            r = (r + 1) & (N - 1);
        //        }
        //    }

        //    if (codeBufPtr > 1)
        //    {
        //        codeBuf[0] |= mask; // Bandera 'enviar un byte'
        //        codeBuf[codeBufPtr++] = 0xFF;
        //        codeBuf[codeBufPtr++] = 0x00;
        //        bufDest.AddRange(codeBuf.Take((int)codeBufPtr));
        //        codesize += codeBufPtr;
        //    }
        //    else
        //    {
        //        bufDest.Add(0x01);
        //        bufDest.Add(0xFF);
        //        bufDest.Add(0x00);
        //    }

        //    sizeResult = codesize;

        //    return true;
        //}

        public bool Compress(ref byte[] bufDest, byte[] bufSrc, ref ulong sizeResult, ulong sizeSrc)
        {
            byte[] ptrRes = bufDest;
            uint r, matchPos, matchLen, maxlen, codeBufPtr;
            ulong ps = 0, textsize, codesize;
            byte[] codeBuf = new byte[17];
            byte mask, c;
            uint block;

            InitTree(); // Inicializar árboles

            r = 0;
            textsize = codesize = 0;
            codeBuf[0] = 0;
            codeBufPtr = 1;
            mask = 1;

            if (textsize + F < sizeSrc)
                block = F;
            else
                block = (uint)(sizeSrc - textsize);

            Array.Copy(bufSrc, 0, ringBuff, 0, block);
            Array.Copy(bufSrc, 0, ringBuff, N, (int)block);
            maxlen = block;
            textsize += block;
            bufSrc = bufSrc.Skip((int)block).ToArray();

            while (maxlen > 0)
            {
                LocateNode(r, out matchLen, out matchPos);
                if (matchLen > maxlen)
                    matchLen = maxlen;
                if ((matchLen < THRESHOLD - 1) || ((matchLen < THRESHOLD) && (matchPos > 16)))
                {
                    matchLen = 1; // No hay suficiente coincidencia. Envía un byte sin comprimir.
                    codeBuf[codeBufPtr++] = ringBuff[r]; // Envía sin comprimir.
                }
                else if ((matchLen > 2 - 1) && (matchLen < 6) && (matchPos < 17))
                {
                    codeBuf[0] |= mask; // Bandera 'enviar un byte'
                    codeBuf[codeBufPtr++] = (byte)(((matchLen + 7 - 1) << 4) | (matchPos - 1));
                }
                else
                {
                    codeBuf[0] |= mask; // Bandera 'enviar un byte'
                    codeBuf[codeBufPtr++] = (byte)(((matchLen - 2 - 1) << 2) | (matchPos >> 8));
                    codeBuf[codeBufPtr++] = (byte)(matchPos & 0xFF);
                }

                if ((mask <<= 1) == 0)
                {
                    Array.Copy(codeBuf, 0, ptrRes, (int)codesize, (int)codeBufPtr); // Envía hasta 8 unidades de código juntas
                    ptrRes = ptrRes.Skip((int)codeBufPtr).ToArray();

                    codesize += codeBufPtr;
                    codeBuf[0] = 0;
                    codeBufPtr = 1;
                    mask = 1;
                }

                while (matchLen-- > 0)
                {
                    DeleteNode((r + F) & (N - 1));
                    maxlen--;
                    if (textsize < sizeSrc)
                    {
                        c = bufSrc[0];
                        bufSrc = bufSrc.Skip(1).ToArray();
                        ringBuff[(r + F) & (N - 1)] = c;
                        if (r + F >= N)
                            ringBuff[r + F] = c;
                        textsize++;
                        maxlen++;
                    }

                    InsertNode(r);
                    r = (r + 1) & (N - 1);
                }
            }

            if (codeBufPtr > 1)
            {
                codeBuf[0] |= mask; // Bandera 'enviar un byte'
                codeBuf[codeBufPtr++] = 0xFF;
                codeBuf[codeBufPtr++] = 0x00;
                Array.Copy(codeBuf, 0, ptrRes, (int)codesize, (int)codeBufPtr);
                ptrRes = ptrRes.Skip((int)codeBufPtr).ToArray();
                codesize += codeBufPtr;
            }
            else
            {
                ptrRes[0] = 0x01;
                ptrRes[1] = 0xFF;
                ptrRes[2] = 0x00;
                ptrRes = ptrRes.Skip(3).ToArray();
            }

            sizeResult = codesize;

            return true;
        }

        public void Comprimir(ref byte[] arrayDestino, string datosComprimidos, ref ulong resultSize)
        {
            byte[] buffer;
            ulong finalSize = 0;
            ulong dataSize;
            using (FileStream fs = new FileStream(datosComprimidos,FileMode.Open,FileAccess.Read))
            {
                fs.Position = 0;
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                dataSize = (ulong)fs.Length;
            }

            if (Compress(ref arrayDestino, buffer, ref finalSize, dataSize));
            {
                MessageBox.Show("Compresión realizada con éxito!\nTamaño en bytes: " + dataSize.ToString());
            }
        }

        public void Comprimir(string archivSalida,ref List<byte> arrayDestino, string datosComprimidos, ref ulong resultSize)
        {
            byte[] buffer;
            byte[] arrayTemp = null;
            ulong finalSize = 0;
            ulong dataSize;
            using (FileStream fs = new FileStream(datosComprimidos, FileMode.Open, FileAccess.Read))
            {
                fs.Position = 0;
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                dataSize = (ulong)fs.Length;
            }

            if (Compress(ref arrayDestino, buffer, ref finalSize, dataSize)) ;
            {
                arrayTemp = arrayDestino.ToArray();

                using (FileStream fs = new FileStream(archivSalida, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Position = 0;
                    fs.Write(arrayTemp, 0, arrayTemp.Length);   
                }
                if (File.Exists(archivSalida))
                {
                MessageBox.Show("Compresión realizada con éxito!\nTamaño en bytes: " + dataSize.ToString());
                }
            }
        }

    }

}
