using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace decompress
{
    public  class CWECompress
    {
        // Constants
        private const int HASHTAB = 4096;   // size of hash
        private const int N = 1024;         // size of ring buffer 
        private const int F = 34;           // size of look ahead buffer
        private const int THRESHOLD = 3;    // minimum match length if match_length is greater than this

        // Variables
        private byte[] ring_buff = new byte[N + F];
        private uint[] next = new uint[N + 1 + HASHTAB];
        private uint[] prev = new uint[N + 1]; /* reserve space for hash as sons */

        // Constructor
        public CWECompress()
        {
            InitTree();
        }


        public void InitTree()
        {
            uint i;

            for (i = 0; i < N + F; i++)
                ring_buff[i] = 0;

            for (i = 0; i < N + 1 + HASHTAB; i++)
                next[i] = N;

            for (i = 0; i < N + 1; i++)
                prev[i] = N;
        }

        /*******************************************/

        public void InsertNode(uint r)
        {
            uint next_r, c;

            c = (uint)(ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff; // hash func
            next_r = next[c + N + 1];
            next[c + N + 1] = r;
            prev[r] = c + N + 1;
            next[r] = next_r;
            if (next_r != N)
                prev[next_r] = r;
        }

        /**********************************************/

        public void DeleteNode(uint r)
        {
            if (prev[r] == N)
                return;

            next[prev[r]] = next[r];
            prev[next[r]] = prev[r];
            prev[r] = next[r] = N;
        }

        /**********************************************/

        public void LocateNode(uint r, ref uint match_len, ref uint match_pos)
        {
            uint p, c, i;

            match_len = 0;
            match_pos = 0;
            c = (uint)(ring_buff[r] + (ring_buff[r + 1] << 8)) & 0xfff; // hash func

            p = next[c + N + 1];
            i = 0;

            while (p != N)
            {
                for (i = 0; (i < F) && (ring_buff[p + i] == ring_buff[r + i]); i++) ;

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

        /***********************************************************************/

        public long FindCompressedLength(byte[] BufSrc)
        {
            long k3, counter;
            ulong i, j;
            byte k, k2;

            i = 0;
            counter = 0;

            while (true)
            {
                if ((i & 0x100) == 0)
                {
                    k = BufSrc[counter];
                    counter++; // incrementar contador
                    //BufSrc++; // avanzar el puntero
                    i =(ulong)(k | 0xFF00);
                }

                if ((BufSrc[counter] == 0) && (BufSrc[counter+1] == 0) && (BufSrc[counter+2] == 0) && (BufSrc[counter+3] == 0))
                    return 0; // salir bloque comprimido inválido

                k2 = BufSrc[counter]; // obtener dato
                //BufSrc++; // avanzar el puntero
                counter++; // incrementar contador

                if (((byte)i & 1) == 0)
                {
                   // BufSrc++; // avanzar el puntero
                    counter++; // incrementar contador
                }
                else
                {
                    if ((k2 & 0x80) != 0)
                    {
                        //BufSrc++; // avanzar el puntero
                        counter++; // incrementar contador

                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            k3 = k - 0xB9;
                            if (k == 0xFF)
                                break; // salir

                            while (k3-- >= 0)
                            {
                                k2 = BufSrc[counter]; // obtener dato
                                //BufSrc++; // avanzar el puntero
                                counter++; // incrementar contador
                            }

                            i = i >> 1; // i SHR 1
                            continue;
                        }
                    }
                    else
                    {
                        j = BufSrc[1]; // obtener dato
                        counter += 2; // avanzar el puntero por 2
                        counter += 2; // incrementar contador
                    }
                }

                i = i >> 1;
            }

            return counter;
        }

        /*********************************************************************************/

        public bool DeCompress(ref byte[] BufDest, byte[] BufSrc)
        {
            byte[] ptrRes = BufDest;
            int counter = 0;
            int counterPtres = 0;
            long k3;
            ulong i, j;
            byte k, k2;
            List<byte> buf = new List<byte>();
            i = 0;

            while (true)
            {
                if ((i & 0x100) == 0)
                {
                    k = BufSrc[counter];
                    counter++; // avanzar el puntero
                    i = (ulong)( k | 0xFF00);
                }

                k2 = BufSrc[counter];
                counter++; // avanzar el puntero

                if (((byte)i & 1) == 0)
                {
                    buf.Add((byte)k2);
                    //ptrRes[counterPtres] = (byte)k2;
                    counterPtres++; // avanzar el puntero
                }
                else
                {
                    if ((k2 & 0x80) != 0)
                    {
                        counter++; // avanzar el puntero

                        if ((k2 & 0x40) != 0)
                        {
                            k = k2;
                            k3 = k - 0xB9;
                            if (k == 0xFF)
                                break;

                            while (k3-- >= 0)
                            {
                                k2 = BufSrc[counter];
                                counter++; // avanzar el puntero
                                buf.Add(k2);
                                //ptrRes[counterPtres] = (byte)k2;
                                counterPtres++; // avanzar el puntero
                            }

                            i = i >> 1;
                            continue;
                        }

                        j = (ulong)(k2 & 0x0F) + 1;
                        k3 = (k2 >> 4) - 7;
                        j = j | ((ulong)(k2 & 3) << 8);

                    }
                    else
                    {
                        j = BufSrc[counter + 1];
                        counter += 2; // avanzar el puntero por 2 bytes
                        k3 = (k2 >> 2) + 2;
                        j = j | ((ulong)(k2 & 3) << 8);
                    }

                    for (; k3 >= 0; k3--)
                    {
                        int contadorTemp = counterPtres - (int)j;
                        
                        buf.Add((byte) (buf[contadorTemp] & 0xFF));
                        //ptrRes[counterPtres] = (byte) (ptrRes[contadorTemp] & 0xFF);
                        counterPtres++; // avanzar el puntero
                    }
                }
                i = i >> 1;
            }
            BufDest = buf.ToArray();
            return true;
        }

        /************************************************************************/

        public bool Compress(ref byte[] BufDest, byte[] BufSrc, ref ulong SizeResult, ulong SizeSrc)
        {
            byte[] ptrRes = new byte[160000]; // Puntero de resultado inicializado con BufDest
            uint r, match_pos, match_len, maxlen, code_buf_ptr;
            ulong ps = 0, textsize, codesize;
            byte[] code_buf = new byte[17]; // Buffer de código de tamaño 17
            byte mask, c;
            uint block;
            List<byte> block_list = new List<byte>();
            InitTree();  // Inicializar árboles

            r = 0;
            textsize = codesize = 0;
            code_buf[0] = 0;
            code_buf_ptr = 1;
            mask = 1;
            match_len = 0;
            match_pos = 0;
            maxlen = 0;
            // Determinar el tamaño del bloque inicial
            if (textsize + F < SizeSrc)
                block = F;
            else
                block = (uint)(SizeSrc - textsize);

            // Copiar el primer bloque de datos a ring_buff
            Array.Copy(BufSrc, 0, ring_buff, 0, block);
            Array.Copy(BufSrc, 0, ring_buff, N, block);
            maxlen = block;
            textsize += block;
            BufSrc = BufSrc.Skip((int)block).ToArray(); // Avanzar el puntero de origen por el tamaño del bloque

            while (maxlen > 0)
            {
                LocateNode(r, ref match_len, ref match_pos); // Buscar el nodo en el árbol

                // Limitar la longitud del match a maxlen
                if (match_len > maxlen)
                    match_len = maxlen;

                if ((match_len < THRESHOLD - 1) || ((match_len < THRESHOLD) && (match_pos > 16)))
                {
                    match_len = 1;  // No es un match lo suficientemente largo. Envía un byte.
                    code_buf[code_buf_ptr++] = ring_buff[r];  // Enviar sin comprimir.
                }
                else if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17))
                {
                    code_buf[0] |= mask;  // Bandera de 'enviar un byte'
                    code_buf[code_buf_ptr++] = (byte)(((match_len + 7 - 1) << 4) | (match_pos - 1));
                }
                else
                {
                    code_buf[0] |= mask;  // Bandera de 'enviar un byte'
                    code_buf[code_buf_ptr++] = (byte)(((match_len - 2 - 1) << 2) | (match_pos >> 8));
                    code_buf[code_buf_ptr++] = (byte)(match_pos & 0xFF);
                }

                if ((mask <<= 1) == 0)
                {
                    // Enviar los códigos reunidos hasta ahora (máximo 8 unidades de código juntas)
                    //ptrRes = block_list.ToArray();
                    Array.Copy(code_buf, 0, ptrRes, 0, code_buf_ptr);
                    ptrRes = ptrRes.Skip((int)code_buf_ptr).ToArray();

                    codesize += (ulong)code_buf_ptr;
                    code_buf[0] = 0;
                    code_buf_ptr = 1;
                    mask = 1;
                }

                // Eliminar el nodo actual del árbol y agregar el nuevo
                while (match_len-- > 0)
                {
                    DeleteNode((r + F) & (N - 1));
                    maxlen--;

                    if (textsize < SizeSrc)
                    {
                        c = BufSrc[0];
                        BufSrc = BufSrc.Skip(1).ToArray(); // Avanzar el puntero de origen
                        ring_buff[(r + F) & (N - 1)] = c;

                        if ((r + F) >= N)
                            ring_buff[r + F] = c;

                        textsize++;
                        maxlen++;
                    }

                    InsertNode(r);
                    r = (r + 1) & (N - 1);
                }
            }

            if (code_buf_ptr > 1)
            {
                code_buf[0] |= mask;  // Bandera de 'enviar un byte'
                code_buf[code_buf_ptr++] = 0xFF;
                code_buf[code_buf_ptr++] = 0x00;
                Array.Copy(code_buf, 0, ptrRes, 0, code_buf_ptr);
                ptrRes = ptrRes.Skip((int)code_buf_ptr).ToArray();

                codesize += (ulong)code_buf_ptr;
            }
            else
            {
                // En caso de que no haya códigos para enviar, enviar un byte especial
                ptrRes[0] = 0x01;
                ptrRes[1] = 0xFF;
                ptrRes[2] = 0x00;
                ptrRes = ptrRes.Skip(3).ToArray();
                codesize += 3;
            }

            SizeResult = codesize; // Tamaño total del resultado
            BufDest = new byte[ptrRes.Length];
            //BufDest = new byte[(int)SizeResult];
            Array.Copy(ptrRes, BufDest, BufDest.Length); 
            return true;
        }

        /*****************************************************************************/
    }
}
