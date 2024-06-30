#pragma once

#include <stdio.h>
#include <cstdint>

#ifndef WECOMPRESS_H
#define WECOMPRESS_H

#ifdef WE_COMPRESS_EXPORTS
#define WE_COMPRESS_API __declspec(dllexport)
#else
#define WE_COMPRESS_API __declspec(dllimport)
#endif

#define HASHTAB  4096	// size of hash
#define N		 1024	// size of ring buffer 
#define F  		 34     // size of look ahead buffer
#define THRESHOLD 3 



uint8_t ring_buff[N + F];
unsigned int next[N + 1 + HASHTAB], prev[N + 1]; /* reserve space for hash as sons */

extern "C" {
	WE_COMPRESS_API void InitTree(void);
}
extern "C" {
	WE_COMPRESS_API void InsertNode(unsigned int r);
}
extern "C" {
	WE_COMPRESS_API void DeleteNode(unsigned int r);
}
extern "C" {
	WE_COMPRESS_API void LocateNode(unsigned int r, unsigned int* match_len, unsigned int* match_pos);
}
extern "C" {
	WE_COMPRESS_API BOOL WECompress(BYTE** BufDest, BYTE* BufSrc, ULONG* SizeResult, ULONG SizeSrc);
}




#endif // CWECOMPRESS_H
//using BYTE = uint8_t;
//using BOOL = bool;
//using ULONG = uint32_t;
//using LONG = int32_t;
//using UINT = unsigned int;  // Definición de UINT