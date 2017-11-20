using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public class GFace7Sdk
    {
        public const string dllPath = @"DLL\GFace7\GFace7.dll";


        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_Init(string pKey);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static void GFace7_Uninit();


        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace7_GetFeaSize();



        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace7_Detect(uint nThreadID, byte[] pGrayBuf, int nWidth, int nHeight, IntPtr FaceRect, IntPtr FacePoint);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static float GFace7_Compare(uint nThreadID, byte[] pFea1, byte[] pFea2);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_GetFea(uint nThreadID, byte[] pRgb24Buf, int nWidth, int nHeight, int nCount, FacePointInfo[] FacePoint, IntPtr ptrFea);


        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_CheckKey();

        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static string GFace7_GetVer();
    }
}
