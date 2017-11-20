using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{   
    public class ZFaceSdk
    {
        private const string dllPath = @"DLL\ZFace6.dll";
       
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static float ZFace_Compare(string pFile1, string pFile2);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool ZFace_Init();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static void ZFace_Uninit();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool ZFace_CheckKey();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static IntPtr ZFace_GetVer();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int ZFace_Detect(byte[] pGrayBuf,int nWidth,int nHeight, IntPtr Rect);
    }
}
