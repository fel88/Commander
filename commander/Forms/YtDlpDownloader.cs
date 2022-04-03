using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace commander.Forms
{
    public partial class YtDlpDownloader : Form
    {
        public YtDlpDownloader()
        {
            InitializeComponent();
            if (File.Exists("settings.xml"))
            {
                try
                {
                    var doc = XDocument.Load("settings.xml");
                    textBox1.Text = doc.Element("settings").Element("extras").Element("ytdlp").Element("exePath").Value;
                }catch(Exception ex)
                {

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "exe files|*.exe";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox1.Text = ofd.FileName;

            //update setting
            if (File.Exists("settings.xml"))
            {
                try
                {
                    File.Copy("settings.xml", "settings_temp.xml", true);
                    var doc = XDocument.Load("settings.xml");
                    var fr = doc.Descendants("settings").First();
                    var ext = fr.Element("extras");
                    if (ext == null)
                    {
                        fr.Add(new XElement("extras"));
                        ext = fr.Element("extras");
                    }
                    var ytdlp = ext.Element("ytdlp");
                    if (ytdlp == null)
                    {
                        ext.Add(new XElement("ytdlp"));
                        ytdlp = ext.Element("ytdlp");
                    }
                    var exep = ytdlp.Element("exePath");
                    if (exep == null)
                    {
                        ytdlp.Add(new XElement("exePath"));
                        exep = ytdlp.Element("exePath");
                    }
                    exep.ReplaceNodes(new XCData(textBox1.Text));                    

                    doc.Save("settings.xml");
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                new Uri(textBox2.Text);
                listView1.Items.Add(new ListViewItem(textBox2.Text) { Tag = textBox2.Text });
                textBox2.Text = string.Empty;

                textBox2.BackColor = Color.White;
                textBox2.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
                textBox2.ForeColor = Color.White;
            }
        }

        Process p;
        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text))
            {
                Stuff.Warning($"File {textBox1.Text} doesn't exist");
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in listView1.Items)
            {
                sb.Append((item as ListViewItem).Text + " ");
            }
            Directory.CreateDirectory("Downloads");
            ProcessStartInfo pci = new ProcessStartInfo(textBox1.Text)
            {
                Arguments = sb.ToString(),
                WorkingDirectory = "Downloads",
                CreateNoWindow = true
            };
            p = new Process();
            p.StartInfo = pci;
            p.EnableRaisingEvents = true;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_OutputDataReceived;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.Exited += P_Exited;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            p.BeginOutputReadLine();
        }



        private void P_Exited(object sender, EventArgs e)
        {
            richTextBox1.Invoke((Action)(() =>
            {
                richTextBox1.AppendText("terminated");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }));
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.Contains("%"))
            {
                var arr = e.Data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Contains("%"))
                    {
                        var perc = arr[i].Replace("%", "");
                        try
                        {
                            var val = float.Parse(perc.Replace(",", "."), CultureInfo.InvariantCulture);
                            progressBar1.Invoke((Action)(() =>
                            {
                                progressBar1.Value = (int)Math.Round(val);
                            }));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            richTextBox1.Invoke((Action)(() =>
            {
                richTextBox1.AppendText(e.Data + Environment.NewLine);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }));

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start("Downloads");
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            List<ListViewItem> toDel = new List<ListViewItem>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                toDel.Add(listView1.SelectedItems[i]);
            }
            foreach (var t in toDel)
            {
                listView1.Items.Remove(t);
            }
        }
    }
}
