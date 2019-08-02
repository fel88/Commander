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
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
        }


        public string[] parseItems(string lns, string key1, string key2)
        {
            string accum = "";
            bool inside = false;
            string accum2 = "";
            List<string> list = new List<string>();
            for (int i = 0; i < lns.Length; i++)
            {
                if (accum.Length > Math.Max(key1.Length, key2.Length))
                {
                    accum = accum.Remove(0, 1);
                }
                accum += lns[i];
                if (accum.EndsWith(key1))
                {
                    inside = true;
                }

                if (inside && accum.EndsWith(key2))
                {
                    inside = false;
                    list.Add(accum2);
                    accum2 = "";
                }

                if (inside)
                {
                    accum2 += lns[i];
                }
            }
            return list.ToArray();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var txt = richTextBox1.Text;
            var lns = File.ReadAllText(txt);
            var list = parseItems(lns, "<a", "/a>");

            foreach (var item in list)
            {

                var ar = item.Split(new char[] { ' ', '?', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                //listView1.Items.Add(item);
                if (ar.Count() < 2) continue;
                if (!ar[2].Contains("vk.com")) continue;
                if (!ar[2].Contains("photo")) continue;
                listView1.Items.Add(new ListViewItem(ar[2]) { Tag = ar[2] });
            }

        }
        private String url;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (listView1.SelectedItems.Count == 0) return;


            webBrowser1.ScriptErrorsSuppressed = true;
            // set cookie
            //InternetSetCookie(url, "JSESSIONID", Globals.ThisDocument.sessionID);
            webBrowser1.Navigate((string)listView1.SelectedItems[0].Tag, null, null,
                    "User-Agent: Mozilla/5.0");


        }

        bool savePic = false;
        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            richTextBox2.Text =
            webBrowser1.DocumentText;
            if (savePic)
            {
                savePicFunc();
            }
            iscomplete = true;
        }

        bool iscomplete = true;
        public void savePicFunc()
        {
            richTextBox2.Text =
        webBrowser1.DocumentText;
            if (!Directory.Exists("pics"))
            {
                Directory.CreateDirectory("pics");
            }

            var list = parseItems(richTextBox2.Text, "<meta", "/>");
            foreach (var item in list)
            {
                if (!item.Contains("http")) continue;
                var ar1 = item.Split(new char[] { ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var fr = ar1.FirstOrDefault(z => z.Contains("jpg") || z.Contains("png") || z.Contains("gif"));
                if (fr != null)
                {
                    try
                    {
                        listView2.Invoke((Action)(() =>
                        {
                            listView2.Items.Add(new ListViewItem(fr) { Tag = fr });
                            WebClient wc = new WebClient();
                            var data = wc.DownloadData(fr);
                            MemoryStream ms = new MemoryStream(data);
                            var img = Image.FromStream(ms);
                            pictureBox1.Image = img;
                            
                            img.Save(Path.Combine(textBox1.Text, DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Second + ".png"));

                            ms.Dispose();
                            
                        }));
                        break;

                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            Clipboard.SetText((string)listView1.SelectedItems[0].Tag);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            savePicFunc();


        }

        private void button3_Click(object sender, EventArgs e)
        {
            savePic = true;
            webBrowser1.ScriptErrorsSuppressed = true;
            int cnt = listView1.Items.Count;
            Thread th = new Thread(() =>
            {
              

                for (int i = 0; i < cnt; i++)
                {
                    while (!iscomplete) { Thread.Sleep(10); }
                    webBrowser1.Invoke((Action)(() => {
                        var item = listView1.Items[i];
                        iscomplete = false;
                        webBrowser1.Navigate((string)(item as ListViewItem).Tag, null, null, "User-Agent: Mozilla/5.0");
                    }));
                }
                
                
                
            });
            th.IsBackground = true;
            th.Start();


        }
    }
  
}