using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OneCardSystem.Recognizer;

namespace FaceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[] firstFeature;

        byte[] secondFeature;

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*;";

            openFile.Multiselect = false;

            openFile.FileName = "";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                this.pictureBox1.Image = null;

                Image image = Image.FromFile(openFile.FileName);

                this.pictureBox1.Image = new Bitmap(image);

                image.Dispose();

                //TODO检测人脸，提取特征
                firstFeature = detectAndExtractFeature(this.pictureBox1.Image, 1);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*;";

            openFile.Multiselect = false;

            openFile.FileName = "";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                this.pictureBox2.Image = null;

                Image image = Image.FromFile(openFile.FileName);

                this.pictureBox2.Image = new Bitmap(image);

                image.Dispose();

                //TODO检测人脸，提取特征
                secondFeature = detectAndExtractFeature(this.pictureBox2.Image, 2);
            }
        }

        private byte[] detectAndExtractFeature(Image imageParam, int firstSecondFlg)
        {
            byte[] feature = null;
            FaceRectInfo[] faceRect = new FaceRectInfo[1];
            FacePointInfo[] facePoint = new FacePointInfo[1];

            Bitmap bitmap = new Bitmap(imageParam);
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                imageData = ms.GetBuffer();
            }

            Stopwatch watchTime = new Stopwatch();
            watchTime.Start();
            int faceNum = recognizer.Face_DetectByFile(imageData, 0, 0, out faceRect, out facePoint);
            watchTime.Stop();
            if (firstSecondFlg == 1)
            {
                setControlText(this.label5, String.Format("检测耗时:{0}ms", watchTime.ElapsedMilliseconds));

            }
            else if (firstSecondFlg == 2)
            {
                setControlText(this.label3, String.Format("检测耗时:{0}ms", watchTime.ElapsedMilliseconds));
            }
            if (faceNum <= 0)
            {
                LogHelper.WriteLog("未识别到人脸");
                return feature;
            }

            RECT rect = faceRect[0].rc;
            Image image = CutFace(bitmap, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            if (firstSecondFlg == 1)
            {
                this.pictureBox3.Image = image;
            }
            else if (firstSecondFlg == 2)
            {
                this.pictureBox4.Image = image;
            }

            watchTime.Start();
            bool result = recognizer.Face_GetFeaByFile(imageData, 0, 0, out feature);
            watchTime.Stop();
            if (firstSecondFlg == 1)
            {
                setControlText(this.label6, String.Format("抽取特征耗时:{0}ms", watchTime.ElapsedMilliseconds));
            }
            else if (firstSecondFlg == 2)
            {
                setControlText(this.label7, String.Format("抽取特征耗时:{0}ms", watchTime.ElapsedMilliseconds));
            }
            if (!result || feature == null || feature.Length <= 0)
            {
                LogHelper.WriteLog("未提取到特征");
            }
            return feature;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (recognizer != null)
            {
                recognizer.Uninit();
                recognizer = null;
            }

            string type = comboBox1.SelectedItem.ToString();
            if (type == "")
            {
                return;
            }
            IFaceRecognizer newRecognizer = RecongnizerFactory.GetRecognizer(type);
            if (!newRecognizer.CheckKey())
            {
                LogHelper.WriteLog("算法检测Key错误!!!");
            }
            if (!newRecognizer.Init())
            {
                LogHelper.WriteLog("算法初始化错误!!!");
            }
            this.recognizer = newRecognizer;
        }

        private IFaceRecognizer recognizer;

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("ArcFace");
        }

        private void setControlText(Control control, string value)
        {
            control.Invoke(new Action<Control, string>((ct, v) => { ct.Text = v; }), new object[] {control, value});
        }

        private void setPictureBoxControlImage(PictureBox control, Bitmap value)
        {
            control.Invoke(new Action<PictureBox, Bitmap>((ct, v) => { ct.Image = v; }), new object[] {control, value});
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float similar = 0f;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            similar = recognizer.Compare(firstFeature, secondFeature);

            stopwatch.Stop();
            setControlText(this.label1,
                "相似度:" + similar.ToString() + " 耗时:" + stopwatch.ElapsedMilliseconds.ToString() + "ms");
        }

        public static Bitmap CutFace(Bitmap srcImage, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (srcImage == null)
            {
                return null;
            }

            int w = srcImage.Width;

            int h = srcImage.Height;

            if (StartX >= w || StartY >= h)
            {
                return null;
            }
            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);

                Graphics g = Graphics.FromImage(bmpOut);

                g.DrawImage(srcImage, new Rectangle(0, 0, iWidth, iHeight),
                    new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);

                g.Dispose();

                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[]) e.Data.GetData(DataFormats.FileDrop, false);

            if (fileList.Length == 1)
            {
                string filePath = fileList[0];
                if (pictureBox1.Image != null && pictureBox2.Image == null)
                {
                    WorkBox2(filePath);
                }
                else
                {
                    WorkBox1(filePath);
                }
            }
            else
            {
                string filePath1 = fileList[0];
                string filePath2 = fileList[1];
                WorkBox1(filePath1);
                WorkBox2(filePath2);
            }

        }

        private void WorkBox1(string filepath)
        {
            this.pictureBox1.Image = null;
            Image image = Image.FromFile(filepath);
            this.pictureBox1.Image = new Bitmap(image);
            image.Dispose();
            firstFeature = detectAndExtractFeature(this.pictureBox1.Image, 1);
        }

        private void WorkBox2(string filepath)
        {
            this.pictureBox2.Image = null;
            Image image = Image.FromFile(filepath);
            this.pictureBox2.Image = new Bitmap(image);
            image.Dispose();
            secondFeature = detectAndExtractFeature(this.pictureBox2.Image, 2);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            // Check if the Dataformat of the data can be accepted
            // (we only accept file drops from Explorer, etc.)
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Okay
            }
            else
            {
                e.Effect = DragDropEffects.None; // Unknown data, ignore it
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string outFile = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.OverwritePrompt = true;//询问是否覆盖
            saveFileDialog.Filter = "所有文件(*.*) | *.*";
            saveFileDialog.DefaultExt = "db";  //缺省默认后缀名
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                outFile = saveFileDialog.FileName;
                //如果文件存在，删除重建
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }
                using (FileStream fs = new FileStream(outFile, FileMode.Create))
                {
                    fs.Write(firstFeature, 0, firstFeature.Length);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string outFile = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.OverwritePrompt = true;//询问是否覆盖
            saveFileDialog.Filter = "所有文件(*.*) | *.*";
            saveFileDialog.DefaultExt = "db";  //缺省默认后缀名
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                outFile = saveFileDialog.FileName;
                //如果文件存在，删除重建
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }
                using (FileStream fs = new FileStream(outFile, FileMode.Create))
                {
                    fs.Write(secondFeature, 0, secondFeature.Length);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            firstFeature = null;
            pictureBox3.Image = null;
            label5.Text = "";
            label6.Text = "";
            label1.Text = "相似度：";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            secondFeature = null;
            pictureBox4.Image = null;
            label3.Text = "";
            label7.Text = "";
            label1.Text = "相似度：";
        }
    }
}
