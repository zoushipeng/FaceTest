using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public class PicGrayProcess
    {
        #region
        static IntPtr pbuffer = Marshal.AllocHGlobal(10240 * 10240);
        static object objRgb = new object();
        public static byte[] PicHeadAddBytes(byte[] picRgb, ref int nWidth, ref int nHeight)
        {
            //byte[] bytRgb8 = null;
            byte[] files = null;

            lock (objRgb)
            {
                #region
                int width = nWidth;
                int height = nHeight;
                int bytePerLine = width * 3;
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write('B');
                    bw.Write('M');
                    bw.Write(picRgb.Length + 54);
                    bw.Write(0);
                    bw.Write(54);
                    bw.Write(40);
                    bw.Write(nWidth);
                    bw.Write(nHeight);
                    bw.Write((ushort)1);
                    bw.Write((ushort)24);
                    bw.Write(0);
                    bw.Write(bytePerLine * height);
                    bw.Write(0);
                    bw.Write(0);
                    bw.Write(0);
                    bw.Write(0);

                    //byte[] data = picfiles;
                    //bw.Write(data, 0, data.Length);
                    //bw.Flush();


                    byte[] data = new byte[bytePerLine * height];
                    int gIndex = width * height;
                    int bIndex = gIndex * 2;

                    for (int y = height - 1; y >= 0; y--)
                    {
                        for (int x = 0, i = 0; x < width; x++)
                        {
                            data[y * bytePerLine + x * 3] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 2];    //RGB
                            data[y * bytePerLine + x * 3 + 1] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 1];
                            data[y * bytePerLine + x * 3 + 2] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 0];
                        }
                    }
                    bw.Write(data, 0, data.Length);
                    bw.Flush();
                }
                files = ms.ToArray();
                #endregion

            }
            return files;
        }

        #endregion


        public static byte[] ConvertByImgRgb24(byte[] picRgb, ref int nWidth, ref int nHeight)
        {
            byte[] pRgb24 = null;
            byte[] files = null;
            try
            {
                lock (objRgb)
                {
                    #region
                    int width = nWidth;
                    int height = nHeight;
                    int bytePerLine = width * 3;
                    MemoryStream ms = new MemoryStream();
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write('B');
                        bw.Write('M');
                        bw.Write(picRgb.Length + 54);
                        bw.Write(0);
                        bw.Write(54);
                        bw.Write(40);
                        bw.Write(nWidth);
                        bw.Write(nHeight);
                        bw.Write((ushort)1);
                        bw.Write((ushort)24);
                        bw.Write(0);
                        bw.Write(bytePerLine * height);
                        bw.Write(0);
                        bw.Write(0);
                        bw.Write(0);
                        bw.Write(0);

                        //byte[] data = picfiles;
                        //bw.Write(data, 0, data.Length);
                        //bw.Flush();


                        byte[] data = new byte[bytePerLine * height];
                        int gIndex = width * height;
                        int bIndex = gIndex * 2;

                        for (int y = height - 1; y >= 0; y--)
                        {
                            for (int x = 0, i = 0; x < width; x++)
                            {
                                data[y * bytePerLine + x * 3] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 2];    //RGB
                                data[y * bytePerLine + x * 3 + 1] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 1];
                                data[y * bytePerLine + x * 3 + 2] = picRgb[(height - y - 1) * bytePerLine + x * 3 + 0];
                            }
                        }
                        bw.Write(data, 0, data.Length);
                        bw.Flush();
                    }
                    files = ms.ToArray();
                    #endregion


                    if (files != null && pbuffer != IntPtr.Zero)
                    {

                        ImageGrayscaleSdk.GetRgb24FromBuffer(files, files.Length, pbuffer, ref nWidth, ref nHeight);

                        pRgb24 = new byte[nWidth * nHeight * 3];
                        Marshal.Copy(pbuffer, pRgb24, 0, nWidth * nHeight * 3);
                    }
                    else
                    {
                        LogHelper.WriteLog("转换图片为灰度24位数组,图片数据为空，或者申请的指针为空指针");
                    }
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
    }
}
