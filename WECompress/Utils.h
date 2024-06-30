#pragma once
#include "Structures.h"
#include "pch.h"
#include "afx.h"

#ifdef UTILS_EXPORTS
#define UTILS_EXPORTS __declspec(dllexport)
#else
#define UTILS_EXPORTS __declspec(dllimport)
#endif


CFilesStructList m_ImageFileList;
CFilesStructList m_PaletteFileList;
CString m_ImageFile;
CString m_PaletteFile;
CString m_SourceImageFile;
CString m_SourcePaletteFile;
UINT	m_ActualImagesIndex;
UINT	m_ActualPalettesIndex;
UINT	m_ActualColorsNumber;
BOOL	m_FirstOK;
BOOL	m_SecondOK;
BOOL	m_ThirdOK;
BOOL	m_FourthOK;
BOOL	m_AutomaticPal;


extern "C" {
	WE_COMPRESS_API bool DecodeImage(uint8_t* BufSrc, uint8_t** BufDest, uint32_t xsize, uint32_t ysize,
		uint8_t depth, uint8_t ComprFlag);
}

