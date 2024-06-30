#pragma
#include "pch.h"
#include "WECompress.h"
#include "Utils.h"

CFilesStructList m_ImageFileList;
CFilesStructList m_PaletteFileList;
CString m_ImageFile = CString(" ");
CString m_PaletteFile = CString(" ");
CString m_SourceImageFile = CString(" ");
CString m_SourcePaletteFile = CString(" ");
unsigned int m_ActualImagesIndex = 0;
unsigned int m_ActualPalettesIndex = 0;
unsigned int m_ActualColorsNumber = 256;
unsigned int m_ComboPalIndex = 0; // INDICE DEL COMBO DE LA PALETA (4 bits - 8 bits)
unsigned int m_ComboIndex = 0; // INDICE DEL COMBO DE LA IMAGEN (BMP - TIM - RAW)
bool m_AutomaticPal;

bool DecodeImage(uint8_t* BufSrc, uint8_t** BufDest, uint32_t xsize, uint32_t ysize,
	uint8_t depth, uint8_t ComprFlag)
{
	uint32_t xtemp;
	unsigned int j;

	uint8_t* pDest = *BufDest;

	j = 0;
	if (depth == 4)
	{
		// 4 bit
		xtemp = xsize / 2;
		if (ComprFlag == BI_RGB)
		{
			for (long y1 = ysize - 1; y1 >= 0; y1--)
			{
				for (unsigned long x1 = 0; x1 < xtemp; x1++)
				{
					pDest[j] = (BufSrc[(y1 * xtemp) + x1]) >> 4;
					pDest[j] |= ((BufSrc[(y1 * xtemp) + x1] << 4) & 0xF0);
					j++;
				}
			}
		}
		else
		{
			// BI_RLE4
			bool bEOB = FALSE;
			uint8_t* pSrc = BufSrc;

			int code1, code2, i, k, hi = 0, abs_cou = 0, adj_cou = 0;

			uint8_t* pTempData = new uint8_t[xtemp * ysize];
			pDest = pTempData;

			uint8_t* sta_ptr = pDest;
			for (i = 0; i < xtemp * ysize && !bEOB; i += 2)
			{
				code1 = *pSrc++;
				code2 = *pSrc++;

				if (abs_cou)
				{
					if (hi)
						*pDest++ |= (code1 >> 4);
					else
						*pDest = (code1 & 0xF0);
					abs_cou--;
					hi ^= 1;

					if (abs_cou)
					{
						if (hi)
							*pDest++ |= (code1 & 0x0F);
						else
							*pDest = (code1 << 4);
						abs_cou--;
						hi ^= 1;
					}

					if (abs_cou)
					{
						if (hi)
							*pDest++ |= (code2 >> 4);
						else
							*pDest = (code2 & 0xF0);
						abs_cou--;
						hi ^= 1;
					}

					if (abs_cou)
					{
						if (hi)
							*pDest++ |= (code2 & 0x0F);
						else
							*pDest = (code2 << 4);
						abs_cou--;
						hi ^= 1;
					}
					continue;

				}

				if (code1 == 0)  // RLE_COMMAND
				{
					switch (code2) // Escape
					{
					case 0:	// End of line escape EOL
						if (!adj_cou)  adj_cou = 3 - ((pDest - sta_ptr + 3) % 4);
						for (i = 0; i < adj_cou; i++) *pDest++ = 0;
						continue;
					case 1: // End of block escape EOB
						if (!adj_cou)  adj_cou = 3 - ((pDest - sta_ptr + 3) % 4);
						for (i = 0; i < adj_cou; i++) *pDest++ = 0;
						bEOB = TRUE;
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
							*pDest++ |= (code2 >> 4);
							*pDest = (code2 & 0x0F);
						}
						else
							*pDest++ = code2;
					}

					if (code1 % 2)
					{
						if (hi)
							*pDest++ |= (code2 >> 4);
						else
							*pDest = (code2 & 0xF0);
						hi ^= 1;
					}

				}
			}

			pDest = *BufDest;
			for (long y1 = ysize - 1; y1 >= 0; y1--)
			{
				for (unsigned long x1 = 0; x1 < xtemp; x1++)
				{
					pDest[j] = (pTempData[(y1 * xtemp) + x1]) >> 4;
					pDest[j] |= ((pTempData[(y1 * xtemp) + x1] << 4) & 0xF0);
					j++;
				}
			}

			delete pTempData;

		}
	}
	else
	{
		// 8 bit

		xtemp = xsize;
		if (ComprFlag == BI_RGB)
		{
			for (long y1 = ysize - 1; y1 >= 0; y1--) {
				for (unsigned long x1 = 0; x1 < xtemp; x1++)
					pDest[j++] = BufSrc[(y1 * xtemp) + x1];
			}
		}
		else
		{
			// BI_RLE8
			bool bEOB = FALSE;
			uint8_t* pSrc = BufSrc;

			int code1, code2, i, k, abs_cou = 0, adj_cou = 0;

			uint8_t* pTempData = new uint8_t[xtemp * ysize];
			pDest = pTempData;

			uint8_t* sta_ptr = pDest;
			for (i = 0; i < xtemp * ysize && !bEOB; i += 2)
			{
				code1 = *pSrc++;
				code2 = *pSrc++;

				if (abs_cou)
				{
					*pDest++ = code1;
					abs_cou--;
					if (abs_cou)
					{
						*pDest++ = code2;
						abs_cou--;
					}
					continue;
				}

				if (code1 == 0)  // RLE_COMMAND
				{
					switch (code2) // Escape
					{
					case 0:	// End of line escape EOL
						if (!adj_cou)  adj_cou = 3 - ((pDest - sta_ptr + 3) % 4);
						for (i = 0; i < adj_cou; i++) *pDest++ = 0;
						continue;
					case 1: // End of block escape EOB
						if (!adj_cou)  adj_cou = 3 - ((pDest - sta_ptr + 3) % 4);
						for (i = 0; i < adj_cou; i++) *pDest++ = 0;
						bEOB = TRUE;
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
						*pDest++ = code2;
			}

			pDest = *BufDest;
			for (long y1 = ysize - 1; y1 >= 0; y1--) {
				for (unsigned long x1 = 0; x1 < xtemp; x1++)
					pDest[j++] = pTempData[(y1 * xtemp) + x1];
			}

			delete pTempData;
		}
	}

	return true;
}

//using BYTE = uint8_t;
//using BOOL = bool;
//using ULONG = uint32_t;
//using LONG = int32_t;
//using UINT = unsigned int;  
//using WORD = unsigned short;