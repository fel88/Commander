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
            Stuff.SetDoubleBuffered(listView1);
        }

        public static FileInfo[][] FindRepeats(DirectoryInfo d)
        {
            List<FileInfo> files = new List<FileInfo>();
            Stuff.GetAllFiles(d, files);

            var grp1 = files.GroupBy(z => z.Length).Where(z => z.Count() > 1).ToArray();
            List<FileInfo[]> groups = new List<FileInfo[]>();
            foreach (var item in grp1)
            {
                var arr0 = item.GroupBy(z => Stuff.CalcPartMD5(z.FullName, 1024 * 1024)).ToArray();
                var cnt0 = arr0.Count(z => z.Count() > 1);
                if (cnt0 == 0) continue;
                groups.AddRange(arr0.Select(z => z.ToArray()).ToArray());
            }

            return groups.ToArray();
        }
        public DirectoryInfo Directory;
        public void SetRepeats(DirectoryInfo dir, FileInfo[][] repeats)
        {
            Directory = dir;
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

        private void FolowInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            if (!(listView2.SelectedItems[0].Tag is FileInfo)) return;

            var finfo = listView2.SelectedItems[0].Tag as FileInfo;

            var arr = mdi.MainForm.MdiChildren.OfType<Explorer>().ToArray();
            if (arr.Count() == 0)
            {
                Stuff.Warning("Explorer not opened.");
                return;
            }
            var a = arr[0];
            var f = (a as Explorer).FileListControls[0];
            f.NavigateTo(finfo.DirectoryName);
            f.SetFilter(finfo.Name, true);
            a.Activate();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            var grps = FindRepeats(Directory);
            if (grps.Count() == 0)
            {
                Stuff.Info("No repeats found.");
            }
            SetRepeats(Directory, grps);
        }
    }
}
