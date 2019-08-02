using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class MemInfoReport : Form
    {
        public MemInfoReport()
        {
            InitializeComponent();
            Shown += MemInfoReport_Shown;

            Stuff.SetDoubleBuffered(listView1);
            DoubleBuffered = true;
            SizeChanged += MemInfoReport_SizeChanged;

            var b = new Button() { Text = "Level Up", AutoSize = true };
            b.Click += B_Click;
            pictureBox1.Controls.Add(b);
        }

        private void MemInfoReport_Shown(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
        }
                
        private void B_Click(object sender, EventArgs e)
        {
            if (CurrentDirectory.FullName == StartDirectory.FullName) return;
            Init(CurrentDirectory.Parent);
        }

        private void MemInfoReport_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);

        }

        Bitmap bmp;
        Graphics gr;

        public IDirectoryInfo CurrentDirectory;
        public IDirectoryInfo StartDirectory = null;
        public long TotalLen;
        public void Init(IDirectoryInfo d)
        {
            if (StartDirectory == null)
            {
                StartDirectory = d;
            }
            Sectors.Clear();
            Text = "Memory report: " + d.FullName;
            CurrentDirectory = d;
            var list = Stuff.GetAllFiles(d);
            var total = list.Sum(z => z.Length);
            TotalLen = total;
            var rootl = d.GetFiles().Sum(z => z.Length);
            if (rootl > 0)
            {

                Sectors.Add(new ReportSectorInfo() { Name = ".root", Length = rootl, Tag = d, Percentage = (float)rootl / total });
            }
            foreach (var item in d.GetDirectories())
            {
                var f = Stuff.GetAllFiles(item);
                var l = f.Sum(z => z.Length);
                Sectors.Add(new ReportSectorInfo()
                {
                    Name = item.Name,
                    Length = l,
                    Percentage = (float)l / total,

                    Tag = item
                });
            }
            Sectors = Sectors.OrderByDescending(z => z.Percentage).ToList();

            listView1.Items.Clear();
            foreach (var item in Sectors)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, Stuff.GetUserFriendlyFileSize(item.Length), (item.Percentage * 100).ToString("F") + "%" }) { Tag = item });
            }
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public List<ReportSectorInfo> Sectors = new List<ReportSectorInfo>();
        public ReportSectorInfo hovered = null;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            /*if (checkBox2.Checked)
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
            else
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }*/

            int gap = 30;
            int min = Math.Min(bmp.Width, bmp.Height);
            var cx = bmp.Width / 2;
            var cy = bmp.Height / 2;
            var rad0 = (min / 2) - gap;
            Rectangle rect = new Rectangle(cx - rad0, cy - rad0, rad0 * 2, rad0 * 2);

            //check hover
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var atan2 = Math.Atan2(pos.Y - cy, pos.X - cx) * 180f / Math.PI;

            var dist = Math.Sqrt(Math.Pow(cx - pos.X, 2) + Math.Pow(cy - pos.Y, 2));


            List<ReportSectorInfo> sectors = Sectors.ToList();
            if (checkBox3.Checked)
            {
                #region remove small sectors
                List<ReportSectorInfo> todel = new List<ReportSectorInfo>();
                long totale = 0;

                var avg = collapseTreshold;
                for (int i = sectors.Count - 1; i >= 0; i--)
                {
                    totale += sectors[i].Length;
                    var perc1 = (float)totale / TotalLen;
                    todel.Add(sectors[i]);
                    if (sectors[i].Percentage > avg) break;
                }
                if (todel.Count > 1)
                {
                    foreach (var item in todel)
                    {
                        sectors.Remove(item);
                    }

                    sectors.Add(new ReportSectorInfo() { Length = totale, Name = ".other sectors", Percentage = totale / (float)TotalLen });
                }
            }
            #endregion;


            if (atan2 < 0)
            {
                atan2 += 360;
            }

            atan2 %= 360;
            float sang = 0;

            hovered = null;
            if (dist < rad0)
            {
                foreach (var item in sectors)
                {
                    float eang = sang + item.Angle;
                    if (atan2 > sang && atan2 < eang)
                    {
                        hovered = item;
                        break;
                    }
                    sang = eang;
                }
            }
            gr.Clear(Color.White);
            sang = 0;
            
            foreach (var item in sectors)
            {


                if (hovered == item)
                {
                    gr.FillPie(Brushes.Red, rect, sang, item.Angle);
                }
                else
                {
                    if (item.Tag == CurrentDirectory)
                    {
                        gr.FillPie(Brushes.Yellow, rect, sang, item.Angle);
                    }
                    else
                    {
                        if (item.Tag == null)
                        {
                            gr.FillPie(Brushes.Blue, rect, sang, item.Angle);
                        }
                        else
                        {
                            gr.FillPie(Brushes.Green, rect, sang, item.Angle);
                        }
                    }
                }

                sang += item.Angle;
            }
            
            sang = 0;
            foreach (var item in sectors)
            {
                gr.DrawPie(Pens.Black, rect, sang, item.Angle);
                sang += item.Angle;
            }
            sang = 0;
            var font = new Font("Arial", 10);
            foreach (var item in sectors)
            {
                float eang = sang + item.Angle;

                var shift = 20;
                var ang = sang + (eang - sang) / 2;
                var tx = cx + (float)((rad0 + shift) * Math.Cos(ang * Math.PI / 180f));
                var ty = cy + (float)((rad0 + shift) * Math.Sin(ang * Math.PI / 180f));
                var ms = gr.MeasureString(item.Text, font);

                if (checkBox1.Checked && (item.Percentage * 100) > collapseTreshold)
                {

                    var tx1 = cx + (float)((rad0 / 2) * Math.Cos(ang * Math.PI / 180f));
                    var ty1 = cy + (float)((rad0 / 2) * Math.Sin(ang * Math.PI / 180f));
                    int ww1 = 6;


                    gr.DrawLine(Pens.Black, tx1, ty1, tx, ty);
                    gr.FillEllipse(new SolidBrush(Color.FromArgb(180, Color.Yellow)), tx1 - ww1, ty1 - ww1, ww1 * 2, ww1 * 2);
                    gr.DrawEllipse(Pens.Black, tx1 - ww1, ty1 - ww1, ww1 * 2, ww1 * 2);
                    gr.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.White)), tx - 2, ty, ms.Width + 4, ms.Height);
                    gr.DrawRectangle(Pens.Black, tx - 2, ty, ms.Width + 4, ms.Height);
                    gr.DrawString(item.Text, font, Brushes.Black, tx, ty);
                }

                sang = eang;

                if (hovered == item)
                {

                    float sx = pos.X + 10;
                    float sy = pos.Y;
                    List<string> strings = new List<string>();
                    strings.Add(item.Name);
                    strings.Add(Stuff.GetUserFriendlyFileSize(item.Length));
                    strings.Add((item.Percentage * 100).ToString("F") + "%");
                    var mss = strings.Select(z => gr.MeasureString(z, font)).ToArray();
                    var wmax = mss.Max(z => z.Width);
                    var hmax = mss.Max(z => z.Height);
                    if (pos.X > (bmp.Width - wmax))
                    {
                        sx = pos.X - wmax;
                    }
                    if (pos.Y > (bmp.Height - hmax * 3))
                    {
                        sy = pos.Y - hmax * 3;
                    }
                    gr.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.White)), sx, sy, wmax, hmax * 3);
                    gr.DrawRectangle(Pens.Black, sx, sy, wmax, hmax * 3);
                    for (int i = 0; i < strings.Count; i++)
                    {
                        gr.DrawString(strings[i], font, Brushes.Black, sx, sy + hmax * i);
                    }

                }
            }
            pictureBox1.Image = bmp;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        float collapseTreshold = 0.05f;
        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            collapseTreshold = ((float)numericUpDown1.Value) / 100f;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (hovered == null) return;
            if (hovered.Tag == null) return;
            var d = hovered.Tag as IDirectoryInfo;
            Init(d);
        }
    }
}
