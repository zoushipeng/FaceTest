using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public interface IGFaceRecognizer:IRecognizer
    {
        /// <summary>
        /// 检测人脸
        /// </summary>
        /// <param name="pGrayBuf">8位灰度图</param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <param name="facePoint"></param>
        /// <returns></returns>
        int GFaceDetect(byte[] pGrayBuf, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint);
        /// <summary>
        ///  提取人脸特征
        /// </summary>
        /// <param name="pRgb24"></param>
        /// <param name="pRgb8"></param>
        /// <param name="nW"></param>
        /// <param name="nH"></param>
        /// <param name="nThreadID"></param>
        /// <param name="pFea"></param>
        /// <returns></returns>
        bool GFace6_GetFea(byte[] pRgb24, byte[] pRgb8, int nW, int nH, int nThreadID, out byte[] pFea);

        /// <summary>
        /// 图片特征比对
        /// </summary>
        /// <param name="pFea1"></param>
        /// <param name="pFea2"></param>   
        /// <returns></returns>
        float Compare(byte[] pFea1, byte[] pFea2, int nThreadId = 0);
    }
}
