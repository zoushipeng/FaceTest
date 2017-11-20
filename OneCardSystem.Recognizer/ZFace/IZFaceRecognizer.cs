using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public interface IZFaceRecognizer:IRecognizer
    {
        /// <summary>
        /// 通过8位灰度图获取人脸数
        /// </summary>
        /// <param name="bytRgb8"></param>
        /// <param name="nWith"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        int CheckFaceNum(byte[] bytRgb8, int nWith, int nHeight, out RECT[] rect);
        /// <summary>
        /// 比对图片
        /// </summary>
        /// <param name="strFile1"></param>
        /// <param name="strFile2"></param>
        /// <returns></returns>
        float Compare(string strFile1, string strFile2);
    }
}
