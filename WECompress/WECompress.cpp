#pragma
#include "stdafx.h"
#include "WECompress.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif




void InitTree(void)  // initialize trees 
{
    unsigned int i;

    for (i = 0; i < N + F; i++)
        ring_buff[i] = '\0';

    for (i = 0; i < N + 1 + HASHTAB; i++)
        next[i] = N;

    for (i = 0; i < N + 1; i++)
        prev[i] = N;
}

void InsertNode(unsigned int r)
{
    unsigned int next_r, c;

    c = ring_buff[r] + (ring_buff[r + 1] << 8) & 0xfff;// hash func
    next_r = next[c + N + 1];
    next[c + N + 1] = r;
    prev[r] = c + N + 1;
    next[r] = next_r;
    if (next_r != N)
        prev[next_r] = r;
}

void DeleteNode(unsigned int r)
{
    if (prev[r] == N)
        return;
    next[prev[r]] = next[r];
    prev[next[r]] = prev[r];
    prev[r] = next[r] = N;
}

void LocateNode(unsigned int r, unsigned int* match_len, unsigned int* match_pos)
{
    unsigned int p, c, i;

    *match_len = 0;
    *match_pos = 0;
    c = ring_buff[r] + (ring_buff[r + 1] << 8) & 0xfff;// hash func

    p = next[c + N + 1];
    i = 0;

    while (p != N)
    {
        for (i = 0; (i < F) && (ring_buff[p + i] == ring_buff[r + i]); i++);

        if (i > *match_len)
        {
            *match_len = i;
            *match_pos = (r - p) & (N - 1);
        };

        if (i == F)
            break;

        p = next[p];
    };

    if (i == F)
        DeleteNode(p);
}

int32_t FindCompressedLength(uint8_t* BufSrc)
{
    int32_t k3, counter;
    uint32_t i, j;
    uint8_t k, k2;

    i = 0;
    counter = 0;

    while (TRUE)
    {
        if ((i & 0x100) == 0)
        {
            k = *BufSrc & 0xFF;
            BufSrc++; // add pointer
            counter++; // counter
            i = k | 0xFF00;

        }

        if ((*BufSrc == 0) && (*(BufSrc + 1) == 0) && (*(BufSrc + 2) == 0) && (*(BufSrc + 3) == 0))
            return 0;// exit invalid compressed block

        k2 = *BufSrc & 0xFF; // get data			

        if (((uint8_t)i & 1) == 0)
        {
            BufSrc++;
            counter++; // counter
        }
        else
        {
            if ((k2 & 0x80) != 0)
            {
                BufSrc++;// add pointer
                counter++; // counter

                if ((k2 & 0x40) != 0)
                {
                    k = k2 & 0xFF;
                    k3 = k - 0xB9;
                    if (k == 0xFF)
                        break; // esci

                    while (k3-- >= 0)
                    {
                        k2 = *BufSrc & 0xFF;// get data
                        BufSrc++;// add pointer
                        counter++; // counter

                    }

                    i = i >> 1;// i SHR 1
                    continue;
                }

            }
            else
            {
                j = *(BufSrc + 1) & 0xFF;// get data (prendi il byte successivo)
                BufSrc = BufSrc + 2;// add pointer by 2
                counter += 2; // counter
            }

        }

        i = i >> 1;

    }

    return counter;

}

