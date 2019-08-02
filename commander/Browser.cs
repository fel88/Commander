using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                webBrowser1.Navigate(textBox1.Text);
            }
        }

        private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

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
            var url = webBrowser1.Url.ToString();
            if (string.IsNullOrEmpty(url)) { MessageBox.Show("url is empty"); return; }
            using (WebClient wec = new WebClient())
            {
                var data = wec.DownloadData(url);
                //get current library and add file                
                var lib = (sender as ToolStripMenuItem).Tag as ILibrary;
                var l = webBrowser1.Url.Segments.Last();
                lib.AppendFile(l, data);
                MessageBox.Show("File was added.");
            }
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            var arr = textBox2.Text.Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            //var c = comboBox1.SelectedItem as ComboBoxItem;
            //var fsl = c.Tag as FilesystemLibrary;
            
            foreach (var item in arr)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item, "" }) { Tag = item });
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }
    }
}
