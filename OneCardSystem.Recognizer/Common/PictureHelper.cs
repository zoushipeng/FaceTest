using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public class PictureHelper
    {
        /// <summary>
        /// 将转换后的 RGB 图像数据按照 BMP 格式写入文件。
        /// </summary>
        /// <param name="rgbFrame">RGB 格式图像数据。</param>
        /// <param name="width">图像宽（单位：像素）。</param>
        /// <param name="height">图像高（单位：像素）。</param>
        /// <param name="bmpFile"> BMP 文件名。</param>
        public void WriteBMP(byte[] rgbFrame, int width, int height, string bmpFile,string basePath)
        {
            // 写 BMP 图像文件。         
            try
            {
                string temp = basePath+"tmp.bmp";
                int bytePerLine = width * 3;
                using (FileStream fs = File.Open(temp, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
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
                                data[y * bytePerLine + x * 3] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 2];    //RGB
                                data[y * bytePerLine + x * 3 + 1] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 1];
                                data[y * bytePerLine + x * 3 + 2] = rgbFrame[(height - y - 1) * bytePerLine + x * 3 + 0];
                            }
                        }
                        bw.Write(data, 0, data.Length);
                        bw.Flush();                     
                    }
                }
                Bitmap bmp = new Bitmap(temp);
                bmp.Save(bmpFile, ImageFormat.Jpeg);
                bmp.Dispose();
                File.Delete(temp);
            }
            catch(Exception ex)
            {
            }
        }

    }
}
