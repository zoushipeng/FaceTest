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
    public class ArcFaceRecognizer : IFaceRecognizer
    {
        bool inited = false;
        private static ArcFaceRecognizer instance;

        //人脸检测引擎
        IntPtr detectEngine = IntPtr.Zero;

        //人脸识别引擎
        IntPtr regcognizeEngine = IntPtr.Zero;

        IntPtr traceEngine = IntPtr.Zero;

        public static ArcFaceRecognizer GetInstance()
        {
            if (instance == null)
            {
                instance = new ArcFaceRecognizer();
            }
            return instance;
        }
        private ArcFaceRecognizer()
        {
            #region 初始化人脸引擎

            int detectSize = 40 * 1024 * 1024;

            IntPtr pMem = Marshal.AllocHGlobal(detectSize);

            string appId = "3CsdArm8tQewfTgepWVc7AQG7R8Y1EPREcbGuzXQZ4Ud";
            //1-n
            string sdkKey = "GuiDyH6k9serebZwz3sehJGHvmz1dNV8sx8KwxqPgz9J";

            int retCode = AmFaceVerify.AFD_FSDK_InitialFaceEngine(appId, sdkKey, pMem, detectSize, ref detectEngine, 5, 50, 1);

            int recognizeSize = 40 * 1024 * 1024;

            IntPtr pMemDetect = Marshal.AllocHGlobal(recognizeSize);

            //1-n
            string appIdDetect = "3CsdArm8tQewfTgepWVc7AQG7R8Y1EPREcbGuzXQZ4Ud";

            //1-n
            string sdkKeyDetect = "GuiDyH6k9serebZwz3sehJGnaP2kAuHjqzENtba3R9W5";

            //人脸识别引擎初始化
            retCode = AmFaceVerify.AFR_FSDK_InitialEngine(appIdDetect, sdkKeyDetect, pMemDetect, recognizeSize, ref regcognizeEngine);

            //初始化人脸跟踪引擎
            int traaceSize = 40 * 1024 * 1024;
            IntPtr pMemt = Marshal.AllocHGlobal(traaceSize);

            string appIdFt = "3CsdArm8tQewfTgepWVc7AQG7R8Y1EPREcbGuzXQZ4Ud";
            //1-n
            string sdkKeyFt = "GuiDyH6k9serebZwz3sehJGAmNitpNSnALxweG3TfNAC";

            //人脸跟踪引擎初始化
            retCode = AmFaceVerify.AFT_FSDK_InitialFaceEngine(appIdFt, sdkKeyFt, pMemt, traaceSize, ref traceEngine, 5, 16, 1);


            #endregion
        }
        ~ArcFaceRecognizer()
        {
        }

        public float Compare(byte[] pFea1, byte[] pFea2)
        {
            float similar = 0f;

            AFR_FSDK_FACEMODEL localFaceModels = new AFR_FSDK_FACEMODEL();

            IntPtr firstFeaturePtr = Marshal.AllocHGlobal(pFea1.Length);

            Marshal.Copy(pFea1, 0, firstFeaturePtr, pFea1.Length);

            localFaceModels.lFeatureSize = pFea1.Length;

            localFaceModels.pbFeature = firstFeaturePtr;

            IntPtr secondFeaturePtr = Marshal.AllocHGlobal(pFea2.Length);

            Marshal.Copy(pFea2, 0, secondFeaturePtr, pFea2.Length);

            AFR_FSDK_FACEMODEL localFaceModels2 = new AFR_FSDK_FACEMODEL();

            localFaceModels2.lFeatureSize = pFea2.Length;

            localFaceModels2.pbFeature = secondFeaturePtr;

            IntPtr firstPtr = Marshal.AllocHGlobal(Marshal.SizeOf(localFaceModels));

            Marshal.StructureToPtr(localFaceModels, firstPtr, false);

            IntPtr secondPtr = Marshal.AllocHGlobal(Marshal.SizeOf(localFaceModels2));

            Marshal.StructureToPtr(localFaceModels2, secondPtr, false);

            int result = AmFaceVerify.AFR_FSDK_FacePairMatching(regcognizeEngine, firstPtr, secondPtr, ref similar);

            localFaceModels = new AFR_FSDK_FACEMODEL();

            Marshal.FreeHGlobal(firstFeaturePtr);

            Marshal.FreeHGlobal(secondFeaturePtr);

            Marshal.FreeHGlobal(firstPtr);

            Marshal.FreeHGlobal(secondPtr);

            localFaceModels2 = new AFR_FSDK_FACEMODEL();
            return similar * 100;
        }

        public bool Init()
        {
            try
            {
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        public bool Uninit()
        {
            try
            {
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
                IntPtr version = AmFaceVerify.AFD_FSDK_GetVersion(detectEngine);

                return version;
            }
            catch
            { return (IntPtr)0; }
        }

        public bool CheckKey()
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        static object detectObject = new object();
        static object recongizeObject = new object();

        private byte[] getBGR(Bitmap image, ref int width, ref int height, ref int pitch)
        {
            //Bitmap image = new Bitmap(imgPath);

            const PixelFormat PixelFormat = PixelFormat.Format24bppRgb;

            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat);

            IntPtr ptr = data.Scan0;

            int ptr_len = data.Height * Math.Abs(data.Stride);

            byte[] ptr_bgr = new byte[ptr_len];

            Marshal.Copy(ptr, ptr_bgr, 0, ptr_len);

            width = data.Width;

            height = data.Height;

            pitch = Math.Abs(data.Stride);

            int line = width * 3;

            int bgr_len = line * height;

            byte[] bgr = new byte[bgr_len];

            for (int i = 0; i < height; ++i)
            {
                Array.Copy(ptr_bgr, i * pitch, bgr, i * line, line);
            }

            pitch = line;

            image.UnlockBits(data);

            return bgr;
        }

        //不带图片头
        public int FaceDetect(byte[] picFiles, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            int faceNum = 0;
            rect = new FaceRectInfo[1];
            facePoint = new FacePointInfo[1];

            try
            {

                Bitmap bitmap = getBitmap(picFiles, nWidth, nHeight);

                int width = 0;
                int height = 0;
                int pitch = 0;

                byte[] imageData = getBGR(bitmap, ref width, ref height, ref pitch);

                IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();

                offInput.u32PixelArrayFormat = 513;
                offInput.ppu8Plane = new IntPtr[4];
                offInput.ppu8Plane[0] = imageDataPtr;
                offInput.i32Width = width;
                offInput.i32Height = height;
                offInput.pi32Pitch = new int[4];
                offInput.pi32Pitch[0] = pitch;

                AFD_FSDK_FACERES faceRes = new AFD_FSDK_FACERES();

                IntPtr offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));

                Marshal.StructureToPtr(offInput, offInputPtr, false);

                IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));

                int detectResult;
                //人脸检测
                lock (detectObject)
                {
                    detectResult = AmFaceVerify.AFD_FSDK_StillImageFaceDetection(detectEngine, offInputPtr, ref faceResPtr);
                }
                if (detectResult == 0)
                {
                    object obj = Marshal.PtrToStructure(faceResPtr, typeof(AFD_FSDK_FACERES));
                    faceRes = (AFD_FSDK_FACERES) obj;

                    if (faceRes.nFace > 0)
                    {
                        RECT t = (RECT) Marshal.PtrToStructure(faceRes.rcFace + Marshal.SizeOf(typeof(RECT)) * 0,
                            typeof(RECT));
                        rect[0] = new FaceRectInfo()
                        {
                            rc = t,
                        };
                        faceNum = 1;
                    }
                }
                bitmap.Dispose();
                imageData = null;

                Marshal.FreeHGlobal(imageDataPtr);

                offInput = new ASVLOFFSCREEN();

                faceRes = new AFD_FSDK_FACERES();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("detect" + e.Message + "\n" + e.StackTrace);
            }
            return faceNum;
        }

        public bool Face_GetFea(byte[] pFiles, int nW, int nH, out byte[] pFea)
        {
            pFea = null;
            bool flag = false;
            try
            {
                Bitmap bitmap = getBitmap(pFiles, nW, nH);

                int width = 0;
                int height = 0;
                int pitch = 0;

                byte[] imageData = getBGR(bitmap, ref width, ref height, ref pitch);
                IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();

                offInput.u32PixelArrayFormat = 513;
                offInput.ppu8Plane = new IntPtr[4];
                offInput.ppu8Plane[0] = imageDataPtr;
                offInput.i32Width = width;
                offInput.i32Height = height;
                offInput.pi32Pitch = new int[4];
                offInput.pi32Pitch[0] = pitch;

                AFD_FSDK_FACERES faceRes = new AFD_FSDK_FACERES();

                IntPtr offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));

                Marshal.StructureToPtr(offInput, offInputPtr, false);

                IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));

                int detectResult;
                //人脸检测
                lock (detectObject)
                {
                    detectResult = AmFaceVerify.AFD_FSDK_StillImageFaceDetection(detectEngine, offInputPtr, ref faceResPtr);
                }
                if (detectResult == 0)
                {
                    object obj = Marshal.PtrToStructure(faceResPtr, typeof(AFD_FSDK_FACERES));
                    faceRes = (AFD_FSDK_FACERES)obj;

                    if (faceRes.nFace > 0)
                    {
                        AFR_FSDK_FACEINPUT faceResult = new AFR_FSDK_FACEINPUT();

                        int orient = (int)Marshal.PtrToStructure(faceRes.lfaceOrient, typeof(int));

                        faceResult.lfaceOrient = orient;

                        faceResult.rcFace = new RECT();

                        RECT rect = (RECT)Marshal.PtrToStructure(faceRes.rcFace, typeof(RECT));

                        faceResult.rcFace = rect;

                        IntPtr faceResultPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceResult));

                        Marshal.StructureToPtr(faceResult, faceResultPtr, false);

                        AFR_FSDK_FACEMODEL localFaceModels = new AFR_FSDK_FACEMODEL();

                        IntPtr localFaceModelsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(localFaceModels));
                        int extractResult;
                        lock (recongizeObject)
                        {
                            extractResult = AmFaceVerify.AFR_FSDK_ExtractFRFeature(regcognizeEngine, offInputPtr, faceResultPtr, localFaceModelsPtr);
                        }
                        if (extractResult == 0)
                        {
                            flag = true;
                        }

                        Marshal.FreeHGlobal(faceResultPtr);

                        Marshal.FreeHGlobal(offInputPtr);

                        object objFeature = Marshal.PtrToStructure(localFaceModelsPtr, typeof(AFR_FSDK_FACEMODEL));

                        Marshal.FreeHGlobal(localFaceModelsPtr);

                        localFaceModels = (AFR_FSDK_FACEMODEL)objFeature;

                        pFea = new byte[localFaceModels.lFeatureSize];

                        Marshal.Copy(localFaceModels.pbFeature, pFea, 0, localFaceModels.lFeatureSize);
                        
                    }
                }
                bitmap.Dispose();
                imageData = null;

                Marshal.FreeHGlobal(imageDataPtr);

                offInput = new ASVLOFFSCREEN();

                faceRes = new AFD_FSDK_FACERES();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("get_fea" + e.Message + "\n" + e.StackTrace);
            }
            return flag;
        }

        //带图片头
        public bool Face_GetFeaByFile(byte[] pFiles, int nW, int nH, out byte[] pFea)
        {
            pFea = null;
            bool flag = false;
            try
            {
                Bitmap readBitmap = new Bitmap(new MemoryStream(pFiles));


                int width = 0;
                int height = 0;
                int pitch = 0;

                byte[] imageData = getBGR(readBitmap, ref width, ref height, ref pitch);
                IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();

                offInput.u32PixelArrayFormat = 513;
                offInput.ppu8Plane = new IntPtr[4];
                offInput.ppu8Plane[0] = imageDataPtr;
                offInput.i32Width = width;
                offInput.i32Height = height;
                offInput.pi32Pitch = new int[4];
                offInput.pi32Pitch[0] = pitch;

                AFD_FSDK_FACERES faceRes = new AFD_FSDK_FACERES();

                IntPtr offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));

                Marshal.StructureToPtr(offInput, offInputPtr, false);

                IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));

                int detectResult;
                //人脸检测
                lock (detectObject)
                {
                    detectResult = AmFaceVerify.AFD_FSDK_StillImageFaceDetection(detectEngine, offInputPtr, ref faceResPtr);
                }
                if (detectResult == 0)
                {
                    object obj = Marshal.PtrToStructure(faceResPtr, typeof(AFD_FSDK_FACERES));
                    faceRes = (AFD_FSDK_FACERES)obj;

                    if (faceRes.nFace > 0)
                    {
                        AFR_FSDK_FACEINPUT faceResult = new AFR_FSDK_FACEINPUT();

                        int orient = (int)Marshal.PtrToStructure(faceRes.lfaceOrient, typeof(int));

                        faceResult.lfaceOrient = orient;

                        faceResult.rcFace = new RECT();

                        RECT rect = (RECT)Marshal.PtrToStructure(faceRes.rcFace, typeof(RECT));

                        faceResult.rcFace = rect;

                        IntPtr faceResultPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceResult));

                        Marshal.StructureToPtr(faceResult, faceResultPtr, false);

                        AFR_FSDK_FACEMODEL localFaceModels = new AFR_FSDK_FACEMODEL();

                        IntPtr localFaceModelsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(localFaceModels));

                        int extractResult;
                        lock (recongizeObject)
                        {
                            extractResult = AmFaceVerify.AFR_FSDK_ExtractFRFeature(regcognizeEngine, offInputPtr, faceResultPtr, localFaceModelsPtr);
                        }
                        if (extractResult == 0)
                        {
                            flag = true;
                        }

                        Marshal.FreeHGlobal(faceResultPtr);

                        Marshal.FreeHGlobal(offInputPtr);

                        object objFeature = Marshal.PtrToStructure(localFaceModelsPtr, typeof(AFR_FSDK_FACEMODEL));

                        Marshal.FreeHGlobal(localFaceModelsPtr);

                        localFaceModels = (AFR_FSDK_FACEMODEL)objFeature;

                        pFea = new byte[localFaceModels.lFeatureSize];

                        Marshal.Copy(localFaceModels.pbFeature, pFea, 0, localFaceModels.lFeatureSize);

                    }
                }
                readBitmap.Dispose();
                imageData = null;

                Marshal.FreeHGlobal(imageDataPtr);

                offInput = new ASVLOFFSCREEN();

                faceRes = new AFD_FSDK_FACERES();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("get_fea_file" + e.Message + "\n" + e.StackTrace);
            }
            return flag;
        }

        private Bitmap getBitmap(byte[] rgbFrame, int width, int height)
        {
            int bytePerLine = width * 3;
            MemoryStream fs = new MemoryStream();

            BinaryWriter bw = new BinaryWriter(fs);
            {
                bw.Write('B');
                bw.Write('M');
                bw.Write(bytePerLine * height + 54);
                bw.Write(0);
                bw.Write(54);
                bw.Write(40);
                bw.Write(width);
                bw.Write(height);
                bw.Write((ushort)1);
                bw.Write((ushort)24);
                bw.Write(0);
                bw.Write(bytePerLine * height);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);

                byte[] data = new byte[bytePerLine * height];
                int gIndex = width * height;
                int bIndex = gIndex * 2;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0, i = 0; x < width; x++)
                    {
                        data[y * bytePerLine + x * 3] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 0];    //RGB
                        data[y * bytePerLine + x * 3 + 1] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 1];
                        data[y * bytePerLine + x * 3 + 2] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 2];
                    }
                }
                bw.Write(data, 0, data.Length);
                bw.Flush();
            }
            Bitmap bmp = new Bitmap(fs);
            
            bw.Close();
            fs.Close();

            return bmp;
        }

        private Bitmap getBitmapNoaddHead(byte[] rgbFrame, int width, int height)
        {
            int bytePerLine = width * 3;
            MemoryStream fs = new MemoryStream();

            BinaryWriter bw = new BinaryWriter(fs);
            {
                byte[] data = new byte[bytePerLine * height];
                int gIndex = width * height;
                int bIndex = gIndex * 2;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0, i = 0; x < width; x++)
                    {
                        data[y * bytePerLine + x * 3] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 0];    //RGB
                        data[y * bytePerLine + x * 3 + 1] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 1];
                        data[y * bytePerLine + x * 3 + 2] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 2];
                    }
                }
                bw.Write(data, 0, data.Length);
                bw.Flush();
            }
            Bitmap bmp = new Bitmap(fs);

            bw.Close();
            fs.Close();

            return bmp;
        }

        //带图片头
        public int Face_DetectByFile(byte[] files, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint)
        {
            int faceNum = 0;
            rect = new FaceRectInfo[1];
            facePoint = new FacePointInfo[1];

            try
            {

                Bitmap bitmap = new Bitmap(new MemoryStream(files));

                int width = 0;
                int height = 0;
                int pitch = 0;

                byte[] imageData = getBGR(bitmap, ref width, ref height, ref pitch);

                IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();

                offInput.u32PixelArrayFormat = 513;
                offInput.ppu8Plane = new IntPtr[4];
                offInput.ppu8Plane[0] = imageDataPtr;
                offInput.i32Width = width;
                offInput.i32Height = height;
                offInput.pi32Pitch = new int[4];
                offInput.pi32Pitch[0] = pitch;

                AFD_FSDK_FACERES faceRes = new AFD_FSDK_FACERES();

                IntPtr offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));

                Marshal.StructureToPtr(offInput, offInputPtr, false);

                IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));

                int detectResult;
                //人脸检测
                lock (detectObject)
                {
                    detectResult = AmFaceVerify.AFD_FSDK_StillImageFaceDetection(detectEngine, offInputPtr, ref faceResPtr);
                }
                if (detectResult == 0)
                {
                    object obj = Marshal.PtrToStructure(faceResPtr, typeof(AFD_FSDK_FACERES));
                    faceRes = (AFD_FSDK_FACERES)obj;

                    if (faceRes.nFace > 0)
                    {
                        RECT t = (RECT)Marshal.PtrToStructure(faceRes.rcFace + Marshal.SizeOf(typeof(RECT)) * 0,
                            typeof(RECT));
                        rect[0] = new FaceRectInfo()
                        {
                            rc = t,
                        };
                        faceNum = 1;
                    }
                }
                bitmap.Dispose();
                imageData = null;

                Marshal.FreeHGlobal(imageDataPtr);

                offInput = new ASVLOFFSCREEN();

                faceRes = new AFD_FSDK_FACERES();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("detect" + e.Message + "\n" + e.StackTrace);
            }
            return faceNum;
        }
    }
}