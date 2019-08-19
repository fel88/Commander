using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class VkExtractorForm : Form
    {
        public VkExtractorForm()
        {
            InitializeComponent();
            //webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            Stuff.SetDoubleBuffered(listView1);
            Stuff.SetDoubleBuffered(listView2);
            Stuff.SetDoubleBuffered(listView3);
        }



        private void button1_Click(object sender, EventArgs e)
        {
            
            var txt = richTextBox1.Text;
            //var lns = File.ReadAllText(txt);
            var lns = txt;
            var list = Stuff.ParseHtmlItems(lns, "<a", "/a>");

            listView1.Items.Clear();
            foreach (var item in list)
            {

                var ar = item.Split(new char[] { ' ', '?', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                //listView1.Items.Add(item);
                if (ar.Count() < 2) continue;
                if (!ar[2].Contains("vk.com")) continue;
                if (!ar[2].Contains("photo")) continue;
                listView1.Items.Add(new ListViewItem(ar[2]) { Tag = ar[2] });
            }
            toolStripStatusLabel1.Text = listView1.Items.Count + " items parsed";

        }
        private String url;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (listView1.SelectedItems.Count == 0) return;


            /*webBrowser1.ScriptErrorsSuppressed = true;
            // set cookie
            //InternetSetCookie(url, "JSESSIONID", Globals.ThisDocument.sessionID);
            webBrowser1.Navigate((string)listView1.SelectedItems[0].Tag, null, null,
                    "User-Agent: Mozilla/5.0");*/


        }

        bool savePic = false;



        int skipped = 0;
        int saved = 0;
        int errors = 0;
        int links = 0;
        public void savePicFunc(string str)
        {
            //richTextBox2.Text = webBrowser1.DocumentText;


            var list = Stuff.ParseHtmlItems(str, "<meta", "/>");
            var list2 = Stuff.ParseHtmlItems(str, "<a", "/a>");
            int totalLinks = 0;
            foreach (var item in list.Union(list2))
            {
                if (!item.Contains("http")) continue;
                var ar1 = item.Split(new char[] { ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var ww = ar1.Where(z => z.Contains("jpg") || z.Contains("png") || z.Contains("gif"));

                foreach (var fr in ww)
                {
                    totalLinks++;
                    try
                    {
                        listView2.Invoke((Action)(() =>
                        {
                            listView2.Items.Add(new ListViewItem(fr) { Tag = fr });
                            links++;
                            using (WebClient wc = new WebClient())
                            {
                                var uri = new Uri(fr);

                                var data = wc.DownloadData(fr);
                                if (data.Length >= 0)
                                {
                                    MemoryStream ms = new MemoryStream(data);
                                    if (showPics)
                                    {
                                        var img = Image.FromStream(ms);
                                        pictureBox1.Image = img;
                                    }
                                    ms.Seek(0, SeekOrigin.Begin);
                                    var hash = Stuff.CalcMD5(ms);
                                    if (md5cache.Add(hash))
                                    {
                                        var fp = Path.Combine(textBox1.Text, uri.Segments.Last());
                                        if (File.Exists(fp))
                                        {
                                            throw new Exception("hash not found, but file exist");
                                        }
                                        saved++;
                                        File.WriteAllBytes(fp, data);
                                        /*File.WriteAllBytes(Path.Combine(textBox1.Text, DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Second + ".png"), data);*/
                                        //img.Save(Path.Combine(textBox1.Text, DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Second + ".png"));
                                    }
                                    else
                                    {
                                        skipped++;
                                    }

                                    ms.Dispose();
                                }
                            }
                        }));

                        break;

                    }
                    catch (Exception ex)
                    {
                        errors++;
                        listView3.Invoke((Action)(() =>
                        {
                            listView3.Items.Add(new ListViewItem(new string[] { ex.Message }) { Tag = ex });
                        }));
                    }
                }

            }

            if (totalLinks == 0)
            {
                listView3.Invoke((Action)(() =>
                {
                    listView3.Items.Add(new ListViewItem(new string[] { "no links: " + str }) { Tag = str, BackColor = Color.Yellow });
                }));
            }

        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            Clipboard.SetText((string)listView1.SelectedItems[0].Tag);
        }



        HashSet<string> md5cache = new HashSet<string>();
        Thread th;
        private void button3_Click(object sender, EventArgs e)
        {
            savePic = true;
            //webBrowser1.ScriptErrorsSuppressed = true;
            int cnt = listView1.Items.Count;
            //calc md5 in target dir
            var dir = new DirectoryInfo(textBox1.Text);
            if (dir.Exists)
            {
                foreach (var item in dir.GetFiles())
                {
                    var md5 = Stuff.CalcMD5(item.FullName);
                    md5cache.Add(md5);
                }
            }
            else
            {
                Stuff.Error("directory " + dir.FullName + " not exist");
                return;
            }
            skipped = 0;
            saved = 0;
            List<string> ss = new List<string>();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                ss.Add((string)listView1.Items[i].Tag);
            }
            th = new Thread(() =>
           {


               for (int i = 0; i < ss.Count; i++)
               {
                   statusStrip1.Invoke((Action)(() =>
                   {
                       toolStripProgressBar1.Visible = true;
                       toolStripProgressBar1.Value = (int)Math.Round(((float)i / (ss.Count)) * 100f);
                   }));
                   try
                   {
                       if (useDelay)
                       {
                           Thread.Sleep(delay);
                       }
                       using (WebClient wc = new WebClient())
                       {
                           wc.Headers.Add("User-Agent: Mozilla/5.0");
                           //while (!iscomplete) { Thread.Sleep(10); }

                           var item = ss[i];
                           //  iscomplete = false;
                           var str = wc.DownloadString(item);
                           if (savePic)
                           {
                               savePicFunc(str);
                           }

                           //webBrowser1.Navigate((string)(item as ListViewItem).Tag, null, null, "User-Agent: Mozilla/5.0");
                           statusStrip1.Invoke((Action)(() =>
                           {
                               toolStripStatusLabel1.Text = "skipped: " + skipped + "  saved: " + saved + "; errors: " + errors+"; links extracted: "+links;
                           }));

                       }
                   }
                   catch (Exception ex)
                   {
                       errors++;
                   }
               }
               statusStrip1.Invoke((Action)(() =>
               {
                   toolStripStatusLabel1.Text = "skipped: " + skipped + "  saved: " + saved + "; errors: " + errors + "; links extracted: " + links;
                   toolStripProgressBar1.Value = 100;
                   toolStripProgressBar1.Visible = false;

               }));
           });
            th.IsBackground = true;
            th.Start();


        }

        bool showPics = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            showPics = checkBox1.Checked;
        }

        private void VkExtractorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null)
            {
                th.Abort();
            }
        }

        private void ListView3_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void ListView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            Clipboard.SetText((string)listView3.SelectedItems[0].Tag);
        }

        bool useDelay = false;
        int delay = 1000;
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            useDelay = checkBox2.Checked;
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                delay = int.Parse(textBox2.Text);
                textBox2.BackColor = Color.White;
            }
            catch(Exception ex)
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void CopyAllToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                var str = (string)listView2.Items[i].Tag;
                sb.AppendLine(str);
            }
            Clipboard.SetText(sb.ToString());
        }
    }
}