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
            foreach (var fileInfo in repeats.OrderByDescending(z => z.First().Length * z.Length))
            {
                listView1.Items.Add(new ListViewItem(new string[] { fileInfo.First().Name,
                    Stuff.CalcPartMD5(fileInfo.First().FullName,1024*1024),
                    fileInfo.Length + "" ,
                    Stuff.GetUserFriendlyFileSize((fileInfo.First().Length*(fileInfo.Length-1)))
                })
                { Tag = fileInfo });
            }

            label1.Text = "Total repeats groups: " + repeats.Length;
            label2.Text = "Total memory overhead: " + Stuff.GetUserFriendlyFileSize(repeats.Sum(z => z.First().Length * (z.Length - 1)));

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var ff = listView1.SelectedItems[0].Tag as FileInfo[];
                listView2.Items.Clear();
                foreach (var l in ff)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { l.FullName, Stuff.GetUserFriendlyFileSize(l.Length) }) { Tag = l });
                }
                listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private void CopyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var fi = listView2.SelectedItems[0].Tag as FileInfo;
                Clipboard.SetText(fi.FullName);
            }
        }

        private void ListView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExecuteSelected();
        }

        void ExecuteSelected()
        {
            if (listView2.SelectedItems.Count > 0)
            {
                if (listView2.SelectedItems[0].Tag is FileInfo)
                {
                    var f = listView2.SelectedItems[0].Tag as FileInfo;
                    Stuff.ExecuteFile(f);
                }
            }
        }
        private void ExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteSelected();
        }
    }
}
