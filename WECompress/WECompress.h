
#if _MSC_VER > 1000
#pragma once
#endif 
#ifndef WECOMPRESS_H
#define WECOMPRESS_H

#include <windows.h>

#ifdef WE_COMPRESS_EXPORTS
#define WE_COMPRESS_API __declspec(dllexport)
#else
#define WE_COMPRESS_API __declspec(dllimport)
#endif

#define HASHTAB  4096	// size of hash
#define N		 1024	// size of ring buffer 
#define F  		 34     // size of look ahead buffer
#define THRESHOLD 3 



BYTE ring_buff[N + F];
UINT next[N + 1 + HASHTAB], prev[N + 1]; /* reserve space for hash as sons */

extern "C" {
	WE_COMPRESS_API void InitTree(void);
	WE_COMPRESS_API void InsertNode(unsigned int r);
	WE_COMPRESS_API void DeleteNode(unsigned int r);
	WE_COMPRESS_API void LocateNode(unsigned int r, unsigned int* match_len, unsigned int* match_pos);
	WE_COMPRESS_API BOOL Compress(BYTE** BufDest, BYTE* BufSrc, ULONG* SizeResult, ULONG SizeSrc);
	WE_COMPRESS_API BOOL DeCompress(BYTE** BufDest, BYTE* BufSrc);
	WE_COMPRESS_API LONG FindCompressedLength(BYTE* BufSrc);
}

#endif // CWECOMPRESS_H
//using BYTE = uint8_t;
//using BOOL = bool;
//using ULONG = uint32_t;
//using LONG = int32_t;
//using UINT = unsigned int;  // Definición de UINT