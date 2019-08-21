using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();
            webBrowser1.ScriptErrorsSuppressed = true;
            Stuff.SetDoubleBuffered(listView1);
            foreach (var item in Stuff.Libraries)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.Name });
            }
        }


        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                webBrowser1.Navigate(textBox1.Text);
            }
        }


        private void ToolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            toolStripDropDownButton1.DropDownItems.Clear();
            foreach (var item in Stuff.Libraries)
            {
                var d = new ToolStripMenuItem(item.Name) { Tag = item, AutoSize = true };
                d.Click += D_Click;
                toolStripDropDownButton1.DropDownItems.Add(d);

            }
        }

        private void D_Click(object sender, EventArgs e)
        {
            var temp1 = ServicePointManager.Expect100Continue;
            var temp2 = ServicePointManager.SecurityProtocol;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var url = webBrowser1.Url.ToString();
            if (string.IsNullOrEmpty(url)) { MessageBox.Show("url is empty"); return; }
            using (WebClient wec = new WebClient())
            {
                var data = wec.DownloadData(url);
                string fileName = "";


                if (!String.IsNullOrEmpty(wec.ResponseHeaders["Content-Disposition"]))
                {
                    var arr1 = wec.ResponseHeaders["Content-Disposition"].Split(new string[] { "filename", "*", "=", "\"", ";", "'" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    fileName = arr1.Last();
                }

                //get current library and add file                
                var lib = (sender as ToolStripMenuItem).Tag as ILibrary;
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = webBrowser1.Url.Segments.Last();
                }
                lib.AppendFile(fileName, data);
                MessageBox.Show("File was added.");
            }
            ServicePointManager.Expect100Continue = temp1;
            ServicePointManager.SecurityProtocol = temp2;
        }



        private void ToolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FilesystemLibrary lib = null;
            if (listView1.Items.Count == 0)
            {
                Stuff.Error("No files to save.");
                return;
            }
            if (radioButton1.Checked)
            {
                if (!Directory.Exists(textBox3.Text))
                {
                    Stuff.Error("Directory not exist: " + textBox3.Text);
                    return;
                }
            }
            else
            {
                if (comboBox1.SelectedItem == null)
                {
                    Stuff.Error("Library to save not selected.");
                    return;
                }
                lib = (comboBox1.SelectedItem as ComboBoxItem).Tag as FilesystemLibrary;
            }
            int skipped = 0;
            foreach (var item in listView1.Items)
            {
                var uri = ((Uri)(item as ListViewItem).Tag);


                using (WebClient wc = new WebClient())
                {
                    var name = uri.Segments.Last();
                    var path = Path.Combine(textBox3.Text, name);
                    if (radioButton2.Checked)
                    {
                        path = Path.Combine((lib as FilesystemLibrary).BaseDirectory.FullName, name);
                    }
                    if (File.Exists(path))
                    {
                        skipped++;
                    }
                    wc.DownloadFile(uri.ToString(), path);
                }
            }
            toolStripStatusLabel1.Text = "Total: " + listView1.Items.Count + "; Skipped: " + skipped;
        }

        private void ToolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uris.Clear();
            listView1.Items.Clear();
        }

        HashSet<string> uris = new HashSet<string>();
        private void TextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                e.SuppressKeyPress = true;
                try
                {
                    var txt = Clipboard.GetText();
                    var arr = txt.Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                    listView1.BeginUpdate();
                    int added = 0;
                    int duplicates = 0;
                    foreach (var item in arr)
                    {
                        var uri = new Uri(item);
                        if (uris.Add(uri.ToString()))
                        {
                            listView1.Items.Add(new ListViewItem(new string[] { item, uri.Segments.Last() }) { Tag = uri });
                            added++;
                        }
                        else
                        {
                            duplicates++;
                        }
                    }
                    toolStripStatusLabel1.Text = "Added: " + added + "; skipped: " + duplicates;

                }
                catch (Exception ex)
                {
                    Stuff.Error(ex.Message);
                }
                finally
                {
                    listView1.EndUpdate();
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
        }
    }
}
