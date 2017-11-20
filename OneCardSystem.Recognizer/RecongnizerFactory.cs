using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public class RecongnizerFactory
    {
        private static IFaceRecognizer recognizer=null;
        public static IFaceRecognizer GetRecognizer(string strType)
        {
            switch (strType)
            {
                case "ZFace":
                    recognizer = ZFaceRecognizer.GetInstance();
                    break;
                case "GFace":
                    recognizer = GFaceRecognizer.GetInstance();
                    break;
                case "GFace7":
                    recognizer = GFace7Recognizer.GetInstance();
                    break;
                //case "XLFace":
                //    recognizer =XLFaceRecognizer.GetInstance();
                //    break;
                case "ArcFace":
                    recognizer = ArcFaceRecognizer.GetInstance();
                    break;
                default:
                    recognizer = GFaceRecognizer.GetInstance();
                    break;
            }
            return recognizer;
        }
    }
}