bool Compress(uint8_t** BufDest, uint8_t* BufSrc, uint32_t* SizeResult, uint32_t SizeSrc)
{
    uint8_t* ptrRes = *BufDest;
    unsigned int r, match_pos, match_len, maxlen, code_buf_ptr;
    uint32_t ps = 0, textsize, codesize;
    uint8_t  code_buf[17], mask, c;
    unsigned int block;

    InitTree();  // initialize trees     

    r = 0;
    textsize = codesize = 0;
    code_buf[0] = 0;
    code_buf_ptr = 1;
    mask = 1;

    if (textsize + F < SizeSrc)
        block = F;
    else
        block = SizeSrc - textsize;

    memcpy(ring_buff, BufSrc, block);
    memcpy(ring_buff + N, BufSrc, block);
    maxlen = block;
    textsize += block;
    BufSrc += block;

    while (maxlen)
    {
        LocateNode(r, &match_len, &match_pos);
        if (match_len > maxlen) match_len = maxlen;//at the end of file often happens		
        if ((match_len < THRESHOLD - 1) || ((match_len < THRESHOLD) && (match_pos > 16)))
        {
            match_len = 1;  // Not long enough match.  Send one byte.
            code_buf[code_buf_ptr++] = ring_buff[r];  // Send uncoded. 
        }
        else
            if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17)) {
                code_buf[0] |= mask;  // 'send one byte' flag 
                code_buf[code_buf_ptr++] = (uint8_t)
                    (((match_len + 7 - 1) << 4) | (match_pos - 1));

            }
        /*	else
            if ((match_pos == 1) && (match_len > 6)) {
                code_buf[0] |= mask;  // 'send one byte' flag
                code_buf[code_buf_ptr++] = (BYTE)
                    (match_len + 0xB9);
            }*/
            else
            {
                code_buf[0] |= mask;  // 'send one byte' flag 
                code_buf[code_buf_ptr++] = (uint8_t)
                    (((match_len - 2 - 1) << 2) | (match_pos >> 8));
                code_buf[code_buf_ptr++] = (uint8_t)
                    (match_pos & 0xFF);

            }
        if ((mask <<= 1) == 0)
        {  // Shift mask left one bit. 
            memcpy(ptrRes, code_buf, code_buf_ptr);// Send at most 8 units of code together 
            ptrRes += code_buf_ptr;

            codesize += code_buf_ptr;
            code_buf[0] = 0;
            code_buf_ptr = 1;
            mask = 1;
        }

        while (match_len--)
        {
            DeleteNode((r + F) & (N - 1));
            maxlen--;
            if (textsize < SizeSrc)
            {
                c = *BufSrc++;
                ring_buff[(r + F) & (N - 1)] = c;
                if (r + F >= N) ring_buff[r + F] = c;
                textsize++; maxlen++;
            };

            InsertNode(r);
            r = (r + 1) & (N - 1);
        };

    };

    if (code_buf_ptr > 1)
    {
        code_buf[0] |= mask;  // 'send one byte' flag 
        code_buf[code_buf_ptr++] = 0xFF;
        code_buf[code_buf_ptr++] = 0x00;
        memcpy(ptrRes, code_buf, code_buf_ptr);
        ptrRes += code_buf_ptr;
        codesize += code_buf_ptr;
    }
    else
    {
        *ptrRes = 0x01;
        *ptrRes = 0xFF;
        *ptrRes = 0x00;
        ptrRes += 3;
    }

    *SizeResult = codesize;

    return true;
}

BOOL WECompress(BYTE** BufDest, BYTE* BufSrc, ULONG* SizeResult, ULONG SizeSrc)
{
    BYTE* ptrRes = *BufDest;
    UINT r, match_pos, match_len, maxlen, code_buf_ptr;
    ULONG ps = 0, textsize, codesize;
    BYTE  code_buf[17], mask, c;
    UINT block;

    InitTree();  // initialize trees     

    r = 0;
    textsize = codesize = 0;
    code_buf[0] = 0;
    code_buf_ptr = 1;
    mask = 1;

    if (textsize + F < SizeSrc)
        block = F;
    else
        block = SizeSrc - textsize;

    memcpy(ring_buff, BufSrc, block);
    memcpy(ring_buff + N, BufSrc, block);
    maxlen = block;
    textsize += block;
    BufSrc += block;

    while (maxlen)
    {
        LocateNode(r, &match_len, &match_pos);
        if (match_len > maxlen) match_len = maxlen;//at the end of file often happens		
        if ((match_len < THRESHOLD - 1) || ((match_len < THRESHOLD) && (match_pos > 16)))
        {
            match_len = 1;  // Not long enough match.  Send one byte.
            code_buf[code_buf_ptr++] = ring_buff[r];  // Send uncoded. 
        }
        else
            if ((match_len > 2 - 1) && (match_len < 6) && (match_pos < 17)) {
                code_buf[0] |= mask;  // 'send one byte' flag 
                code_buf[code_buf_ptr++] = (BYTE)
                    (((match_len + 7 - 1) << 4) | (match_pos - 1));

            }
        /*	else
            if ((match_pos == 1) && (match_len > 6)) {
                code_buf[0] |= mask;  // 'send one byte' flag
                code_buf[code_buf_ptr++] = (BYTE)
                    (match_len + 0xB9);
            }*/
            else
            {
                code_buf[0] |= mask;  // 'send one byte' flag 
                code_buf[code_buf_ptr++] = (BYTE)
                    (((match_len - 2 - 1) << 2) | (match_pos >> 8));
                code_buf[code_buf_ptr++] = (BYTE)
                    (match_pos & 0xFF);

            }
        if ((mask <<= 1) == 0)
        {  // Shift mask left one bit. 
            memcpy(ptrRes, code_buf, code_buf_ptr);// Send at most 8 units of code together 
            ptrRes += code_buf_ptr;

            codesize += code_buf_ptr;
            code_buf[0] = 0;
            code_buf_ptr = 1;
            mask = 1;
        }

        while (match_len--)
        {
            DeleteNode((r + F) & (N - 1));
            maxlen--;
            if (textsize < SizeSrc)
            {
                c = *BufSrc++;
                ring_buff[(r + F) & (N - 1)] = c;
                if (r + F >= N) ring_buff[r + F] = c;
                textsize++; maxlen++;
            };

            InsertNode(r);
            r = (r + 1) & (N - 1);
        };

    };

    if (code_buf_ptr > 1)
    {
        code_buf[0] |= mask;  // 'send one byte' flag 
        code_buf[code_buf_ptr++] = 0xFF;
        code_buf[code_buf_ptr++] = 0x00;
        memcpy(ptrRes, code_buf, code_buf_ptr);
        ptrRes += code_buf_ptr;
        codesize += code_buf_ptr;
    }
    else
    {
        *ptrRes = 0x01;
        *ptrRes = 0xFF;
        *ptrRes = 0x00;
        ptrRes += 3;
    }

    *SizeResult = codesize;

    return TRUE;
}

