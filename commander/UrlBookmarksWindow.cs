using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace commander
{
    public partial class UrlBookmarksWindow : Form
    {
        public UrlBookmarksWindow()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
            Shown += UrlBookmarksWindow_Shown;
            
            UpdateList();
            pictureBox1.Image = DrawInfoIcon();
            pictureBox1.MouseHover += PictureBox1_MouseHover;
            pictureBox1.MouseLeave += PictureBox1_MouseLeave;
            toolTip1.SetToolTip(this.pictureBox1, "Paste a link from the clipboard (Press ctrl+v) ");
        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = DrawInfoIcon();
        }

        private void PictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.Image = DrawInfoIconHovered();
        }

        private void UrlBookmarksWindow_Shown(object sender, System.EventArgs e)
        {
            watermark1.Init();
        }

        HashSet<string> hash = new HashSet<string>();

        void DeleteSelected()
        {
            if (listView1.SelectedItems.Count == 0) return;
            var bm = listView1.SelectedItems[0].Tag as UrlBookmark;
            if (Stuff.Question("Delete " + bm.OriginalUrl + " from bookmarks?") == DialogResult.Yes)
            {
                listView1.Items.Remove(listView1.SelectedItems[0]);
                Stuff.DeleteBookmark(bm);
            }
        }
        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (listView1.SelectedItems.Count == 0) return;
                var bm = listView1.SelectedItems[0].Tag as UrlBookmark;
                RenameDialog rd = new RenameDialog();
                rd.Value = bm.Info;
                if (rd.ShowDialog() == DialogResult.OK) 
                {
                    bm.Info = rd.Value;
                    listView1.SelectedItems[0].SubItems[1].Text = bm.Info;
                    Stuff.IsDirty = true;
                }
            }
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();

            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
            {
                var txt = Clipboard.GetText();
                if (Stuff.UrlBookmarks.Any(z => z.OriginalUrl == txt))
                {
                    Stuff.Warning("same url already exist.");
                    return;
                }
                if (hash.Add(txt))
                {
                    var dec = HttpUtility.UrlDecode(txt);

                    var b = new UrlBookmark() { Uri = new Uri(txt), OriginalUrl = dec };
                    if (dec.ToLower().Contains(watermark1.Text))
                    {
                        listView1.Items.Add(new ListViewItem(new string[] { dec, "" }) { Tag = b });
                    }
                    Stuff.AddUrlBookmark(b);
                }
            }
        }

        Bitmap b1;
        Bitmap b2;
        public Bitmap DrawInfoIcon()
        {
            if (b1 != null) return b1;
            Bitmap bmp = new Bitmap(20, 20);
            var gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.FillEllipse(Brushes.Blue, 0, 0, 18, 18);
            gr.DrawString("i", new Font("Times New Roman", 12), Brushes.White, 5, 0);
            b1 = bmp;
            return bmp;
        }
        public Bitmap DrawInfoIconHovered()
        {
            if (b2 != null) return b2;
            Bitmap bmp = new Bitmap(20, 20);
            var gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.FillEllipse(Brushes.Orange, 0, 0, 18, 18);
            gr.DrawString("i", new Font("Times New Roman", 12), Brushes.White, 5, 0);
            b2 = bmp;
            return bmp;
        }
        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (listView1.SelectedItems[0].Tag is UrlBookmark)
            {
                var r = listView1.SelectedItems[0].Tag as UrlBookmark;
                Process.Start(r.OriginalUrl);
            }
        }

        public void UpdateList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var item in Stuff.UrlBookmarks)
            {
                
                bool pass = false;
                if (item.OriginalUrl.ToLower().Contains(watermark1.Text.ToLower())) pass = true;
                if (!string.IsNullOrEmpty(item.Info) && item.Info.ToLower().Contains(watermark1.Text.ToLower())) pass = true;

                if (!pass) continue;
                listView1.Items.Add(new ListViewItem(new string[] { item.OriginalUrl, item.Info }) { Tag = item });
            }
            listView1.EndUpdate();
        }
        private void Watermark1_TextChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void ExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (listView1.SelectedItems[0].Tag is UrlBookmark)
            {
                var r = listView1.SelectedItems[0].Tag as UrlBookmark;
                Process.Start(r.OriginalUrl);
            }
        }
        public static void InitiateSSLTrust()
        {
            try
            {
                //Change SSL checks so that all checks pass
                ServicePointManager.ServerCertificateValidationCallback =
                   new RemoteCertificateValidationCallback(
                        delegate
                        { return true; }
                    );
            }
            catch (Exception ex)
            {
                //ActivityLog.InsertSyncActivity(ex);
            }
        }
        private void UpdateTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //InitiateSSLTrust();
            // using System.Net;
            ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            if (listView1.SelectedItems.Count == 0) return;
            if (listView1.SelectedItems[0].Tag is UrlBookmark)
            {
                var r = listView1.SelectedItems[0].Tag as UrlBookmark;
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        var str = wc.DownloadString(r.Uri.ToString());

                        var list = Stuff.ParseHtmlItems(str, "<title", "/title>");
                        var list2 = Stuff.ParseHtmlItems(str, "<meta", "/>");
                        if (list.Any())
                        {

                            var targetEnc = Encoding.Default;
                            if (str.ToLower().Contains("charset=utf-8") || (str.ToLower().Contains("charset=\"utf-8")))
                            {
                                targetEnc = Encoding.UTF8;
                            }
                            var encl = list2.FirstOrDefault(z => z.Contains("charset"));
                            if (encl != null)
                            {
                                var aa = encl.Split(new char[] { '\"', '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                                if (Encoding.GetEncodings().Any(z => z.Name == aa.Last().ToLower()))
                                {
                                    targetEnc = Encoding.GetEncoding(aa.Last());
                                }
                            }
                            //CP1251UTF - 8


                            var strr = list[0];
                            var s1 = strr.IndexOf('>');
                            strr = strr.Substring(s1 + 1);
                            s1 = strr.IndexOf('<');
                            strr = strr.Substring(0, s1);
                            strr = targetEnc.GetString(Encoding.Default.GetBytes(strr));
                            Clipboard.SetText(strr);
                            r.Info = strr;
                            listView1.SelectedItems[0].SubItems[1].Text = r.Info;
                            Stuff.IsDirty = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Stuff.Error(ex.Message);
                    }
                }
            }
        }

        private void AddDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }
    }
}
