using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public class ImageGrayscaleSdk
    {
        private const string dllPath = @"DLL\ImageGrayscale.dll"; 
        /// <summary>
        /// 通过图片获取8位灰度图
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pRgb8"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <returns></returns>

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GetRgb8ByName(string pFileName, IntPtr pRgb8, ref int pWidth, ref int pHeight);

        /// <summary>
        /// 通过图片获取24位灰度图
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pRgb24"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <returns></returns>

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GetRgb24ByName(string pFileName, IntPtr pRgb24, ref int pWidth, ref int pHeight);


        /// <summary>
        /// 通过字节流获取24位灰度图
        /// </summary>
        /// <param name="m_Buffer"></param>
        /// <param name="m_BufferSize"></param>
        /// <param name="pRgb24"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <returns></returns>
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GetRgb24FromBuffer(byte[] m_Buffer,int m_BufferSize, IntPtr pRgb24, ref int pWidth, ref int pHeight);


        /// <summary>
        /// 通过字节流获取8位灰度图
        /// </summary>
        /// <param name="m_Buffer"></param>
        /// <param name="m_BufferSize"></param>
        /// <param name="pRgb8"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <returns></returns>
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GetRgb8FromBuffer(byte[] m_Buffer, int m_BufferSize, IntPtr pRgb8, ref int pWidth, ref int pHeight);

    }
}