bool  DeCompress(uint8_t** BufDest, uint8_t* BufSrc) {
       uint8_t* ptrRes = *BufDest;
    int32_t k3;
    uint32_t i = 0;
    uint8_t k, k2;
    uint32_t j;

    while (true) {
        if ((i & 0x100) == 0) {
            k = *BufSrc & 0xFF;
            BufSrc++;
            i = k | 0xFF00;
        }

        k2 = *BufSrc & 0xFF;

        if ((i & 1) == 0) {
            *ptrRes = k2;
            ptrRes++;
            BufSrc++;
        }
        else {
            if ((k2 & 0x80) != 0) {
                BufSrc++;

                if ((k2 & 0x40) != 0) {
                    k = k2 & 0xFF;
                    k3 = k - 0xB9;
                    if (k == 0xFF) {
                        break;
                    }

                    while (k3-- >= 0) {
                        k2 = *BufSrc & 0xFF;
                        BufSrc++;
                        ptrRes++;
                        *(ptrRes - 1) = k2;
                    }

                    i = i >> 1;
                    continue;
                }

                j = (k2 & 0x0F) + 1;
                k3 = (k2 >> 4) - 7;
            }
            else {
                j = *(BufSrc + 1) & 0xFF;
                BufSrc += 2;
                k3 = (k2 >> 2) + 2;
                j = j | ((k2 & 3) << 8);
            }

            for (; k3 >= 0; k3--) {
                *ptrRes = *(ptrRes - j) & 0xFF;
                ptrRes++;
            }
        }

        i = i >> 1;
    }

    return true;
}

uint32_t Decompress(uint8_t** BufDest, uint8_t* BufSrc) {
    uint8_t* ptrRes = *BufDest;
    uint8_t* initialPtrRes = ptrRes;  // Guardamos el puntero inicial para calcular la diferencia al final
    int32_t k3;
    uint32_t i = 0;
    uint8_t k, k2;
    uint32_t j;

    while (true) {
        if ((i & 0x100) == 0) {
            k = *BufSrc & 0xFF;
            BufSrc++;
            i = k | 0xFF00;
        }

        k2 = *BufSrc & 0xFF;

        if ((i & 1) == 0) {
            *ptrRes = k2;
            ptrRes++;
            BufSrc++;
        }
        else {
            if ((k2 & 0x80) != 0) {
                BufSrc++;

                if ((k2 & 0x40) != 0) {
                    k = k2 & 0xFF;
                    k3 = k - 0xB9;
                    if (k == 0xFF) {
                        break;
                    }

                    while (k3-- >= 0) {
                        k2 = *BufSrc & 0xFF;
                        BufSrc++;
                        *ptrRes = k2;
                        ptrRes++;
                    }

                    i = i >> 1;
                    continue;
                }

                j = (k2 & 0x0F) + 1;
                k3 = (k2 >> 4) - 7;
            }
            else {
                j = *(BufSrc + 1) & 0xFF;
                BufSrc += 2;
                k3 = (k2 >> 2) + 2;
                j = j | ((k2 & 3) << 8);
            }

            for (; k3 >= 0; k3--) {
                *ptrRes = *(ptrRes - j) & 0xFF;
                ptrRes++;
            }
        }

        i = i >> 1;
    }

    // Calculamos la cantidad de bytes descomprimidos
    return  (uint32_t)(ptrRes - initialPtrRes);
      
}

//using BYTE = uint8_t;
//using BOOL = bool;
//using ULONG = uint32_t;
//using LONG = int32_t;
//using UINT = unsigned int;  
//using WORD = unsigned short;