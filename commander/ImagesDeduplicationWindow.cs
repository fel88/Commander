using PluginLib;
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

        public static int[] GetImageHash2D(Bitmap bmp, int step = 10)
        {
            var db = new DirectBitmap(bmp);
            int cww = 10;
            int chh = 10;
            int[,] ret = new int[cww, chh];

            var sx = bmp.Width / (cww - 1);
            var sy = bmp.Height / (chh - 1);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    var px = db.GetPixel(i, j);
                    var s = (px.R + px.G + px.B) / 3;
                    ret[i / sx, j / sy] += s;

                }
            }

            int total = (bmp.Width * bmp.Height) / 100;
            int[] ret2 = new int[10 * 10];
            int ind = 0;
            for (int i = 0; i < cww; i++)
            {
                for (int j = 0; j < chh; j++)
                {
                    ret[i, j] /= total;
                    ret2[ind++] = ret[i, j];
                }
            }
            db.Dispose();
            return ret2;
        }

        public static int[] GetImageHash(string path1, int step = 40)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(path1))
                {
                    return Cache[path1];
                }
                using (var bmp = Bitmap.FromFile(path1) as Bitmap)
                {
                    bmp.SetResolution(96, 96);
                    Cache.Add(path1, GetImageHash2D(bmp, step));
                }
                return Cache[path1];
            }
        }
        public static int[] GetImageHash(Bitmap bmp, int step = 40)
        {

            int levels = 255 / step;
            int[,] hist = new int[levels + 1, 3];
            int size = 0;


            DirectBitmap dbm = new DirectBitmap(bmp);
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
        public static IFileInfo[][] FindDuplicates(DedupContext dup, Action<int, int, string> reportProgress = null)
        {
            var files = dup.GetAllFiles();

            List<List<IFileInfo>> groups = new List<List<IFileInfo>>();
            Dictionary<string, int[]> hashes = new Dictionary<string, int[]>();
            int cnt = 0;
            foreach (var item in files)
            {
                if (reportProgress != null)
                {
                    reportProgress(cnt, files.Length * 2, "calc hash of " + item.Name);
                }
                try
                {
                    hashes.Add(item.FullName, GetImageHash(item.FullName));
                }
                catch (Exception ex)
                {

                }
                cnt++;
            }
            int treshold = 800;

            foreach (var item in files)
            {
                if (reportProgress != null)
                {
                    reportProgress(cnt, files.Length * 2, "grouping " + item.Name);
                }
                cnt++;
                if (!hashes.ContainsKey(item.FullName)) continue;
                var h = hashes[item.FullName];
                List<IFileInfo> grp = null;
                int best = treshold;
                foreach (var gitem in groups)
                {
                    foreach (var hitem in gitem)
                    {
                        if (Dist(h, hashes[hitem.FullName]) < best)
                        {
                            best = Dist(h, hashes[hitem.FullName]);
                            grp = gitem;
                        }
                    }
                }
                if (grp == null)
                {
                    groups.Add(new List<IFileInfo>());
                    groups.Last().Add(item);
                }
                else
                {
                    grp.Add(item);
                }
            }

            //var grp1 = files.GroupBy(z => ToHash(GetImageHash(z.FullName))).Where(z => z.Count() > 1).ToArray();
            //return grp1.Select(z => z.ToArray()).ToArray();
            return groups.Where(z => z.Count > 1).Select(z => z.ToArray()).ToArray();
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
                var tgs = Stuff.GetTagsOfFile(ff.FullName);
                lbl.Text += " tags: " + tgs.Aggregate("", (x, y) => x + y.Name + "; ");
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

        private void DeleteAllButThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //delete all and merge tags
            if (listView2.SelectedItems.Count == 0) return;
            var fli = listView2.SelectedItems[0].Tag as IFileInfo;
            if (Stuff.Question("Are you sure to delete all files in this group except " + fli.Name + ", and merge all tags into it?") != DialogResult.Yes) return;
                        
            List<TagInfo> tags = new List<TagInfo>();
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                var fl = listView2.Items[i].Tag as IFileInfo;
                if (fl == fli) continue;
                var tgs = Stuff.GetAllTagsOfFile(fl.FullName);
                tags.AddRange(tgs);
                fl.Filesystem.DeleteFile(fl);
            }
            tags = tags.Distinct().ToList();
            foreach (var item in tags)
            {
                if (item.ContainsFile(fli)) continue;
                item.AddFile(fli);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}
