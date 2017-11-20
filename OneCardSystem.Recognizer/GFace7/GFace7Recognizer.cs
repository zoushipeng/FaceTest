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
    public class GFace7Recognizer : IFaceRecognizer
    {
        bool inited = false;
        private static GFace7Recognizer instance;

        static object obj = new object();
        static object objFea = new object();

        static object lockerDetector = new object();

        public static GFace7Recognizer GetInstance()
        {
            if (instance == null)
            {
                instance = new GFace7Recognizer();
            }
            return instance;
        }
        private GFace7Recognizer()
        {
            ptrFea = Marshal.AllocHGlobal(10240 * 10240);
            ptrRect = Marshal.AllocHGlobal(10240 * 10240);
            ptrFaceRect = Marshal.AllocHGlobal(10240 * 10240);
        }
        ~GFace7Recognizer()
        {
        }

        public float Compare(byte[] pFea1, byte[] pFea2)
        {
            float result = 0;
            uint nThreadID = 0;
            try
            {
                //LogHelper.WriteLog("（Compare）特征比对开始.....");
                result = GFace7Sdk.GFace7_Compare(nThreadID, pFea1, pFea2);
                //LogHelper.WriteLog(string.Format("（Compare）特征比对结束.....结果:{0}",result.ToString()));
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public bool Init()
        {
            try
            {
                if (!inited)
                {
                    inited = GFace7Sdk.GFace7_Init("TS_G7");
                }
                return inited;
            }
            catch (Exception err)
            {
                LogHelper.WriteLog(err.ToString());
                return false;
            }
        }

        public bool Uninit()
        {
            try
            {
                GFace7Sdk.GFace7_Uninit();
                
                Marshal.FreeHGlobal(pbuf);
                Marshal.FreeHGlobal(ptemprect);

                inited = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IntPtr GetVersion()
        {
            try
            {
                string version = GFace7Sdk.GFace7_GetVer();
                return (IntPtr)0;
            }
            catch
            { return (IntPtr)0; }
        }

        public bool CheckKey()
        {
            try
            {
                return GFace7Sdk.GFace7_CheckKey();
            }
            catch(Exception e)
            {
                LogHelper.WriteLog(e.ToString());
                return false;
            }
        }

        IntPtr ptrRect = IntPtr.Zero;
        IntPtr ptrFaceRect = IntPtr.Zero;
        public int FaceDetect(byte[] picFiles, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            //RGB数据添加图片头，返回的是带54字节的完整图片
            byte[] pBuf = PicGrayProcess.PicHeadAddBytes(picFiles, ref nWidth, ref nHeight);
            return Face_DetectByFile(pBuf, nWidth, nHeight, out rect, out facePoint);

        }

        static object objlock = new object();
        public bool Face_GetFea(byte[] pFiles, int nW, int nH, out byte[] pFea)
        {
            pFea = null;
            bool flag = false;

            LogHelper.WriteLog("提取头像特征sssss!");
            //lock (objlock)
            {
                flag = GFace_GetFea(pFiles, nW, nH, 0, out pFea);
                if (flag)
                {

                }
            }
            LogHelper.WriteLog("提取头像特征eeeee!");
            return flag;
        }


        IntPtr ptrFea = IntPtr.Zero;
        private bool GFace_GetFea(byte[] picFiles, int nW, int nH, int nThreadID, out byte[] pFea)
        {
            //LogHelper.WriteLog("（GetFea）获取特征码开始.....");
            pFea = new byte[1024];
            FaceRectInfo[] FaceRect = new FaceRectInfo[100];
            FacePointInfo[] rectPoint = new FacePointInfo[100];
            bool flag = false;

            //lock (objFea)
            {

                #region
                //byte[] pRgb24 = PicGrayProcess.ConvertByImgRgb24(picFiles, ref nW, ref nH);
                //RGB数据添加图片头，返回的是带54字节的完整图片
                byte[] pBuf = PicGrayProcess.PicHeadAddBytes(picFiles, ref nW, ref nH);
                byte[] pRgb24 = convertByImgRgb24(pBuf, ref nW, ref nH);

                if (pRgb24 == null || pRgb24.Count() <= 0)
                {
                    LogHelper.WriteLog("提取失败!");
                }

                #endregion              
                FacePointInfo[] faceTempPoint = new FacePointInfo[1];
                try
                {
                    //int nRet = FaceDetect(picFiles, nW, nH, out FaceRect, out rectPoint);//检测到多少人 
                    int nRet = Face_DetectByFile(pBuf, nW, nH, out FaceRect, out rectPoint);//检测到多少人 
                    if (nRet == 0)
                    {
                        return false;
                    }
                    int max = 0;
                    int m = 0;
                    FacePointInfo maxFacePointInfo = new FacePointInfo();
                    for (int i = 0; i < nRet; i++)
                    {
                        m = rectPoint[i].ptEyeRight.x - rectPoint[i].ptEyeLeft.x;
                        if (m > max)
                        {
                            maxFacePointInfo = rectPoint[i];
                            max = m;
                        }
                    }
                    faceTempPoint[0] = maxFacePointInfo;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("GFace7_GetFea（GetFea）获取特征码结束，发生异常....." + ex.ToString());
                    return false;
                }

                try
                {
                    if (ptrFea == IntPtr.Zero)
                    {
                        LogHelper.WriteLog("GFace7_GetFea（GetFea）申请到的内存为空指针.....");
                    }
                    else
                    {
                        lock (objFea)
                        {
                            flag = GFace7Sdk.GFace7_GetFea(0, pRgb24, nW, nH, 1, faceTempPoint, ptrFea);

                        }
                        if (flag)
                        {
                            pFea = new byte[1024 * 8];
                            Marshal.Copy(ptrFea, pFea, 0, pFea.Length);
                        }
                    }
                    //LogHelper.WriteLog("（GetFea）获取特征码结束.....");
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;

        }

        public bool Face_GetFeaByFile(byte[] files, int nW, int nH, out byte[] pPea)
        {
            pPea = new byte[1];
            nW = 0;
            nH = 0;
            try
            {
                byte[] pRgb24 = convertByImgRgb24(files, ref nW, ref nH);

                byte[] pRgb8 = convertByImgRgb8(files, ref nW, ref nH);
                if (pRgb8 == null || pRgb8.Count() <= 0 || pRgb24 == null || pRgb24.Count() <= 0)
                {
                    LogHelper.WriteLog("转换图片为灰度图像失败!");
                }
                LogHelper.WriteLog("提取图片特征sssss!");
                //lock (objlock)
                {
                    if (GFace7_GetFeaCerti(pRgb24, pRgb8, nW, nH, 0, out pPea))
                    {
                        LogHelper.WriteLog("提取图片特征eeeee!");
                        return true;
                    }
                }


                return false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("提取图片特征失败!详细信息:" + ex.StackTrace + "\r\n" + ex.ToString());
            }
            return false;
        }


        IntPtr pbuffer = Marshal.AllocHGlobal(10240 * 10240);
        static object objRgb = new object();
        private byte[] convertByImgRgb24(byte[] files, ref int nWidth, ref int nHeight)
        {
            byte[] pRgb24 = null;
            try
            {
                if (files != null && pbuffer != IntPtr.Zero)
                {
                    //lock (objRgb)
                    {
                        ImageGrayscaleSdk.GetRgb24FromBuffer(files, files.Length, pbuffer, ref nWidth, ref nHeight);
                    }
                    pRgb24 = new byte[nWidth * nHeight * 3];
                    Marshal.Copy(pbuffer, pRgb24, 0, nWidth * nHeight * 3);
                }
                else
                {
                    LogHelper.WriteLog("转换图片为灰度24位数组,图片数据为空，或者申请的指针为空指针");
                }
            }
            catch (Exception ex)
            {
                pRgb24 = null;
                LogHelper.WriteLog("转换图片为灰度24位数组,调用 ImageGrayscaleSdk :" + ex.StackTrace + "\r\n" + ex.ToString());
            }
            finally
            {
            }
            return pRgb24;
        }

        private byte[] convertByImgRgb8(byte[] files, ref int nWidth, ref int nHeight)
        {
            byte[] bytRgb8 = null;

            try
            {
                nWidth = 1;
                nHeight = 1;
                if (files != null && pbuffer != IntPtr.Zero)
                {
                    //lock (objRgb)
                    {
                        ImageGrayscaleSdk.GetRgb8FromBuffer(files, files.Length, pbuffer, ref nWidth, ref nHeight);
                    }
                    bytRgb8 = new byte[nWidth * nHeight];
                    Marshal.Copy(pbuffer, bytRgb8, 0, nWidth * nHeight);
                }
                else
                {
                    LogHelper.WriteLog("转换图片为灰度8位数组,图片数据为空，或者申请的指针为空指针");
                }
            }
            catch (Exception ex)
            {
                bytRgb8 = null;
                LogHelper.WriteLog("转换图片为灰度8位数组!详细信息，调用 ImageGrayscaleSdk :" + ex.StackTrace + "\r\n" + ex.ToString());
            }
            finally
            {
            }
            return bytRgb8;
        }


        private byte[] convertByImgRgb8(string files, ref int nWidth, ref int nHeight)
        {
            byte[] bytRgb8 = null;
            nWidth = 1;
            nHeight = 1;
            try
            {
                if (files != null && pbuffer != IntPtr.Zero)
                {
                    ImageGrayscaleSdk.GetRgb8ByName(files, pbuffer, ref nWidth, ref nHeight);
                    bytRgb8 = new byte[nWidth * nHeight];
                    Marshal.Copy(pbuffer, bytRgb8, 0, nWidth * nHeight);
                }
                else
                {
                    LogHelper.WriteLog("转换图片为灰度8位数组,图片数据为空，或者申请的指针为空指针");
                }
            }
            catch (Exception ex)
            {
                bytRgb8 = null;
                LogHelper.WriteLog("转换图片为灰度8位数组! 调用 ImageGrayscaleSdk :" + ex.StackTrace + "\r\n" + ex.ToString());
            }
            finally
            {
            }
            return bytRgb8;
        }


        private byte[] convertByImgRgb24(string files, ref int nWidth, ref int nHeight)
        {
            byte[] pRgb24 = null;
            try
            {
                if (files != null && pbuffer != IntPtr.Zero)
                {
                    ImageGrayscaleSdk.GetRgb24ByName(files, pbuffer, ref nWidth, ref nHeight);
                    pRgb24 = new byte[nWidth * nHeight * 3];
                    Marshal.Copy(pbuffer, pRgb24, 0, nWidth * nHeight * 3);
                }
                else
                {
                    LogHelper.WriteLog("转换图片为灰度24位数组,图片数据为空，或者申请的指针为空指针");
                }
            }
            catch (Exception ex)
            {
                pRgb24 = null;
                LogHelper.WriteLog("转换图片为灰度24位数组,调用 ImageGrayscaleSdk :" + ex.StackTrace + "\r\n" + ex.ToString());
            }
            finally
            {
            }
            return pRgb24;
        }

        private bool GFace7_GetFeaCerti(byte[] pRgb24, byte[] pRgb8, int nW, int nH, int nThreadID, out byte[] pFea)
        {
            //LogHelper.WriteLog("（GetFea）获取特征码开始.....");
            pFea = new byte[1024];
            FaceRectInfo[] FaceRect = new FaceRectInfo[100];
            FacePointInfo[] rectPoint = new FacePointInfo[100];
            bool flag = false;

            //lock (objFea)
            {
                FacePointInfo[] faceTempPoint = new FacePointInfo[1];
                try
                {
                    int nRet = GFaceDetectCerti(pRgb8, nW, nH, out FaceRect, out rectPoint);//检测到多少人 
                    if (nRet == 0)
                    {
                        return false;
                    }
                    int max = 0;
                    int m = 0;
                    FacePointInfo maxFacePointInfo = new FacePointInfo();
                    for (int i = 0; i < nRet; i++)
                    {
                        m = rectPoint[i].ptEyeRight.x - rectPoint[i].ptEyeLeft.x;
                        if (m > max)
                        {
                            maxFacePointInfo = rectPoint[i];
                            max = m;
                        }
                    }
                    faceTempPoint[0] = maxFacePointInfo;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("GFace7_GetFea（GetFea）获取特征码结束，发生异常....." + ex.ToString());
                    return false;
                }

                try
                {
                    if (ptrFea == IntPtr.Zero)
                    {
                        LogHelper.WriteLog("GFace7_GetFea（GetFea）申请到的内存为空指针.....");
                    }
                    else
                    {
                        lock (objFea)
                        {
                            flag = GFace7Sdk.GFace7_GetFea(0, pRgb24, nW, nH, 1, faceTempPoint, ptrFea);

                        }
                        if (flag)
                        {
                            pFea = new byte[1024 * 8];
                            Marshal.Copy(ptrFea, pFea, 0, pFea.Length);
                        }
                    }
                    //LogHelper.WriteLog("（GetFea）获取特征码结束.....");
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;

        }

        private int GFaceDetectCerti(byte[] pGrayBuf, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            //lock (objFea)
            {
                rect = new FaceRectInfo[1];
                facePoint = new FacePointInfo[1];
                int nFaceNum = 0;
                try
                {
                    //LogHelper.WriteLog("（Detect）检测人脸开始.....");
                    lock (objFea)
                    {
                        nFaceNum = GFace7Sdk.GFace7_Detect(0, pGrayBuf, nWidth, nHeight, ptrRect, ptrFaceRect);
                    }
                    rect = new FaceRectInfo[nFaceNum];
                    facePoint = new FacePointInfo[nFaceNum];
                    for (int i = 0; i < nFaceNum; i++)
                    {
                        rect[i] = new FaceRectInfo();
                        rect[i] = (FaceRectInfo)Marshal.PtrToStructure((IntPtr)(ptrRect.ToInt32() + i * Marshal.SizeOf(rect[i])), typeof(FaceRectInfo));
                    }

                    for (int i = 0; i < nFaceNum; i++)
                    {
                        facePoint[i] = new FacePointInfo();
                        facePoint[i] = (FacePointInfo)Marshal.PtrToStructure((IntPtr)(ptrFaceRect.ToInt32() + i * Marshal.SizeOf(facePoint[i])), typeof(FacePointInfo));
                    }

                    #region
                    int max = 0;
                    int m = 0;
                    int index = 0;
                    FacePointInfo maxFacePointInfo = new FacePointInfo();
                    for (int i = 0; i < nFaceNum; i++)
                    {
                        m = facePoint[i].ptEyeRight.x - facePoint[i].ptEyeLeft.x;
                        if (m > max)
                        {
                            maxFacePointInfo = facePoint[i];
                            max = m;
                            index = i;
                        }
                    }
                    FacePointInfo[] faceTempPoint = new FacePointInfo[1];
                    faceTempPoint[0] = maxFacePointInfo;

                    facePoint = faceTempPoint;
                    nFaceNum = 1;

                    FaceRectInfo[] rectTempPoint = new FaceRectInfo[1];
                    rectTempPoint[0] = rect[index];
                    rect = rectTempPoint;
                    #endregion`
                }
                catch
                {
                    rect = new FaceRectInfo[1];
                    facePoint = new FacePointInfo[1];
                    nFaceNum = 0;
                }
                finally
                {
                }
                return nFaceNum;
            }
        }


        RECT[] temprect = new RECT[100];
        IntPtr ptemprect = Marshal.AllocHGlobal(16 * 100);
        IntPtr pbuf = Marshal.AllocHGlobal(1920 * 1080);

        public int Face_DetectByFile(byte[] files, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            //lock (objFea)
            {
                rect = new FaceRectInfo[1];
                facePoint = new FacePointInfo[1];
                int nFaceNum = 0;
                try
                {

                    //LogHelper.WriteLog("（Detect）检测人脸开始.....");
                    //通过完整图片文件取到8位灰度图
                    byte[] pRgb8 = convertByImgRgb8(files, ref nWidth, ref nHeight);

                    //added by frank
                    //Marshal.Copy(pRgb8, 0, pbuf, pRgb8.Length);
                    //nFaceNum = GFaceSdk.detectface(pbuf, nWidth, nHeight, ptemprect);

                    //rect = new FaceRectInfo[nFaceNum];
                    //facePoint = new FacePointInfo[nFaceNum];
                    //for (int i = 0; i < nFaceNum; i++)
                    //{
                    //    rect[i] = new FaceRectInfo();

                    //    RECT r = (RECT)Marshal.PtrToStructure((IntPtr)(ptemprect.ToInt32() + i * Marshal.SizeOf(temprect[i])), typeof(RECT));
                    //    rect[i].rc.left = r.left;
                    //    rect[i].rc.top = r.top;
                    //    rect[i].rc.right = r.left + r.right;                        
                    //    rect[i].rc.bottom = r.top + r.bottom;
                    //}


                    lock (objFea)
                    {
                        nFaceNum = GFace7Sdk.GFace7_Detect(0, pRgb8, nWidth, nHeight, ptrRect, ptrFaceRect);
                    }
                    rect = new FaceRectInfo[nFaceNum];
                    facePoint = new FacePointInfo[nFaceNum];
                    for (int i = 0; i < nFaceNum; i++)
                    {
                        rect[i] = new FaceRectInfo();
                        rect[i] = (FaceRectInfo)Marshal.PtrToStructure((IntPtr)(ptrRect.ToInt32() + i * Marshal.SizeOf(rect[i])), typeof(FaceRectInfo));
                    }

                    for (int i = 0; i < nFaceNum; i++)
                    {
                        facePoint[i] = new FacePointInfo();
                        facePoint[i] = (FacePointInfo)Marshal.PtrToStructure((IntPtr)(ptrFaceRect.ToInt32() + i * Marshal.SizeOf(facePoint[i])), typeof(FacePointInfo));
                    }

                    #region
                    int max = 0;
                    int m = 0;
                    int index = 0;
                    FacePointInfo maxFacePointInfo = new FacePointInfo();
                    for (int i = 0; i < nFaceNum; i++)
                    {
                        m = facePoint[i].ptEyeRight.x - facePoint[i].ptEyeLeft.x;
                        if (m > max)
                        {
                            maxFacePointInfo = facePoint[i];
                            max = m;
                            index = i;
                        }
                    }
                    FacePointInfo[] faceTempPoint = new FacePointInfo[1];
                    faceTempPoint[0] = maxFacePointInfo;

                    facePoint = faceTempPoint;
                    nFaceNum = 1;

                    FaceRectInfo[] rectTempPoint = new FaceRectInfo[1];
                    rectTempPoint[0] = rect[index];
                    rect = rectTempPoint;
                    #endregion`
                }
                catch (Exception ex)
                {
                    rect = new FaceRectInfo[1];
                    facePoint = new FacePointInfo[1];
                    nFaceNum = 0;
                }
                finally
                {
                }
                return nFaceNum;
            }
        }
    }
}
