using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OneCardSystem.Recognizer
{
    /// <summary>
    /// 1对1对比
    /// </summary>
    public class ZFaceRecognizer : IFaceRecognizer
    {
        bool inited = false;
        private static ZFaceRecognizer instance;
        static object obj = new object();
        static object objCompare = new object();
        static object obj1 = new object();
        public static ZFaceRecognizer GetInstance()
        {
            if (instance == null)
            {
                instance = new ZFaceRecognizer();
            }
            return instance;
        }
        private ZFaceRecognizer()
        {
            ptrRect = Marshal.AllocHGlobal(4096);
        }

        ~ZFaceRecognizer()
        {
        }

        public float Compare(string strFile1, string strFile2)
        {
            float result = 0;

            try
            {
                lock (objCompare)
                {
                    //LogHelper.WriteLog("#########----库开始比对----###########");                   
                    result = ZFaceSdk.ZFace_Compare(strFile1, strFile2);
                    //LogHelper.WriteLog("#########----库比对结束----###########");
                }

            }
            catch (Exception e)
            {
            }

            return result;
        }

        public bool Init()
        {
            if (!inited)
            {
                try
                {
                    inited = ZFaceSdk.ZFace_Init();
                }
                catch (Exception e)
                { }
            }
            return inited;
        }

        public bool Uninit()
        {
            ZFaceSdk.ZFace_Uninit();
            inited = false;
            return true;
        }

        public IntPtr GetVersion()
        {
            IntPtr version = ZFaceSdk.ZFace_GetVer();
            return version;
        }

        public bool CheckKey()
        {
            return ZFaceSdk.ZFace_CheckKey();
        }
        IntPtr ptrRect = IntPtr.Zero;
        private int FaceDetect(byte[] pGrayBuf, int nWidth, int nHeight, out RECT[] rect)
        {
            int nFaceNum = 0;
            rect = null;


            lock (obj)
            {
                try
                {
                    if (ptrRect != IntPtr.Zero)
                    {
                        nFaceNum = ZFaceSdk.ZFace_Detect(pGrayBuf, nWidth, nHeight, ptrRect);
                        rect = new RECT[nFaceNum];
                        for (int i = 0; i < nFaceNum; i++)
                        {
                            rect[i] = new RECT();
                            rect[i] = (RECT)Marshal.PtrToStructure((IntPtr)(ptrRect.ToInt32() + i * Marshal.SizeOf(rect[i])), typeof(RECT));
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog("ZFace_ZFaceDetect（ZFaceDetect）检测人脸发生异常....." + e.ToString());
                }
            }
            if (nFaceNum == 0)
            {
            }
            return nFaceNum;

        }


        /// <summary>
        /// 通过图片字节流获取人脸数
        /// </summary>
        /// <param name="bytRgb8"></param>
        /// <param name="nWith"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public int CheckFaceNum(byte[] bytRgb8, int nWith, int nHeight, out RECT[] rect)
        {
            rect = new RECT[100];
            try
            {
                int nRet = 0;
                lock (obj1)
                {
                    nRet = FaceDetect(bytRgb8, nWith, nHeight, out rect);//检测到多少人                  
                }
                return nRet;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        int IFaceRecognizer.FaceDetect(byte[] pGrayBuf, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            rect = null;
            facePoint = null;
            return 0;
        }

        bool IFaceRecognizer.Face_GetFea(byte[] files, int nW, int nH, out byte[] pFea)
        {
            pFea = new byte[1];
            return false;
        }

        bool IFaceRecognizer.Face_GetFeaByFile(byte[] files, int nW, int nH, out byte[] pFea)
        {
            pFea = new byte[1];
            return false;
        }

        float IFaceRecognizer.Compare(byte[] pFea1, byte[] pFea2)
        {
            return 0;
        }

        public int Face_DetectByFile(byte[] files, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            rect = null;
            facePoint = null;
            return 0;
        }
    }
}

