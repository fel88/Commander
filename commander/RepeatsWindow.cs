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

        public static IFileInfo[][] FindDuplicates(DedupContext dup)
        {
            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var d in dup.Dirs)
            {
                Stuff.GetAllFiles(d, files);
            }
            files.AddRange(dup.Files);
            files = files.Where(z => z.Exist && z.Length > 0).ToList();


            var grp1 = files.GroupBy(z => z.Length).Where(z => z.Count() > 1).ToArray();
            List<IFileInfo[]> groups = new List<IFileInfo[]>();
            foreach (var item in grp1)
            {
                var arr0 = item.GroupBy(z => Stuff.CalcPartMD5(z.FullName, 1024 * 1024)).ToArray();
                var cnt0 = arr0.Count(z => z.Count() > 1);
                if (cnt0 == 0) continue;
                groups.AddRange(arr0.Select(z => z.ToArray()).ToArray());
            }

            //todo: binary compare candidates
            return groups.ToArray();
        }

        public DedupContext Context;
        public void SetGroups(DedupContext ctx, IFileInfo[][] repeats)
        {
            Context = ctx;
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
                var ff = listView1.SelectedItems[0].Tag as IFileInfo[];
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
                var fi = listView2.SelectedItems[0].Tag as IFileInfo;
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
                if (listView2.SelectedItems[0].Tag is IFileInfo)
                {
                    var f = listView2.SelectedItems[0].Tag as IFileInfo;
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
            if (!(listView2.SelectedItems[0].Tag is IFileInfo)) return;

            var finfo = listView2.SelectedItems[0].Tag as IFileInfo;

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
            var grps = FindDuplicates(Context);
            if (grps.Count() == 0)
            {
                Stuff.Info("No duplicates found.");
            }
            SetGroups(Context, grps);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }


        private void DeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
