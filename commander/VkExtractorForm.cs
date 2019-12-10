using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginLib;
using System;
using System.Collections.Generic;
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
            tableLayoutPanel1.RowStyles[1].Height = 0;
            foreach (var item in Stuff.Libraries)
            {
                comboBox4.Items.Add(new ComboBoxItem() { Tag = item, Name = item.Name });
            }
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
                //if (!ar[2].Contains("vk.com")) continue;
                if (!ar[2].Contains("photo")) continue;
                if (ar[2].StartsWith("/photo"))
                {
                    ar[2] = "https://vk.com" + ar[2];
                }
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

        bool strongNameSkip = false;


        List<HttpPageInfo> Infos = new List<HttpPageInfo>();

        public void savePicFunc(string _uri, string str)
        {

            //extract ajax?
            List<int> jsons = new List<int>();
            string temp = str;
            int shift = 0;
            while (true)
            {
                var ind1 = temp.IndexOf("ajax.preload");
                temp = temp.Substring(ind1 + 5);

                if (ind1 == -1) break;
                jsons.Add(ind1 + shift);
                shift += ind1 + 5;
            }

            List<string> jsonRefs = new List<string>();
            foreach (var ind1 in jsons)
            {
                int brctcnt = 0;
                int brctcnt2 = 0;
                bool start = false;
                bool ajaxStart = false;
                StringBuilder sbb = new StringBuilder();
                int? arrStart = null;
                for (int i2 = ind1; i2 < str.Length; i2++)

                {
                    if (str[i2] == '(') { brctcnt2++; ajaxStart = true; }
                    if (ajaxStart)
                    {
                        if (str[i2] == '[')
                        {
                            if (arrStart == null)
                            {
                                arrStart = i2;
                            }
                            brctcnt++; start = true;
                        }
                    }

                    if (str[i2] == ']') brctcnt--;
                    if (str[i2] == ')') brctcnt2--;
                    if (start)
                    {
                        sbb.Append(str[i2]);
                    }
                    if (brctcnt == 0 && start)
                    {
                        break;
                    }
                    if (brctcnt2 == 0 && ajaxStart)
                    {
                        break;
                    }

                }
                if (sbb.Length > 0 && arrStart != null)
                {
                    var sub = str.Substring(arrStart.Value);
                    byte[] byteArray = Encoding.ASCII.GetBytes(sub);
                    MemoryStream stream = new MemoryStream(byteArray);

                    var rdr = new StreamReader(stream);
                    var rdr2 = new JsonTextReader(rdr);
                    bool fetchNext = false;
                    try
                    {
                        while (rdr2.Read())
                        {

                            if (rdr2.ValueType == typeof(string))
                            {

                                var str2 = (string)rdr2.Value;

                                if (rdr2.TokenType == JsonToken.PropertyName)
                                {
                                    if (str2 == "w_src" || str2 == "z_src")
                                    {
                                        fetchNext = true;
                                    }
                                }
                                else
                                {
                                    if (fetchNext)
                                    {
                                        jsonRefs.Add(str2);
                                        fetchNext = false;
                                    }
                                }

                            }
                        }
                    }
                    catch (JsonException ex)
                    {

                    }

                    /*JArray o1 = JArray.Parse(sbb.ToString()); ;

                    WalkNode(o1, n =>
                    {
                        JToken token = n["w_src"];
                        if (token != null && token.Type == JTokenType.String)
                        {
                            string wsrc = token.Value<string>();
                            jsonRefs.Add(wsrc);
                        }
                        token = n["z_src"];
                        if (token != null && token.Type == JTokenType.String)
                        {
                            string zsrc = token.Value<string>();
                            jsonRefs.Add(zsrc);
                        }
                    });*/

                }
            }


            var pi = new HttpPageInfo();
            pi.Uri = _uri;
            Infos.Add(pi);
            //richTextBox2.Text = webBrowser1.DocumentText;


            var list = Stuff.ParseHtmlItems(str, "<meta", "/>");
            var list2 = Stuff.ParseHtmlItems(str, "<a", "/a>");
            var list3 = Stuff.ParseHtmlItems(str, "<img", ">");
            int totalLinks = 0;
            var allLinks = jsonRefs.Union(list).Union(list2).Union(list3).ToArray();
            foreach (var item in allLinks)
            {
                if (!item.Contains("http")) continue;
                var ar1 = item.Split(new char[] { ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var ww = ar1.Where(z => z.Contains("jpg") || z.Contains("png") || z.Contains("gif"));

                foreach (var _fr in ww)
                {

                    var fr = _fr;
                    try
                    {
                        var ind1 = fr.IndexOf("http");
                        if (ind1 > 0)
                        {
                            fr = fr.Substring(ind1);
                        }
                        string end = ".jpg";
                        if (fr.Contains("png"))
                        {
                            end = "png";
                        }
                        if (fr.Contains("gif"))
                        {
                            end = "gif";
                        }

                        var ind2 = fr.IndexOf(end);
                        fr = fr.Substring(0, ind2 + end.Length);

                        pi.Links.Add(new HttpPageInfo() { Uri = fr });
                        totalLinks++;
                        listView2.Invoke((Action)(() =>
                        {
                            listView2.Items.Add(new ListViewItem(fr) { Tag = fr });
                        }));
                        links++;
                        using (WebClient wc = new WebClient())
                        {
                            var uri = new Uri(fr);
                            if (strongNameSkip)
                            {
                                var nm = Path.Combine(saveTarget, uri.Segments.Last());
                                if (File.Exists(nm))
                                {
                                    skipped++;
                                    continue;
                                }
                            }
                            var data = wc.DownloadData(fr);
                            if (data.Length >= 0)
                            {
                                MemoryStream ms = new MemoryStream(data);
                                if (showPics)
                                {
                                    var img = Image.FromStream(ms);
                                    listView2.Invoke((Action)(() =>
                                    {
                                        pictureBox1.Image = img;
                                    }));
                                }
                                ms.Seek(0, SeekOrigin.Begin);
                                var hash = Stuff.CalcMD5(ms);
                                if (md5cache.Add(hash))
                                {
                                    var fp = Path.Combine(saveTarget, uri.Segments.Last());
                                    if (File.Exists(fp))
                                    {
                                        throw new Exception("hash not found, but file exist");
                                    }

                                    if (!useSizeFilter || ((data.Length / 1024) > sizeFilterKb))
                                    {
                                        saved++;
                                        File.WriteAllBytes(fp, data);
                                    }
                                    else
                                    {
                                        skipped++;
                                    }
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


                        break;

                    }
                    catch (Exception ex)
                    {
                        errors++;
                        listView3.Invoke((Action)(() =>
                        {
                            listView3.Items.Add(new ListViewItem(new string[] { fr, ex.Message }) { Tag = fr + ";" + ex });
                        }));
                    }
                }

            }

            if (totalLinks == 0)
            {
                listView3.Invoke((Action)(() =>
                {
                    listView3.Items.Add(new ListViewItem(new string[] { "no links: " + _uri }) { Tag = _uri, BackColor = Color.Yellow });
                }));
            }

        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            Clipboard.SetText((string)listView1.SelectedItems[0].Tag);
            toolStripStatusLabel1.Text = "selected: " + listView1.SelectedItems.Count;
        }



        HashSet<string> md5cache = new HashSet<string>();
        Thread th;


        static void WalkNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    WalkNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, action);
                }
            }
        }

        string saveTarget = string.Empty;
        private void button3_Click(object sender, EventArgs e)
        {
            if (th == null)
            {
                button3.Text = "abort";
            }
            else
            {
                button3.Text = "download all";
                th.Abort();
                th = null;
                return;
            }
            Infos.Clear();
            savePic = true;
            //webBrowser1.ScriptErrorsSuppressed = true;
            int cnt = listView1.Items.Count;
            //calc md5 in target dir
            saveTarget = textBox1.Text;
            if (radioButton1.Checked)
            {
                var lib = (comboBox4.SelectedItem as ComboBoxItem).Tag as FilesystemLibrary;
                saveTarget = lib.BaseDirectory.FullName;
                if (checkBox6.Checked)
                {
                    saveTarget = Path.Combine(saveTarget, textBox4.Text);
                }
            }
            var dir = new DirectoryInfo(saveTarget);
            if (!dir.Exists)
            {
                Stuff.Error("directory " + dir.FullName + " not exist");
                return;
            }
            skipped = 0;
            saved = 0;
            links = 0;
            errors = 0;
            List<string> ss = new List<string>();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                ss.Add((string)listView1.Items[i].Tag);
            }
            th = new Thread(() =>
           {
               statusStrip1.Invoke((Action)(() =>
               {
                   toolStripStatusLabel1.Text = "calculating md5 hashes..";
                   toolStripProgressBar1.Value = 0;
                   toolStripProgressBar1.Visible = true;

               }));
               var fls = dir.GetFiles().ToArray();
               int fcnt = 0;
               foreach (var item in fls)
               {
                   var md5 = Stuff.CalcMD5(new FileInfoWrapper(item.FullName));
                   md5cache.Add(md5);
                   statusStrip1.Invoke((Action)(() =>
                   {
                       toolStripStatusLabel1.Text = "md5 hash: " + fcnt + " / " + fls.Length;
                       toolStripProgressBar1.Value = (int)((fcnt / (float)fls.Length) * 100);
                   }));
                   fcnt++;
               }
               statusStrip1.Invoke((Action)(() =>
               {
                   toolStripStatusLabel1.Text = "starting downloads..";
                   toolStripProgressBar1.Value = 100;
                   toolStripProgressBar1.Visible = true;

               }));

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
                       var item = ss[i];
                       //  iscomplete = false;

                       //using (WebClient wc = new WebClient())
                       {
                           //  AppendHeaders(wc);
                           //  var str = wc.DownloadString(item);

                           var req = HttpWebRequest.Create(item) as HttpWebRequest;

                           AppendHeaders(req);
                           req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                           var resp = req.GetResponse() as HttpWebResponse;
                           if (resp.StatusCode != HttpStatusCode.OK)
                           {

                           }
                           var enc = Encoding.GetEncoding(resp.CharacterSet);
                           var strm = resp.GetResponseStream();

                           StreamReader readStream = new StreamReader(strm, enc);
                           var str = readStream.ReadToEnd();


                           if (savePic)
                           {
                               savePicFunc(item, str);
                           }

                           //webBrowser1.Navigate((string)(item as ListViewItem).Tag, null, null, "User-Agent: Mozilla/5.0");
                           statusStrip1.Invoke((Action)(() =>
                           {
                               toolStripStatusLabel1.Text = "skipped: " + skipped + "  saved: " + saved + "; errors: " + errors + "; links extracted: " + links;
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
               if (checkBox3.Checked)//close on complete
               {
                   Application.Exit();
               }
               button3.Invoke((Action)(() =>
               {
                   button3.Text = "download all";
               }));

               th = null;

           });
            th.IsBackground = true;
            th.Start();


        }


        public void AppendHeaders(WebClient wc)
        {
            wc.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            wc.Headers.Add("Accept-Encoding: gzip, deflate, br");
            wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36");

            wc.Headers.Add("Sec-Fetch-Mode: navigate");
            wc.Headers.Add("Sec-Fetch-Site: none");
            wc.Headers.Add("Sec-Fetch-User: ?1");
            wc.Headers.Add("Upgrade-Insecure-Requests: 1");
        }
        public void AppendHeaders(HttpWebRequest wc)
        {
            wc.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

            wc.Headers.Add("Accept-Encoding: gzip, deflate, br");
            wc.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";

            wc.Headers.Add("Sec-Fetch-Mode: navigate");
            wc.Headers.Add("Sec-Fetch-Site: none");
            wc.Headers.Add("Sec-Fetch-User: ?1");
            wc.Headers.Add("Upgrade-Insecure-Requests: 1");
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
            catch (Exception ex)
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

        int sizeFilterKb = 10;
        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                sizeFilterKb = int.Parse(textBox3.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            strongNameSkip = checkBox4.Checked;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            Clipboard.SetText((string)listView3.SelectedItems[0].Tag);
        }

        private void CopyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listView3.Items.Count; i++)
            {
                sb.AppendLine((string)listView3.Items[i].Tag);
            }
            Clipboard.SetText(sb.ToString());
        }

        private void SaveAsXmlTreeToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            foreach (var item in Infos)
            {
                if (item.Links.Any())
                {
                    sb.AppendLine($"<item uri=\"{item.Uri}\">");
                    foreach (var litem in item.Links)
                    {
                        sb.AppendLine($"<item uri=\"{litem.Uri}\"/>");
                    }
                    sb.AppendLine($"</item>");
                }
                else
                {
                    sb.AppendLine($"<item uri=\"{item.Uri}\"/>");
                }
            }
            sb.AppendLine("</root>");
            Clipboard.SetText(sb.ToString());
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            List<ListViewItem> todel = new List<ListViewItem>();
            foreach (var item in listView1.SelectedItems)
            {
                todel.Add(item as ListViewItem);
            }
            for (int i = 0; i < todel.Count; i++)
            {
                listView1.Items.Remove(todel[i]);
            }
            listView1.EndUpdate();
        }

        private void CheckBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                tableLayoutPanel1.RowStyles[1].Height = 80;
            }
            else
            {
                tableLayoutPanel1.RowStyles[1].Height = 0;
            }
        }

        bool useSizeFilter = true;
        private void CheckBox7_CheckedChanged(object sender, EventArgs e)
        {
            useSizeFilter = checkBox7.Checked;
        }

        private void ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
        }
    }

    public class HttpPageInfo
    {
        public string Uri;
        public List<HttpPageInfo> Links = new List<HttpPageInfo>();
    }
}