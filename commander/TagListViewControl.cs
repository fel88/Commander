﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using PluginLib;

namespace commander
{
    public partial class TagListViewControl : UserControl, IFileListControl
    {
        public TagListViewControl()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
            listView1.MouseMove += ListView1_MouseMove;

            phelper.Append(this, listView1);

            tuc = new TagPanelHelper() { };


            Stuff.SetDoubleBuffered(tuc);

            tuc.Init(this, null);

            SizeChanged += TagListViewControl_SizeChanged;
            listView1.Controls.Add(tuc);
            Stuff.HelperVisibleChanged += Stuff_HelperVisibleChanged;
        }

        private void Stuff_HelperVisibleChanged(HelperEnum arg1)
        {
            switch (arg1)
            {
                case HelperEnum.TagsHelper:
                    tuc.Visible = Stuff.TagsHelperVisible;
                    break;

            }
        }
        private void TagListViewControl_SizeChanged(object sender, EventArgs e)
        {
            tuc.UpdatePosition();
        }

        TagPanelHelper tuc;

        public void ParentClosing()
        {
            phelper.Stop();
        }
        PreviewHelper phelper = new PreviewHelper();
        private void ListView1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateIcons();
        }

        void UpdateIcons()
        {
            if (listView1.TopItem == null) return;
            for (int i = listView1.TopItem.Index; i < listView1.Items.Count; i++)
            {
                if (!listView1.IsItemVisible(listView1.Items[i]))
                {
                    break;
                }
                if (listView1.Items[i].ImageIndex != -1) continue;

                Tuple<Bitmap, int> tp = null;
                var fileInfo = listView1.Items[i].Tag as IFileInfo;
                if (fileInfo == null) continue;

                if (File.Exists(fileInfo.FullName))
                {
                    tp = Stuff.GetBitmapOfFile(fileInfo.FullName);
                }

                int iindex = -1;
                if (tp != null)
                {
                    iindex = tp.Item2;
                }
                listView1.Items[i].ImageIndex = iindex;

            }
        }
        public void Init(FileListControl fc)
        {
            parent = fc;
            listView1.KeyDown += ListView1_KeyDown;
            tuc.Init();
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelected();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
            }
        }

        FileListControl parent;

        public object tagRootObject = new object();
        public TagInfo SelectedTag
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is TagInfo)
                    {
                        return si as TagInfo;
                    }
                }

                return null;
            }
        }
        public TagInfoCover SelectedTagCover
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is TagInfoCover)
                    {
                        return si as TagInfoCover;
                    }
                }

                return null;
            }
        }
        public TagInfo CurrentTag = null;
        public void UpdateList(TagInfo tag)
        {
            if (tag == null)
            {
                listView1.Items.Clear();
                //listView1.View = View.List;
                parent.CurrentTag = null;

                var filter = parent.Filter;
                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();


                List<TagInfoCover> covers = new List<TagInfoCover>();
                foreach (var item in Stuff.Tags)
                {
                    covers.Add(new TagInfoCover() { Name = item.Name, TagInfo = item, IsMain = true });
                    foreach (var sitem in item.Synonyms)
                    {
                        covers.Add(new TagInfoCover() { Name = sitem, TagInfo = item });
                    }
                }
                foreach (var item in covers.OrderBy(z => z.Name))
                {
                    // if (!Stuff.ShowHidden && item.TagInfo.IsHidden) continue;
                    if (!IsFilterPass(item.Name, fltrs)) continue;
                    if (item.IsMain)
                    {
                        listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.TagInfo.Files.Count() + "" }) { Tag = item.TagInfo });
                    }
                    else
                    {
                        listView1.Items.Add(new ListViewItem(new string[] { item.Name + $" ({item.TagInfo.Name})", item.TagInfo.Files.Count() + "" }) { Tag = item });
                    }

                }
            }
            else
            {

                listView1.Items.Clear();
                //listView1.View = View.Details;                
                //   fc.SetPath(path);
                //textBox1.Text = path;
                parent.CurrentTag = tag;
                CurrentTag = tag;
                parent.SetPath(tag.Name);
                var filter = parent.Filter;

                // var p = new DirectoryInfo(path);
                //   fc.CurrentDirectory = p;


                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = Stuff.list;
                listView1.LargeImageList = Stuff.list;

                listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = tagRootObject });
                foreach (var finfo in tag.Files)
                {
                    try
                    {
                        /*if (!Stuff.ShowHidden)
                        {
                            var tags = Stuff.GetAllTagsOfFile(finfo.FullName);
                            if (tags.Any(z => z.IsHidden))
                            {
                                continue;
                            }
                        }*/
                        var f = finfo;
                        //var tp = FileListControl.GetBitmapOfFile(f.FullName);
                        //bmp.MakeTransparent();
                        // list.Images.Add(bmp);
                        if (!IsFilterPass(f.Name, fltrs)) continue;
                        //  int iindex = -1;
                        /* if (tp != null)
                         {
                             iindex = tp.Item2;
                         }*/
                        listView1.Items.Add(
                            new ListViewItem(new string[]
                            {
                        f.Name, Stuff.GetUserFriendlyFileSize(f.Length)
                            })
                            {
                                Tag = f,
                                //   ImageIndex = iindex

                            });
                    }
                    catch (FileNotFoundException ex)
                    {
                        listView1.Items.Add(
                            new ListViewItem(new string[]
                            {
                                Path.GetFileName(finfo.FullName)
                            })
                            {
                                BackColor = Color.LightPink,
                                Tag = new Tuple<IFileInfo, Exception>(finfo, ex),

                            });
                    }
                }
            }
        }
        public bool IsFilterPass(string str, string[] filters)
        {
            str = str.ToLower();
            if (filters.Length == 0) return true;
            return filters.Any(z => str.Contains(z));
        }
        public IFileInfo SelectedFile
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is IFileInfo)
                    {
                        return si as IFileInfo;
                    }
                }

                return null;
            }
        }
        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (SelectedFileChanged != null)
            {
                SelectedFileChanged(SelectedFile);
            }
            if (parent.SelectedFileChangedDelegates.Any())
            {
                foreach (var item in parent.SelectedFileChangedDelegates)
                {
                    item(SelectedFile);
                }
            }
        }

        private void AddTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ntag = new TagInfo() { Name = "tag1" };
            RenameDialog rd = new RenameDialog();
            rd.Validation = (x) =>
            {
                if (Stuff.IsTagCoverExist(x, ntag.Name))
                {
                    return new Tuple<bool, string>(false, "Same tag name already exist!");
                }
                return new Tuple<bool, string>(true, null);
            };
            rd.Value = ntag.Name;
            if (rd.ShowDialog() == DialogResult.OK)
            {
                ntag.Name = rd.Value;
                Stuff.AddTag(ntag, false);                
                UpdateList(null);
            }
            UpdateList(null);
        }

        void ExecuteSelected()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    UpdateList(tag as TagInfo);
                }
                else
                     if (tag is TagInfoCover tic)
                {
                    UpdateList(tic.TagInfo);
                }
                else
                if (listView1.SelectedItems[0].Tag == tagRootObject)
                {
                    UpdateList(null);
                }
                else if (tag is IFileInfo)
                {
                    var f = tag as IFileInfo;
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.WorkingDirectory = f.DirectoryName;
                    psi.FileName = f.FullName;
                    //Process.Start(f.FullName);
                    Process.Start(psi);
                }
            }
        }
        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExecuteSelected();
        }


        private void TrueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    /*if (!(tag as TagInfo).IsHidden)
                    {
                        (tag as TagInfo).IsHidden = true;
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }*/
                }
            }
        }

        private void FalseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    /*if ((tag as TagInfo).IsHidden)
                    {
                        (tag as TagInfo).IsHidden = false;
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }*/
                }
            }
        }

        void DeleteSelected()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo t)
                {
                    if (Stuff.Question("Are you sure to delete " + t.Name + " tag and all files?") == DialogResult.Yes)
                    {
                        Stuff.DeleteTag(t);

                        UpdateList(null);
                    }
                }
                else if (tag is TagInfoCover tci)
                {
                    if (Stuff.Question($"Are you sure to delete synonym {tci.Name} of tag {tci.TagInfo.Name}?") == DialogResult.Yes)
                    {
                        tci.TagInfo.Synonyms.Remove(tci.Name);
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }
                }
                else if (tag is IFileInfo)
                {
                    var f = tag as IFileInfo;
                    if (Stuff.Question("Are you sure to delete tag " + CurrentTag.Name + " from " + f.Name + " file?") == DialogResult.Yes)
                    {
                        CurrentTag.DeleteFile(f.FullName);
                        UpdateList(CurrentTag);
                    }
                }
                else if (tag is Tuple<IFileInfo, Exception>)
                {
                    var fl = tag as Tuple<IFileInfo, Exception>;
                    var f = fl.Item1;
                    CurrentTag.DeleteFile(f.FullName);
                    UpdateList(CurrentTag);
                }
            }
        }
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void ToIsoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            List<TagInfo> tags = new List<TagInfo>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                if (!(listView1.SelectedItems[i].Tag is TagInfo)) continue;
                tags.Add(listView1.SelectedItems[i].Tag as TagInfo);
            }

            if (tags.Count == 0) return;


            var vfs = new VirtualFilesystem();
            var vdir = new VirtualDirectoryInfo(vfs);
            //vdir.FullName = "z:\\" + tag.Name;
            vdir.FullName = "z:\\";
            vdir.Name = "z:\\";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "iso";
            sfd.Filter = "iso images|*.iso";
            List<IFileInfo> flss = new List<IFileInfo>();
            foreach (var tag in tags)
            {
                flss.AddRange(tag.Files.Where(z => z.Exist));
            }

            var gg = flss.GroupBy(z => z.FullName.ToLower()).ToArray();
            flss.Clear();
            foreach (var item in gg)
            {
                flss.Add(item.First());
            }

            var ord = flss.GroupBy(z => z.FullName.ToLower()).ToArray();
            var ord2 = flss.GroupBy(z => z.Name.ToLower()).ToArray();
            if (ord2.Any(z => z.Count() > 1))
            {
                var fer = ord2.Where(z => z.Count() > 1).Sum(z => z.Count());
                Stuff.Warning(fer + " files has same names. Pack impossible");
                if (Stuff.Question("Open pack editor?") == DialogResult.Yes)
                {
                    PackEditor p = new PackEditor();
                    p.Init(flss);
                    p.MdiParent = mdi.MainForm;
                    p.Show();
                }
                return;
            }

            flss = ord.Select(z => z.First()).ToList();
            flss = flss.Where(z => z.Length > 0).ToList();

            vdir.ChildsFiles.AddRange(flss.Select(z => new VirtualFileInfo(z, vdir) { Directory = vdir }));

            vfs.Files = vdir.ChildsFiles.OfType<VirtualFileInfo>().ToList();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PackToIsoSettings stg = new PackToIsoSettings();
                stg.Dirs.Add(vdir);
                stg.Path = sfd.FileName;
                stg.IncludeMeta = true;
                stg.Root = vdir;
                stg.AfterPackFinished = () => { Stuff.Info("Pack complete!"); };
                if (tags.Count == 1)
                {
                    stg.VolumeId = tags.First().Name.Replace(' ', '_');
                }
                else
                {
                    stg.VolumeId = $"Volume[{tags.Count} tags]";
                }
                if (stg.VolumeId.Length > 32)
                {
                    stg.VolumeId = stg.VolumeId.Substring(0, 32);
                }
                Stuff.PackToIso(stg);
            }
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var tag = listView1.SelectedItems[0].Tag;
            if (tag is IFileInfo)
            {
                var f = tag as IFileInfo;
                Clipboard.SetText(f.FullName);
            }
            if (tag is Tuple<IFileInfo, Exception> tuple)
            {
                Clipboard.SetText(tuple.Item1.FullName);
            }

        }


        public Action<IFileInfo> FollowAction;

        public event Action<IFileInfo> SelectedFileChanged;

        private void FollowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FollowAction == null) return;
            if (SelectedFile == null) return;
            FollowAction(SelectedFile);
        }

        private void TagPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            QuickTagsWindow q = new QuickTagsWindow();

            q.Init(this, SelectedFile);
            q.MdiParent = mdi.MainForm;
            q.TopLevel = false;
            q.Show();
        }

        private void IndexToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void Rename()
        {
            if (SelectedTagCover != null)
            {
                RenameDialog rd = new RenameDialog();
                rd.Validation = (x) =>
                {
                    if (Stuff.IsTagCoverExist(x, SelectedTagCover.Name))
                    {
                        return new Tuple<bool, string>(false, "Same tag name already exist!");
                    }
                    return new Tuple<bool, string>(true, null);
                };
                rd.Value = SelectedTagCover.Name;
                if (rd.ShowDialog() == DialogResult.OK)
                {
                    Stuff.RenameTag(SelectedTagCover, rd.Value);
                    UpdateList(null);
                }
                return;
            }
            {
                if (SelectedTag == null) return;

                RenameDialog rd = new RenameDialog();
                rd.Validation = (x) =>
                {
                    if (Stuff.IsTagExist(x, SelectedTag))
                    {
                        return new Tuple<bool, string>(false, "Same tag name already exist!");
                    }
                    return new Tuple<bool, string>(true, null);
                };
                rd.Value = SelectedTag.Name;
                if (rd.ShowDialog() == DialogResult.OK)
                {
                    Stuff.RenameTag(SelectedTag, rd.Value);
                    UpdateList(null);
                }
            }
        }

        private void OcrToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void OcrToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            Stuff.OCRFile(SelectedFile);
        }

        private void IndexToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            Stuff.IndexFile(SelectedFile);
        }

        private void DeduplicationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var tag = listView1.SelectedItems[0].Tag;
            if (tag is TagInfo)
            {
                var dd = listView1.SelectedItems[0].Tag as TagInfo;
                var files = dd.Files.Select(z => z);

                DedupContext ctx = new DedupContext(new IDirectoryInfo[] { }, files.OfType<IFileInfo>().ToArray());
                ProgressBarOperationDialog pd = new ProgressBarOperationDialog();
                IFileInfo[][] groups = null;
                pd.Init(() =>
                {
                    groups = RepeatsWindow.FindDuplicates(ctx, (p, max, title) => pd.SetProgress(title, p, max));
                    pd.Complete();
                });
                pd.ShowDialog();
                if (pd.DialogResult == DialogResult.Abort)
                {
                    return;
                }
                if (groups.Count() == 0)
                {
                    Stuff.Info("No duplicates found.");
                }
                else
                {
                    RepeatsWindow rp = new RepeatsWindow();
                    rp.MdiParent = mdi.MainForm;
                    rp.SetGroups(ctx, groups.ToArray());
                    rp.Show();
                }
            }
            else
            {
                List<IFileInfo> ff = new List<IFileInfo>();
                List<IDirectoryInfo> dd = new List<IDirectoryInfo>();
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    var tag0 = listView1.SelectedItems[i].Tag;
                    if (tag0 is IFileInfo)
                    {
                        ff.Add(tag0 as IFileInfo);
                    }
                    if (tag0 is IDirectoryInfo)
                    {
                        dd.Add(tag0 as IDirectoryInfo);
                    }
                }
                DedupContext ctx = new DedupContext(dd.ToArray(), ff.ToArray());
                ProgressBarOperationDialog pd = new ProgressBarOperationDialog();
                IFileInfo[][] groups = null;
                pd.Init(() =>
                {
                    groups = RepeatsWindow.FindDuplicates(ctx, (p, max, title) => pd.SetProgress(title, p, max));
                    pd.Complete();
                });
                pd.ShowDialog();
                if (pd.DialogResult == DialogResult.Abort)
                {
                    return;
                }
                if (groups.Count() == 0)
                {
                    Stuff.Info("No duplicates found.");
                }
                else
                {
                    RepeatsWindow rp = new RepeatsWindow();
                    rp.MdiParent = mdi.MainForm;
                    rp.SetGroups(ctx, groups.ToArray());
                    rp.Show();
                }
            }
        }


        private void MemToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            List<IFileInfo> files = new List<IFileInfo>();
            long total = 0;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var tag1 = listView1.SelectedItems[i].Tag;
                if (tag1 is TagInfo)
                {
                    var list = (tag1 as TagInfo).Files.Select(z => z);
                    files.AddRange(list);
                }
                if (tag1 is IFileInfo)
                {
                    files.Add(tag1 as IFileInfo);
                }
            }

            var ff = files.GroupBy(z => z.FullName).ToArray();
            total += ff.Select(z => z.First()).Sum(z => z.Length);
            Stuff.Info("Total size: " + Stuff.GetUserFriendlyFileSize(total));
        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var tag = listView1.SelectedItems[0].Tag;
            if (tag is IFileInfo fi)
            {
                FileMetaInfoEditorDialog d = new FileMetaInfoEditorDialog();
                d.Init(fi);
                d.ShowDialog();
            }
            if (tag is TagInfo ti)
            {
                TagPropertyDialog d = new TagPropertyDialog();
                d.Init(ti);
                d.ShowDialog();
                if (d.Changed)
                {
                    UpdateList(CurrentTag);
                }
            }
            if (tag is TagInfoCover tic)
            {
                TagPropertyDialog d = new TagPropertyDialog();
                d.Init(tic.TagInfo);
                d.ShowDialog();
                if (d.Changed)
                {
                    UpdateList(CurrentTag);
                }
            }

        }

        private void AddSynonymToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is TagInfo)
            {
                (tag as TagInfo).Synonyms.Add((tag as TagInfo).Name + ": synonym01");
                Stuff.IsDirty = true;
                UpdateList(null);
            }
        }

        private void ImgDedupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var tag = listView1.SelectedItems[0].Tag;
            if (tag is TagInfo)
            {
                var dd = listView1.SelectedItems[0].Tag as TagInfo;
                var files = dd.Files.Select(z => z);

                DedupContext ctx = new DedupContext(new IDirectoryInfo[] { }, files.OfType<IFileInfo>().ToArray());

                ProgressBarOperationDialog pd = new ProgressBarOperationDialog();
                IFileInfo[][] groups = null;
                pd.Init(() =>
                {
                    groups = ImagesDeduplicationWindow.FindDuplicates(ctx, (p, max, title) => pd.SetProgress(title, p, max));
                    pd.Complete();
                });
                pd.ShowDialog();
                if (pd.DialogResult == DialogResult.Abort)
                {
                    return;
                }


                if (groups.Count() == 0)
                {
                    Stuff.Info("No duplicates found.");
                }
                else
                {
                    ImagesDeduplicationWindow rp = new ImagesDeduplicationWindow();
                    rp.MdiParent = mdi.MainForm;
                    rp.SetGroups(ctx, groups.ToArray());
                    rp.Show();
                }
            }
            else
            {
                List<IFileInfo> ff = new List<IFileInfo>();
                List<IDirectoryInfo> dd = new List<IDirectoryInfo>();
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    var tag0 = listView1.SelectedItems[i].Tag;
                    if (tag0 is IFileInfo)
                    {
                        ff.Add(tag0 as IFileInfo);
                    }
                    if (tag0 is IDirectoryInfo)
                    {
                        dd.Add(tag0 as IDirectoryInfo);
                    }
                }
                DedupContext ctx = new DedupContext(dd.ToArray(), ff.ToArray());
                ProgressBarOperationDialog pd = new ProgressBarOperationDialog();
                IFileInfo[][] groups = null;
                pd.Init(() =>
                {
                    groups = ImagesDeduplicationWindow.FindDuplicates(ctx, (p, max, title) => pd.SetProgress(title, p, max));
                    pd.Complete();
                });
                pd.ShowDialog();
                if (pd.DialogResult == DialogResult.Abort)
                {
                    return;
                }
                if (groups.Count() == 0)
                {
                    Stuff.Info("No duplicates found.");
                }
                else
                {
                    ImagesDeduplicationWindow rp = new ImagesDeduplicationWindow();
                    rp.MdiParent = mdi.MainForm;
                    rp.SetGroups(ctx, groups.ToArray());
                    rp.Show();
                }

            }
        }
    }

    public class TagInfoCover
    {
        public TagInfo TagInfo;
        public string Name;
        public bool IsMain;
    }
}
