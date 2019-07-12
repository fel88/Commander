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
    public partial class RepeatsWindow : Form
    {
        public RepeatsWindow()
        {
            InitializeComponent();
        }

        public void SetRepeats(FileInfo[][] repeats)
        {
            listView1.Items.Clear();
            foreach (var fileInfo in repeats)
            {
                listView1.Items.Add(new ListViewItem(new string[] { fileInfo.First().Name, Stuff.CalcMD5(fileInfo.First().FullName), fileInfo.Length + "" }) { Tag = fileInfo });
            }

            label1.Text = "Total repeats groups: " + repeats.Length;
            label2.Text = "Total memory overhead: " + ((double)repeats.Sum(z => z.First().Length * (z.Length - 1)) / 1024 / 1024).ToString("F")+"Mb";
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var ff = listView1.SelectedItems[0].Tag as FileInfo[];
                listView2.Items.Clear();
                foreach (var l in ff)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { l.FullName }) { Tag = l });
                }
            }
        }
    }
}
