using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    /// <summary>
    /// 人脸识别算法接口
    /// </summary>
    public interface IFaceRecognizer:IRecognizer
    {
        /// <summary>
        /// RGB检测人脸
        /// </summary>
        /// <param name="pGrayBuf">RGB图像数据</param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <param name="facePoint"></param>
        /// <returns></returns>
        int FaceDetect(byte[] pGrayBuf, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint);
        /// <summary>
        ///  RGB提取人脸特征
        /// </summary>
        /// <param name="files">RGB图像数据</param>      
        /// <param name="nW"></param>
        /// <param name="nH"></param>    
        /// <param name="pFea"></param>
        /// <returns></returns>
        bool Face_GetFea(byte[] files, int nW, int nH, out byte[] pFea);

        /// <summary>
        /// 直接通过图片检测人脸框
        /// </summary>
        /// <param name="files"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <param name="facePoint"></param>
        /// <returns></returns>
        int Face_DetectByFile(byte[] files, int nWidth, int nHeight, out FaceRectInfo[] rect, out FacePointInfo[] facePoint);
        /// <summary>
        /// 直接提取图片的特征
        /// </summary>
        /// <param name="files"></param>
        /// <param name="nW"></param>
        /// <param name="nH"></param>
        /// <param name="pFea"></param>
        /// <returns></returns>
        bool Face_GetFeaByFile(byte[] files, int nW, int nH, out byte[] pFea);

        /// <summary>
        /// 图片特征比对
        /// </summary>
        /// <param name="pFea1"></param>
        /// <param name="pFea2"></param>   
        /// <returns></returns>
        float Compare(byte[] pFea1, byte[] pFea2);
    }
}
