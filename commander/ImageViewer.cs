using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class ImageViewer : Form
    {
        public ImageViewer()
        {
            InitializeComponent();
        }

        public void SetBitmap(Bitmap bmp,bool withTransparentBack=false)
        {
           

            #region draw checkmate background
            if (withTransparentBack)
            {
                Bitmap res = new Bitmap(bmp.Width, bmp.Height);
                var gr = Graphics.FromImage(res);
                int ww = 10;
                for (int i = 0; i < bmp.Width / ww; i++)
                {
                    for (int j = 0; j < bmp.Height / ww; j++)
                    {
                        /*var x1 = ww + i * 2 * ww;
                        var y1 = j * 2 * ww;
                        var x2 = 2 * i * ww;
                        var y2 = ww + j * 2 * ww;
                        var x3 = x2 + ww;
                        var y3 = y1 + 2 * ww;
                        var x4 = x2 + 2 * ww;
                        var y4 = y1 + ww;*/
                        /*gr.FillPolygon(Brushes.LightGray, new PointF[]
                        {
                            new PointF(x1,y1),
                            new PointF(x2,y2),
                            new PointF(x3,y3),
                            new PointF(x4,y4),

                        });*/
                        if ((i + j) % 2 == 0)
                        {
                            gr.FillRectangle(Brushes.LightGray, i * ww, j * ww, ww, ww);
                        }
                    }
                }

                #endregion

                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        var px = bmp.GetPixel(i, j);
                        if (px.A > 0)
                        {
                            res.SetPixel(i, j, Color.FromArgb(px.R, px.G, px.B));
                        }
                    }
                }
                pictureBox1.Image = res;
            }
            else
            {
                pictureBox1.Image = bmp;
            }
            
        }
    }
}
