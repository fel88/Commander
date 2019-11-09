using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class ScannerWindow : Form
    {
        public ScannerWindow()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            pictureBox1.SizeChanged += PictureBox1_SizeChanged;
        }

        private void PictureBox1_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
        }

        Bitmap bmp;
        Graphics gr;

        GraphicsPath GetSector(float startAngle, float endAngle, float r1, float r2)
        {
            GraphicsPath ret = new GraphicsPath();
            float[] xx = new float[4];
            float[] yy = new float[4];


            ret.AddArc(-r1, -r1, 2 * r1, 2 * r1, startAngle, endAngle - startAngle);
            GraphicsPath gptemp = new GraphicsPath();
            gptemp.AddArc(-r2, -r2, 2 * r2, 2 * r2, startAngle, endAngle - startAngle);

            ret.AddLine(ret.GetLastPoint(), gptemp.GetLastPoint());
            ret.AddArc(-r2, -r2, 2 * r2, 2 * r2, endAngle, -(endAngle - startAngle));
            //ret.AddArc(-r2, -r2, 2 * r2, 2 * r2, startAngle, endAngle - startAngle);
            ret.AddLine(gptemp.PathPoints[0], ret.PathPoints[0]);
            ret.CloseAllFigures();
            return ret;
            startAngle *= (float)(Math.PI / 180.0f);
            endAngle *= (float)(Math.PI / 180.0f);

            xx[0] = (float)(r1 * Math.Cos(startAngle));
            xx[1] = (float)(r2 * Math.Cos(startAngle));
            xx[2] = (float)(r1 * Math.Cos(endAngle));
            xx[3] = (float)(r2 * Math.Cos(endAngle));

            yy[0] = (float)(r1 * Math.Sin(startAngle));
            yy[1] = (float)(r2 * Math.Sin(startAngle));
            yy[2] = (float)(r1 * Math.Sin(endAngle));
            yy[3] = (float)(r2 * Math.Sin(endAngle));

            ret.AddPolygon(new PointF[] {
                new PointF(xx[0], yy[0]),
                new PointF(xx[1], yy[1]),
                new PointF(xx[2], yy[2]),
                new PointF(xx[3], yy[3]),
            });
            return ret;
        }

        void DrawLayer(Graphics gr, ScannerItemInfo scd, float r1, float r2, float parentStartAng, float parentEndAng)
        {
            float crntAng = parentStartAng;
            foreach (var item in scd.Items)
            {
                float perc = item.Size / (float)scd.Size;
                var deltaAng = parentEndAng - parentStartAng;
                var ang = deltaAng * perc;

                if (ang >= 1)
                {
                    item.EndAng = crntAng + ang;
                    item.R1 = r1;
                    item.R2 = r2;
                    item.StartAng = crntAng;
                    var gp = GetSector(crntAng, crntAng + ang, r1, r2);
                    //if (item is ScannerDirInfo)
                    {
                        var d1 = item as ScannerDirInfo;
                        //if (d1.Dir.FullName.Contains("apache2"))
                        {
                            if (hovered == item)
                            {
                                gr.FillPath(Brushes.LightGreen, gp);
                            }
                            else
                            {
                                if (item is ScannerFileInfo)
                                {
                                    gr.FillPath(Brushes.LightYellow, gp);
                                }
                                else
                                {
                                    gr.FillPath(Brushes.LightBlue, gp);
                                }
                            }


                            gr.DrawPath(Pens.Black, gp);
                        }
                    }
                    DrawLayer(gr, item, r1 + radInc, r2 + radInc, crntAng, crntAng + ang);
                    crntAng += ang;
                }


            }
        }
        bool ready = false;

        ScannerItemInfo hovered = null;
        int radInc = 20;
        int radb = 50;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            gr.SmoothingMode = SmoothingMode.AntiAlias;

            var pos = pictureBox1.PointToClient(Cursor.Position);
            gr.Clear(Color.White);
            gr.ResetTransform();
            //var gp=GetSector(45, 135, 100, 150);
            //   gr.DrawPath(Pens.Black, gp);

            if (ready/* && false*/)
            {
                hovered = null;
                var dist = Math.Sqrt(Math.Pow(pos.X - pictureBox1.Width / 2, 2) + Math.Pow(pos.Y - pictureBox1.Height / 2, 2));
                if (dist < radb)
                {
                    hovered = Root;
                }
                var ang = Math.Atan2(pos.Y - pictureBox1.Height / 2, pos.X - pictureBox1.Width / 2) * 180f / Math.PI;
                if (ang < 0) { ang += 360f; }
                foreach (var item in Items)
                {

                    if (dist >= item.R1 && dist <= item.R2 && ang >= item.StartAng && ang <= item.EndAng)
                    {
                        hovered = item;
                    }
                }
                gr.DrawString((Root as ScannerDirInfo).GetDirFullName(), new Font("Arial", 12), Brushes.Black, 5, 5);
                if (hovered != null)
                {

                    gr.DrawString(hovered.Name, new Font("Arial", 12), Brushes.Black, 5, 25);
                    gr.DrawString(Stuff.GetUserFriendlyFileSize(hovered.Size), new Font("Arial", 12), Brushes.Black, 5, 45);
                }


                gr.TranslateTransform(bmp.Width / 2, bmp.Height / 2);


                if (Root == hovered)
                {
                    gr.FillEllipse(Brushes.Green, -radb, -radb, 2 * radb, 2 * radb);
                }
                else
                {
                    gr.FillEllipse(Brushes.Violet, -radb, -radb, 2 * radb, 2 * radb);
                }
                gr.DrawEllipse(Pens.Black, -radb, -radb, 2 * radb, 2 * radb);
                var sz = Stuff.GetUserFriendlyFileSize(Root.Size);
                var ff = new Font("Arial", 12);
                var ms = gr.MeasureString(sz, ff);

                gr.DrawString(sz, ff, Brushes.White, -ms.Width / 2, 0);

                sz = Root.Name;
                ms = gr.MeasureString(sz, ff);

                gr.DrawString(sz, ff, Brushes.White, -ms.Width / 2, -ms.Height);
                DrawLayer(gr, Root, radb, radb + radInc, 0, 360);
            }

            pictureBox1.Image = bmp;
        }

        //List<IFileInfo> files = new List<IFileInfo>();

        void ReportProgress(int i, int max)
        {
            statusStrip1.Invoke((Action)(() =>
            {
                toolStripProgressBar1.Maximum = max;
                toolStripProgressBar1.Value = i;
            }));

        }


        public List<ScannerItemInfo> Items = new List<ScannerItemInfo>();
        public ScannerItemInfo Root = null;

        Thread th;
        internal void Init(IDirectoryInfo d)
        {
            if (th != null) return;
            GC.Collect();
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Value = 0;
            timer2.Enabled = false;
            ready = false;
            Items.Clear();
            Root = null;
            //files.Clear();
            th = new Thread(() =>
           {
               var dirs = Stuff.GetAllDirs(d);

               Queue<ScannerItemInfo> q = new Queue<ScannerItemInfo>();
               int cnt = 0;
               Root = new ScannerDirInfo(null, d);
               q.Enqueue(Root);
               Items.Add(Root);
               while (q.Any())
               {
                   ReportProgress(cnt, dirs.Count);
                   var _dd = q.Dequeue();
                   if (_dd is ScannerFileInfo) continue;

                   var dd = _dd as ScannerDirInfo;
                   /* var sz = Stuff.GetDirectorySize(dd.Dir);
                    if (sz < 1024 * 1024)
                    {
                        dd.HiddenItemsSize = sz;
                        continue;
                    }*/
                   cnt++;
                   try
                   {
                       foreach (var item in dd.Dir.GetFiles())
                       {
                           if (item.Length > 10 * 1024 * 1024)
                           {
                               // dd.HiddenItemsSize += item.Length;
                               //continue;
                           }
                           dd.Items.Add(new ScannerFileInfo(dd, item));
                           Items.Add(dd.Items.Last());
                           //     files.Add(item);
                       }


                   }
                   catch (Exception ex)
                   {

                   }
                   try
                   {
                       foreach (var item in dd.Dir.GetDirectories())
                       {
                           var t = new ScannerDirInfo(dd, item);
                           Items.Add(t);
                           dd.Items.Add(t);
                           q.Enqueue(t);
                       }
                   }
                   catch (Exception ex)
                   {

                   }
                   if (dd.Parent != null)
                   {
                       dd.Dir = null;
                   }
               }
               ReportProgress(cnt, cnt);
               Root.CalcSize();
               ready = true;
               statusStrip1.Invoke((Action)(() =>
               {
                   timer2.Enabled = true;
                   timer2.Interval = 2000;
               }));

               th = null;
           });
            th.IsBackground = true;
            th.Start();

        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (temp != null && temp is ScannerDirInfo)
            {
                var d = temp as ScannerDirInfo;
                Process.Start(d.GetDirFullName());
            }
        }

        ScannerItemInfo temp = null;
        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            temp = hovered;

        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            radb += 10;
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            radb -= 10;
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (hovered == null) return;
            if (!(hovered is ScannerDirInfo)) return;

            var d = hovered as ScannerDirInfo;
            var dd = new DirectoryInfo(d.GetDirFullName());
            if (hovered == Root)
            {
                Init(new DirectoryInfoWrapper(dd.Parent));
            }
            else
            {
                Init(new DirectoryInfoWrapper(dd));
            }
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            radInc += 5;
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            radInc -= 5;
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            timer2.Enabled = false;
        }

        private void ScannerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null)
            {
                try
                {
                    th.Abort();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void FollowToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dd = new DirectoryInfo((Root as ScannerDirInfo).GetDirFullName());
            Init(new DirectoryInfoWrapper(dd));
        }
    }
}
