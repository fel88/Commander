using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace commander.Controls
{
    public partial class WebpViewer : UserControl
    {
        public WebpViewer()
        {
            InitializeComponent();
        }
        Graphics gr;
        Bitmap board;

        int sourceW;
        int sourceH;
        internal void Init(string fullName)
        {
            timer1.Enabled = false;

            var opp = WebpWrapper.open(fullName);
            var fn = WebpWrapper.getFramesNum();

            sourceW = WebpWrapper.getWidth();
            sourceH = WebpWrapper.getHeight();
            board = new Bitmap(sourceW, sourceH, PixelFormat.Format32bppArgb);
            gr = Graphics.FromImage(board);
            //if (fn > 1)
            timer1.Enabled = true;
        }

        internal void ResetImage()
        {
            timer1.Enabled = false;
            Image temp = null;
            if (pictureBox1.Image != null)
            {
                temp = pictureBox1.Image;
            }
            pictureBox1.Image = null;
            //CurrentFile = null;
            if (temp != null)
            {
                temp.Dispose();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            WebpWrapper.decodeIteration(0);

            var fn = WebpWrapper.getFramesNum();
            var cf = WebpWrapper.getCurrFrame();
            //   if (cf > 10) return;
            var dur = WebpWrapper.getDuration();
            if (dur < 10) dur = 100;
            timer1.Interval = dur;
            var ww = WebpWrapper.getWidth();
            var hh = WebpWrapper.getHeight();

            var xof = WebpWrapper.getXoffset();
            var yof = WebpWrapper.getYoffset();

            byte[] frame = new byte[ww * hh * 4];
            WebpWrapper.getData(frame);
            var b = getBMP(frame, ww, hh);
            //Mat mat = new Mat(new int[] { ww, hh}, MatType.CV_8UC4,  frame);
            //pictureBox1.Image=BitmapConverter
            RGBtoBGR(b);

            gr.DrawImage(b, xof, yof);

            pictureBox1.Image = board;
        }
        private static Bitmap getBMP(byte[] buffer, int width, int height)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int skipByte = bmpData.Stride - width * 4;
            byte[] newBuff = new byte[buffer.Length + skipByte * height];
            for (int j = 0; j < height; j++)
            {
                Buffer.BlockCopy(buffer, j * width * 4, newBuff, j * (width * 4 + skipByte), width * 4);
            }

            Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
            b.UnlockBits(bmpData);
            return b;
        }
        public static void RGBtoBGR(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                           ImageLockMode.ReadWrite, bmp.PixelFormat);

            int length = Math.Abs(data.Stride) * bmp.Height;

            unsafe
            {
                byte* rgbValues = (byte*)data.Scan0.ToPointer();

                for (int i = 0; i < length; i += 4)
                {
                    byte dummy = rgbValues[i];
                    rgbValues[i] = rgbValues[i + 2];
                    rgbValues[i + 2] = dummy;
                }
            }

            bmp.UnlockBits(data);
        }
    }
}
