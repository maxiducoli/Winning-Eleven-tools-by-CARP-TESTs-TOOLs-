using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TIMTools
{

    public static  class DecompressWrapper
    {
        // Importamos la función Decompress desde la DLL de C++
        [DllImport("WeDecompress.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Decompress(IntPtr BufDest, IntPtr BufSrc);

        public static bool CallDecompress(byte[] bufDest, byte[] bufSrc)
        {
            IntPtr ptrDest = Marshal.AllocHGlobal(bufDest.Length);
            Marshal.Copy(bufDest, 0, ptrDest, bufDest.Length);

            IntPtr ptrSrc = Marshal.AllocHGlobal(bufSrc.Length);
            Marshal.Copy(bufSrc, 0, ptrSrc, bufSrc.Length);

            bool result = Decompress(ptrDest, ptrSrc);

            // Copiar los datos de vuelta a bufDest solo si es necesario
            if (result)
            {
                Marshal.Copy(ptrDest, bufDest, 0, bufDest.Length);
            }

            Marshal.FreeHGlobal(ptrDest);
            Marshal.FreeHGlobal(ptrSrc);

            return result;
        }

    }
}
