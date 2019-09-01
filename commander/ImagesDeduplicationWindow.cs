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
    public partial class ImagesDeduplicationWindow : Form
    {
        public ImagesDeduplicationWindow()
        {
            InitializeComponent();

            lbl.AutoSize = true;
            pictureBox1.Controls.Add(lbl);
        }
        Label lbl = new Label();

        public static string ToHash(int[] d)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < d.Length; i++)
            {
                sb.Append(d[i] + ";");
            }
            return sb.ToString();
        }

        public static Dictionary<string, int[]> Cache = new Dictionary<string, int[]>();
        public static int[] GetImageHash(string path1, int step = 40)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(path1))
                {
                    return Cache[path1];
                }
                using (var bmp = Bitmap.FromFile(path1))
                {
                    Cache.Add(path1, GetImageHash(bmp, step));
                }
                return Cache[path1];
            }            
        }
        public static int[] GetImageHash(Image bmp, int step = 40)
        {

            int levels = 255 / step;
            int[,] hist = new int[levels + 1, 3];
            int size = 0;


            DirectBitmap dbm = new DirectBitmap(bmp as Bitmap);
            size = bmp.Width * bmp.Height;
            for (int i = 0; i < dbm.Width; i++)
            {
                for (int j = 0; j < dbm.Height; j++)
                {
                    var px = dbm.GetPixel(i, j);
                    hist[(px.R / step), 0]++;
                    hist[(px.G / step), 1]++;
                    hist[(px.B / step), 2]++;
                }
            }
            dbm.Dispose();

            List<int> ret = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < levels; j++)
                {
                    hist[j, i] = (int)((hist[j, i] / (float)size) * 100);
                    ret.Add(hist[j, i]);
                }

            }
            return ret.ToArray();

        }
        public static int Dist(int[] d1, int[] d2)
        {
            int ret = 0;
            for (int i = 0; i < d1.Length; i++)
            {
                ret += Math.Abs(d1[i] - d2[i]);
            }
            return ret;
        }
        public static IFileInfo[][] FindDuplicates(DedupContext dup)
        {
            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var d in dup.Dirs)
            {
                Stuff.GetAllFiles(d, files);
            }
            files.AddRange(dup.Files);
            files = files.Where(z => z.Exist).ToList();


            var grp1 = files.GroupBy(z => ToHash(GetImageHash(z.FullName))).Where(z => z.Count() > 1).ToArray();
            return grp1.Select(z => z.ToArray()).ToArray();
        }

        public DedupContext Context;

        public void SetGroups(DedupContext ctx, IFileInfo[][] groups)
        {
            Context = ctx;
            listView1.Items.Clear();
            foreach (var fileInfo in groups.OrderByDescending(z => z.First().Length * z.Length))
            {
                listView1.Items.Add(new ListViewItem(new string[] { fileInfo.First().Name,
                    Stuff.CalcPartMD5(fileInfo.First().FullName,1024*1024),
                    fileInfo.Length + "" ,
                    Stuff.GetUserFriendlyFileSize((fileInfo.First().Length*(fileInfo.Length-1)))
                })
                { Tag = fileInfo });
            }

            label1.Text = "Total repeats groups: " + groups.Length;
            label2.Text = "Total memory overhead: " + Stuff.GetUserFriendlyFileSize(groups.Sum(z => z.First().Length * (z.Length - 1)));

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var ff = listView1.SelectedItems[0].Tag as IFileInfo[];
                listView2.Items.Clear();
                foreach (var l in ff)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { l.FullName, Stuff.GetUserFriendlyFileSize(l.Length) }) { Tag = l });
                }
                listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private void ListView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var ff = listView2.SelectedItems[0].Tag as IFileInfo;
                if (pictureBox1.Image != null)
                {
                    var temp = pictureBox1.Image;
                    pictureBox1.Image = null;
                    temp.Dispose();
                }

                pictureBox1.Image = Bitmap.FromFile(ff.FullName);
                var bmp = pictureBox1.Image;
                lbl.Text = Stuff.GetUserFriendlyFileSize(ff.Length) + " " + bmp.Width + "x" + bmp.Height;
            }
        }

        private void DeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                if (listView2.SelectedItems[0].Tag is IFileInfo)
                {
                    var f = listView2.SelectedItems[0].Tag as IFileInfo;
                    f.Filesystem.DeleteFile(f);
                }
            }
        }
    }
}
