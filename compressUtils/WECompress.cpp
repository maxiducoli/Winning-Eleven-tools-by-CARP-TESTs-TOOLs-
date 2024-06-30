#pragma
#include "pch.h"
#include "WECompress.h"
#include <windows.h>


void InitTree(void)  // initialize trees 
{
	UINT i;

	for (i = 0; i < N + F; i++)
		ring_buff[i] = '\0';

	for (i = 0; i < N + 1 + HASHTAB; i++)
		next[i] = N;

	for (i = 0; i < N + 1; i++)
		prev[i] = N;
}

void InsertNode(UINT r)
{
	UINT next_r, c;

	c = ring_buff[r] + (ring_buff[r + 1] << 8) & 0xfff;// hash func
	next_r = next[c + N + 1];
	next[c + N + 1] = r;
	prev[r] = c + N + 1;
	next[r] = next_r;
	if (next_r != N)
		prev[next_r] = r;
}

void DeleteNode(UINT r)
{
	if (prev[r] == N)
		return;
	next[prev[r]] = next[r];
	prev[next[r]] = prev[r];
	prev[r] = next[r] = N;
}

void LocateNode(UINT r, UINT* match_len, UINT* match_pos)
{
	UINT p, c, i;

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

BOOL Compress(BYTE** BufDest, BYTE* BufSrc, ULONG* SizeResult, ULONG SizeSrc)
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

BOOL DeCompress(BYTE** BufDest, BYTE* BufSrc)
{
	BYTE* ptrRes = *BufDest;

	LONG k3;
	ULONG i, j;
	BYTE k, k2;

	i = 0;

	while (TRUE)
	{
		if ((i & 0x100) == 0) // (i < 0x100) or (0x1000 <i < 0x10FF) ...
		{
			k = *BufSrc & 0xFF; // Get data and do data % FF
			BufSrc++; // add pointer
			i = k | 0xFF00; // i = 0xFF unito a k

		}

		k2 = *BufSrc & 0xFF; // get data			

		if (((BYTE)i & 1) == 0) // t = 0 se i pari, t = 1 se i dispari
			// Literal
		{
			*ptrRes = (BYTE)k2; // res = k2
			ptrRes++;// add pointer
			BufSrc++;
		}
		else
		{
			if ((k2 & 0x80) != 0)  // k2 & 0x80 != 0 Solo se k2 > 0x80 il controllo passa
				// Caso in cui abbiamo 1 solo byte per il comando, 
				// nbit move = 4, nbitrepeat = 4
			{
				BufSrc++;// add pointer

				if ((k2 & 0x40) != 0) // k2 & 0x40 != 0 Solo se k2 > 0xC0 il controllo passa
				{
					// blocco per copiare i bytes in plain mode
					// nrepeatmax = 0xFE-0xB9 = 69 7 bit
					// nrepeatmin = 0xC0-0xB9 = 7

					k = k2 & 0xFF;// k = k2 & 0xFF
					k3 = k - 0xB9;// k3 = k - 0xB9  k3 = numero ottenuto sottraendo k2 fatto passare
					if (k == 0xFF)
						break; // esci dal ciclo

					// ciclo che copia i k3 bytes in plain mode
					while (k3-- >= 0) //sicuramente k3 > 0
					{
						k2 = *BufSrc & 0xFF;// get data
						BufSrc++;// add pointer
						ptrRes++;// add pointer						
						*(ptrRes - 1) = (BYTE)k2; // write k2 on ptrRes

					}

					i = i >> 1;// i SHR 1
					continue;
				}

				// questo j mi dirà quanto dovrò spostarmi indietro per ripetere k3+1 volte il 
				// ciclo (max 16)
				// max j = 16, k3 = 4 con BF
				j = (k2 & 0x0F) + 1;// k2 & 0x0F = prendo gli ultimi 4 bit e sommo 1 
				k3 = (k2 >> 4) - 7;// k2 >> 4 = tolgo gli ultimi 4 bit,e tolgo 7;
				// k3 mi dice quante volte devo ripetere il byte + 1 volta del ciclo + 1 messa in precedenza
				// es. 90 -> j = 1 [1 byte indietro], k3 = 90 >> 4 - 7 = 9 - 7 = 2 
				// -> 2 + 1 + 1 = 4 volte

			}
			else
			{
				// 2 bytes per il comando tipo 24 01
				// nbit move = 14, nbitrepeat = 6

				j = *(BufSrc + 1) & 0xFF;// get data (prendi il byte successivo)
				BufSrc += 2;// add pointer by 2
				k3 = (k2 >> 2) + 2;// k3 = togli ultimi 2 bit + 2 (24 = 11 volte infatti)
				j = j | (k2 & 3) << 8;// j = j | (k2 & 3)*256
				// max j = 1024, k3 = 33 con 7F FF
				// numero di bytes da arretrare per poi scrivere k3 volte i bytes ritrovati
				// es 24 01 -> j = 01 | (24 & 3) << 8 = 1 | 0 = 1, k3 = 24 >> 2 + 2 = 9 + 2 = 11
				// -> 11 + 1 + 1 = 13 volte
			}

			// ciclo per ripetere k3 volte + 1 il byte a partire dalla posizione -j			
			for (; k3 >= 0; k3--) // loop until k3>=0
			{
				*ptrRes = *(ptrRes - j) & 0xFF;// write data from first (far j bytes) for k3 times
				ptrRes++;

			};

		}

		i = i >> 1;

	}

	return TRUE;
}

LONG FindCompressedLength(BYTE* BufSrc)
{
	LONG k3, counter;
	ULONG i, j;
	BYTE k, k2;

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

		if (((BYTE)i & 1) == 0)
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

//using BYTE = uint8_t;
//using BOOL = bool;
//using ULONG = uint32_t;
//using LONG = int32_t;
//using UINT = unsigned int;  
//using WORD = unsigned short;