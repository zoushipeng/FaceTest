using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    /// <summary>
    /// 人脸坐标
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FaceRectInfo
    {
        public RECT rc;       //坐标
        public double roll;   //保留
        public double pitch;  //保留
        public double yaw;    //保留
        public double score;  //保留
    }

    /// <summary>
    /// 坐标点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// 人脸特征点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FacePointInfo
    {
        public POINT ptEyeLeft;//左眼
        public POINT ptEyeRight;//右眼
        public POINT ptNose;//鼻子
        public POINT ptMouthLeft;//嘴巴左
        public POINT ptMouthRight;//嘴巴右
    }

    public class GFaceSdk
    {
        private const string dllPath = @"DLL\GFace6.dll";
       
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static float GFace6_Compare(int nThreadID, byte[] pFea1, byte[] pFea2);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace6_Init(string pKey);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static void GFace6_Uninit();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace6_CheckKey();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static IntPtr GFace6_GetVer();


        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace6_Detect(uint nThreadID, byte[] pGrayBuf,int nWidth,int nHeight, IntPtr FaceRect, IntPtr FacePoint);

        /// <summary>
        /// 特征提取
        /// </summary>
        /// <param name="nThreadID"></param>
        /// <param name="pGrayBuf"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="FaceRect"></param>
        /// <param name="ptrFea"></param>
        /// <returns></returns>
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]       
        public extern static bool GFace6_GetFea(int nThreadID, byte[] pGrayBuf, int nWidth, int nHeight, int nCount, FacePointInfo[] FaceRect, IntPtr ptrFea);



        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        [DllImport("FaceDetection.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool init(string libPath);


        /// <summary>
        /// 卸载
        /// </summary>
        /// <returns></returns>
        [DllImport("FaceDetection.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool unInit();


        /// <summary>
        /// 检测人脸
        /// </summary>
        /// <param name="pdata"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="faceInfo"></param>
        /// <returns></returns>
        [DllImport("FaceDetection.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int detectface(IntPtr pdata, Int32 width, Int32 height, IntPtr faceInfo);
    }
}
