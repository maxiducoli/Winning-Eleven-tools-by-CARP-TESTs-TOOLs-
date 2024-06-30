#pragma once
#include "minwindef.h"

//
//  TIM File structures and keywords
//

#define TIM_ID1         0x10              /* TIM ID    */
#define TIM_4Bit        0x08              /* USE CLUT  */
#define TIM_8Bit        0x09              /* USE CLUT  */
#define TIM_16Bit       0x02              /* NO CLUT   */
#define TIM_24Bit       0x03              /* NO CLUT   */

typedef struct
{
	DWORD   id1;
	DWORD   type;
	DWORD   nextlen;
} TIM_HEADER;

typedef struct
{
	WORD	vramx;
	WORD	vramy;
	WORD	ncols;
	WORD	npals;
} TIM_CLUTINFO;

typedef struct
{
	DWORD	blocklen;
	WORD    vramx;
	WORD	vramy;
	WORD	xsize;
	WORD	ysize;
} TIM_DATAINFO;

typedef struct
{
	WORD	ID;
	WORD	VramX;
	WORD	VramY;
	WORD	width;
	WORD	height;
	WORD	unknown3;
	WORD	offset;
	WORD	separator;
	WORD	unknown5;
	WORD	unknown6;
	WORD	unknown7;
	WORD	unknown8;
	WORD	unknown9;
	WORD	unknown10;
	WORD	unknown11;
	WORD	unknown12;
} DATA_HEADER;

//
// BMP keywords...
//

#define BMFH_SIZE               (14)
#define BMIH_SIZE               (40)
#define RGB_SIZE256             (4*256)
#define RGB_SIZE16              (4*16)
#define TOTAL_HEADER_SIZE256    (BMFH_SIZE + BMIH_SIZE + RGB_SIZE256)
#define TOTAL_HEADER_SIZE16    (BMFH_SIZE + BMIH_SIZE + RGB_SIZE16)

//
//
typedef CTypedPtrList<CPtrList, CFilesStruct*> CFilesStructList;

class CFilesStruct
{
public:
	DATA_HEADER	m_Header;
	BYTE* m_DataBlock;
	ULONG m_BlockLen;
	DWORD m_OffsetReal;
	BYTE m_MultiplyFactor;
	BYTE m_MultiplyY;

};
