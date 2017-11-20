using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    /// <summary>
    /// 人脸检测返回模型
    /// </summary>
    public class FaceDetectInfo
    {
        public byte[] pRgb8Buf;
        public byte[] pRgb24Buf;
        public int nWidth;
        public int nHeight;
        public FaceRectInfo[] faceRect;
        public FacePointInfo[] facePoint;

    }
}
