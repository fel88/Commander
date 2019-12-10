using BrightIdeasSoftware;
using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace commander
{
    public partial class ZipEditor : Form
    {
        public ZipEditor()
        {
            InitializeComponent();
            treeListView1.MouseDoubleClick += TreeListView1_MouseDoubleClick;
            treeListView1.CheckBoxes = true;
            (treeListView1.Columns[0] as OLVColumn).CheckBoxes = false;
            treeListView1.ShowImagesOnSubItems = true;
            treeListView1.CheckAll();
            treeListView1.ItemChecked += TreeListView1_ItemChecked;
            (treeListView1.Columns[0] as OLVColumn).ImageGetter = (x) =>
              {
                  if (x is IDirectoryInfo d)
                  {
                      return 0;
                  }
                  return null;
              };
            treeListView1.SmallImageList = Stuff.list;

            treeListView1.ChildrenGetter = (x) =>
            {
                if (x is IDirectoryInfo d)
                {
                    List<object> r = new List<object>();
                    r.AddRange(d.GetDirectories());
                    r.AddRange(d.GetFiles());

                    return r.ToArray();
                }
                return null;
            };
            treeListView1.CanExpandGetter = (x) =>
            {
                if (x is IDirectoryInfo d)
                {
                    return d.GetFiles().Any() || d.GetDirectories().Any();
                }
                return false;
            };
        }

        private void TreeListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var rowObject = ((OLVListItem)e.Item).RowObject;
            var col = treeListView1.GetChildren(rowObject);
            foreach (var item in col)
            {
                if (treeListView1.IsChecked(rowObject))
                {
                    treeListView1.CheckObject(item);
                }
                else
                {
                    treeListView1.UncheckObject(item);
                }

            }
        }

        private void TreeListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeListView1.SelectedObject == null) return;
            var d = (treeListView1.SelectedObject);

            if (treeListView1.IsExpanded(d))
            {
                treeListView1.Collapse(d);
            }
            else
            {
                treeListView1.Expand(d);
            }

        }

        public IDirectoryInfo[] Dirs;
        public IFileInfo[] Files;
        public void Init(FilesAndDirectoriesContext ctx)
        {
            if (ctx.Dirs == null)
            {
                ctx.Dirs = new IDirectoryInfo[] { };
            }
            if (ctx.Files == null)
            {
                ctx.Files = new IFileInfo[] { };
            }
            Dirs = ctx.Dirs;
            Files = ctx.Files;
            List<object> r = new List<object>();

            r.AddRange(ctx.Dirs);
            r.AddRange(ctx.Files);

            treeListView1.SetObjects(r);
            treeListView1.CheckAll();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.zip|*.zip";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            using (var archive = ZipFile.Open(sfd.FileName, ZipArchiveMode.Create))
            {
                HashSet<string> hash = new HashSet<string>();
                foreach (var item in treeListView1.CheckedObjects)
                {
                    if (item is IFileInfo f)
                    {
                        hash.Add(f.FullName.ToLower());
                    }
                    if (item is IDirectoryInfo d)
                    {
                        hash.Add(d.FullName.ToLower());
                    }
                }
                foreach (var item in Dirs)
                {
                    if (!hash.Contains(item.FullName.ToLower())) continue;
                    var files = Stuff.GetAllFiles(item);
                    foreach (var fPath in files)
                    {
                        if (!hash.Contains(fPath.FullName.ToLower())) continue;
                        archive.CreateEntryFromFile(fPath.FullName, Path.GetFileName(fPath.FullName));
                    }
                }
                foreach (var item in Files)
                {
                    if (!hash.Contains(item.FullName.ToLower())) continue;
                    archive.CreateEntryFromFile(item.FullName, Path.GetFileName(item.FullName));
                }
            }
            Stuff.Info("Archive was created: " + sfd.FileName);
            Close();
        }
    }
}
