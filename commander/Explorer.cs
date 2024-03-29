﻿using IsoLib;
using isoViewer;
using PluginLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace commander
{
    public partial class Explorer : Form
    {
        public Explorer()
        {
            InitializeComponent();
            var drvs = DriveInfo.GetDrives();
            fileListControl1.UpdateList(new DirectoryInfoWrapper(drvs[0].Name), "");
            fileListControl1.TabOwnerString = "left";
            fileListControl2.UpdateList(new DirectoryInfoWrapper(drvs[0].Name), "");
            fileListControl2.TabOwnerString = "right";
            fileListControl1.MountIsoAction = mouseIsoAction;
            fileListControl2.MountIsoAction = mouseIsoAction;
            fileListControl1.IsoExtractAction = isoExtractAction;
            fileListControl2.IsoExtractAction = isoExtractAction;

            Shown += Explorer_Shown;

            fileListControl1.DeleteFileAction += FileListControl1_DeleteFileAction;
            fileListControl2.DeleteFileAction += FileListControl1_DeleteFileAction;

            fileListControl1.FollowAction += followAction;
            fileListControl2.FollowAction += followAction;
            UpdateTabs();

            // previewer = new ImgViewerPanel() { Dock = DockStyle.Fill };

            PreviewExtensions.Add(new GifPreviewExtension());
            PreviewExtensions.Add(new DjvuPreviewExtension());
            PreviewExtensions.Add(new TextPreviewExtension());
            PreviewExtensions.Add(new VideoPreviewExtension());
            PreviewExtensions.Add(new PdfPreviewExtension());
            PreviewExtensions.Add(new ImgPreviewExtension());
            PreviewExtensions.Add(new OdtPreviewExtension());
            PreviewExtensions.Add(new WebpPreviewExtension());

            fileListControl1.AddSelectedFileChangedAction((x) =>
               {
                   if (!IsPreviewMode) return;

                   splitContainer1.Panel2.Controls.Clear();

                   foreach (var item in PreviewExtensions)
                   {
                       if (item.Extensions.Contains(x.Extension.ToLower()))
                       {
                           var cntrl = item.Fabric(x);
                           splitContainer1.Panel2.Controls.Add(cntrl);
                           if (cntrl is IFileListConsumer flc)
                           {
                               flc.SetFileList(fileListControl1);
                           }
                           CurrentPreviewer = item;
                       }
                       else
                       {
                           item.Deselect();
                       }
                   }
               });
            Load += Explorer_Load;
        }

        private void Explorer_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;
        public static List<ExplorerPreviewExtension> PreviewExtensions = new List<ExplorerPreviewExtension>();

        public ExplorerPreviewExtension CurrentPreviewer = null;



        private void Explorer_Shown(object sender, EventArgs e)
        {
            fileListControl1.Init();
            fileListControl2.Init();
        }

        private void FileListControl1_DeleteFileAction(FileListControl arg1, IFileInfo arg2)
        {
            foreach (var item in PreviewExtensions)
            {
                item.Release(arg2);
            }
        }

        private void followAction(FileListControl fc, IFileInfo f)
        {
            if (fc == fileListControl1)
            {
                fileListControl2.FollowToFile(f);
            }
            else
            {
                fileListControl1.FollowToFile(f);
            }

        }
        private void isoExtractAction(FileListControl arg1, IFileInfo arg2)
        {
            var target = arg1;
            if (target == fileListControl1)
            {
                target = fileListControl2;
            }
            else
            {
                target = fileListControl1;
            }
            using (var fs = new FileStream(arg2.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var pvd = reader.WorkPvd;

                string savePath = Path.Combine(target.CurrentDirectory.FullName, Path.GetFileNameWithoutExtension(arg2.Name));
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                var ret = DirectoryRecord.GetAllRecords(pvd.RootDir);

                foreach (var directoryRecord in ret)
                {
                    if (!directoryRecord.IsDirectory) continue;
                    if (directoryRecord.LBA == pvd.RootDir.LBA) continue;
                    if (directoryRecord.Parent != null && directoryRecord.Parent.LBA == directoryRecord.LBA) continue;
                    if (directoryRecord.Parent != null &&
                        directoryRecord.Parent.Parent != null &&
                        directoryRecord.Parent.Parent.LBA == directoryRecord.LBA) continue;

                    var pp = Path.Combine(savePath, directoryRecord.FullPath);
                    if (!Directory.Exists(pp))
                    {
                        Directory.CreateDirectory(pp);
                    }
                }


                foreach (var directoryRecord in ret)
                {
                    if (!directoryRecord.IsFile) continue;
                    var data = directoryRecord.GetFileData(fs, pvd);
                    var pp = Path.Combine(savePath, directoryRecord.FullPath);
                    File.WriteAllBytes(pp, data);
                }

                Stuff.Info("Extraction complete!");
            }
        }

        private void mouseIsoAction(FileListControl sender, IFileInfo obj, IDirectoryInfo target)
        {
            var t = fileListControl1;
            if (target == null)
            {

                if (t == sender)
                {
                    t = fileListControl2;
                }
                target = t.CurrentDirectory;
            }
            if (Stuff.Question("Mount: " + obj.FullName + " to " + target.FullName + "?") == DialogResult.Yes)
            {
                var minf = new MountInfo();
                minf.IsoPath = obj;
                minf.Path = target.FullName;
                minf.IsMounted = true;
                if (minf.Reader == null)
                {
                    IsoReader reader = new IsoReader();
                    reader.Parse(minf.IsoPath.FullName);
                    minf.Reader = reader;
                }
                var r = new IsoDirectoryInfoWrapper(minf, minf.Reader.WorkPvd.RootDir);
                r.Parent = new DirectoryInfoWrapper(minf.Path);
                minf.MountTarget = r;
                r.Filesystem = new IsoFilesystem(minf) { IsoFileInfo = minf.IsoPath };

                Stuff.MountIso(minf);
                if (target == null)
                {
                    t.UpdateList(target.FullName);
                }
            }
        }

        public FileListControl[] FileListControls => new[] { fileListControl1, fileListControl2 };

        private void UpdateTabs()
        {
            foreach (var item in Stuff.Tabs)
            {
                if (item.Owner == fileListControl1.TabOwnerString)
                {
                    fileListControl1.AddTab(item);
                }
                if (item.Owner == fileListControl2.TabOwnerString)
                {
                    fileListControl2.AddTab(item);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                /*FileListControl fc = null;
                if (fileListControl1.ContainsFocus)
                {
                    fc = fileListControl1;
                }
                if (fileListControl2.ContainsFocus)
                {
                    fc = fileListControl2;
                }

                if (fc != null && fc.ListView.Focused && fc.SelectedFile != null)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.WorkingDirectory = fc.SelectedFile.DirectoryName;
                    psi.FileName = fc.SelectedFile.FullName;

                    Process.Start(psi);

                }*/

            }

            //if (e.KeyCode == Keys.Delete)
            //{
            //    FileListControl fc = null;
            //    if (fileListControl1.ContainsFocus)
            //    {
            //        fc = fileListControl1;
            //    }
            //    if (fileListControl2.ContainsFocus)
            //    {
            //        fc = fileListControl2;
            //    }

            //    if (fc != null && fc.ListView.Focused)
            //    {
            //        if (Stuff.Question("Delete file: " + fc.SelectedFile.FullName + "?") == DialogResult.Yes)
            //        {
            //            var attr = File.GetAttributes(fc.SelectedFile.FullName);
            //            bool allow = true;
            //            if (attr.HasFlag(FileAttributes.ReadOnly))
            //            {
            //                if (Stuff.Question("File is read-only, do you want to delete it anyway?") != DialogResult.Yes)
            //                {
            //                    allow = false;
            //                }
            //                else
            //                {
            //                    File.SetAttributes(fc.SelectedFile.FullName, FileAttributes.Normal);
            //                }
            //            }
            //            if (allow)
            //            {
            //                File.Delete(fc.SelectedFile.FullName);
            //            }
            //        }
            //    }
            //}

            if (e.KeyCode == Keys.F2)
            {//rename
                FileListControl fc = null;
                if (fileListControl1.ContainsFocus)
                {
                    fc = fileListControl1;
                }
                if (fileListControl2.ContainsFocus)
                {
                    fc = fileListControl2;
                }
                if (fc != null)
                {
                    fc.Rename();
                }
            }

            if (Stuff.Hotkeys.Any(z => z.Hotkey == e.KeyCode && z.IsEnabled))
            {
                var cc = Stuff.Hotkeys.First(z => z.Hotkey == e.KeyCode);

                IFileListControl fl = fileListControl1;
                if (fileListControl2.ContainsFocus)
                {
                    fl = fileListControl2;
                }
                cc.Execute(fl);
            }


            if (e.KeyCode == Keys.F5)
            {//copy

                if (fileListControl1.Mode == ViewModeEnum.Filesystem && fileListControl2.Mode == ViewModeEnum.Filesystem)
                {

                    var from = fileListControl1;
                    var to = fileListControl2;
                    if (fileListControl2.ContainsFocus)
                    {
                        from = fileListControl2;
                        to = fileListControl1;
                    }
                    /*if (from.SelectedFiles != null)
                    {
                        bool allow = true;
                        var p1 = Path.Combine(to.CurrentDirectory.FullName,
                            from.SelectedFile.Name);
                        if (File.Exists(p1))
                        {
                            if (MessageBox.Show(
                                    "File " + p1 + " already exist. replace?", Text,
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                            {
                                allow = false;
                            }
                        }

                        if (allow)
                        {
                            File.Copy(from.SelectedFile.FullName, p1, true);
                            to.UpdateList(to.CurrentDirectory.FullName);
                        }
                    }*/
                    var self = from.SelectedFiles;
                    var seld = from.SelectedDirectories;
                    if (seld != null || self != null)
                    {
                        //copy recursively all files and dirs
                        var prnt = from.CurrentDirectory;
                        List<IFileInfo> list = new List<IFileInfo>();
                        List<IDirectoryInfo> dirs = new List<IDirectoryInfo>();
                        if (self != null)
                        {
                            list.AddRange(self);
                        }
                        if (seld != null)
                        {
                            foreach (var item in seld)
                            {
                                Stuff.GetAllFiles(item, list);
                                Stuff.GetAllDirs(item, dirs);
                            }

                        }


                        CopyDialog cpd = new CopyDialog();
                        cpd.Init(list.ToArray(), dirs.ToArray(), to.CurrentDirectory, prnt);
                        cpd.ShowDialog();

                    }
                }

                if ((fileListControl1.Mode == ViewModeEnum.Filesystem || fileListControl1.Mode == ViewModeEnum.Libraries)
                    && fileListControl2.Mode == ViewModeEnum.Tags)
                {
                    if (fileListControl1.SelectedFile != null && fileListControl2.CurrentTag != null)
                    {
                        var fn = fileListControl1.SelectedFile;
                        if (!fileListControl2.CurrentTag.ContainsFile(fn))
                        {
                            fileListControl2.CurrentTag.AddFile(fn);
                            if (Stuff.AllowNTFSStreamsSync)
                                Stuff.UpdateFileMetaInfo(fn);
                            MessageBox.Show(Path.GetFileName(fn.FullName) + " tagged as " + fileListControl2.CurrentTag.Name);
                            fileListControl2.UpdateTagsList();
                        }
                    }
                }
            }
            base.OnKeyDown(e);
        }



        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            TextSearchForm tsf = new TextSearchForm();
            tsf.MdiParent = MdiParent;
            if (fileListControl2.ContainsFocus)
            {
                tsf.SetPath(fileListControl2.CurrentDirectory.FullName);
            }
            else
            {
                tsf.SetPath(fileListControl1.CurrentDirectory.FullName);
            }
            tsf.Show();
        }

        private void fileListControl1_Load(object sender, EventArgs e)
        {

        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {

        }

        private void TablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsPreviewMode = false;
            RemoveSecondTab();
            splitContainer1.Panel2.Controls.Add(fileListControl2);

        }

        void RemoveSecondTab()
        {
            splitContainer1.Panel2.Controls.Clear();
        }



        bool IsPreviewMode = false;
        private void TablePreviewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsPreviewMode = true;
            RemoveSecondTab();
            //splitContainer1.Panel2.Controls.Add(previewer);
        }

        private void CompareBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListControl1.SelectedFile != null && fileListControl2.SelectedFile != null)
            {

                var f1 = fileListControl1.SelectedFile.FullName;
                var f2 = fileListControl2.SelectedFile.FullName;
                var b1 = File.ReadAllBytes(f1);
                var b2 = File.ReadAllBytes(f2);

                if (b1.Length != b2.Length)
                {
                    MessageBox.Show("NOT equal");
                    return;
                }
                for (int i = 0; i < b1.Length; i++)
                {
                    if (b1[i] != b2[i])
                    {
                        MessageBox.Show("NOT equal");
                        return;
                    }
                }
                MessageBox.Show("Equal!");


            }
        }

        private void CompareMD5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListControl1.SelectedFile != null && fileListControl2.SelectedFile != null)
            {
                string hash1 = "";
                string hash2 = "";
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileListControl1.SelectedFile.FullName))
                    {
                        hash1 = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

                    }
                }
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileListControl2.SelectedFile.FullName))
                    {
                        hash2 = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

                    }
                }

                if (hash2 == hash1)
                {
                    MessageBox.Show("md5 equal: " + hash1);
                }
                else
                {
                    MessageBox.Show(hash1 + " != " + hash2);
                }
            }
            if (fileListControl1.SelectedDirectory != null && fileListControl2.SelectedDirectory != null)
            {
                //compare per file
                var fls = fileListControl1.SelectedDirectory.GetFiles();
                var fls2 = fileListControl2.SelectedDirectory.GetFiles();
                var nms1 = fls.Select(z => z.Name).ToArray();
                var nms2 = fls2.Select(z => z.Name).ToArray();
                HashSet<string> ss = new HashSet<string>();
                foreach (var item in nms1)
                {
                    ss.Add(item);
                }
                List<string> diff = new List<string>();
                foreach (var item in nms2)
                {
                    if (ss.Add(item))
                    {
                        diff.Add(item);
                    }
                }
                if (diff.Any())
                {
                    Stuff.Warning("There are " + diff.Count + " diff file names.");
                }
                else
                {
                    Stuff.Info("All names are equal");
                }
            }
        }


        private void Explorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileListControl1.ParentClosing();
            fileListControl2.ParentClosing();
        }

        private void HideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stuff.SetShowHidden(false);
            fileListControl1.UpdateAllLists();
            fileListControl2.UpdateAllLists();
        }

        private void UnhideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasswordDialog pdlg = new PasswordDialog();
            if (pdlg.ShowDialog() == DialogResult.OK)
            {
                Stuff.SetShowHidden(true);
                fileListControl1.UpdateAllLists();
                fileListControl2.UpdateAllLists();
            }
        }

        private void ChangePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangePasswordDialog p = new ChangePasswordDialog();
            p.ShowDialog();
        }



        private void ToolStripButton1_Click_1(object sender, EventArgs e)
        {
            var temp = fileListControl1.CurrentDirectory.FullName;
            var temp2 = fileListControl1.Filter;
            var temp3 = fileListControl1.DirFilterEnable;

            fileListControl1.SetPath(fileListControl2.CurrentDirectory.FullName);
            fileListControl1.UpdateList();
            fileListControl1.SetFilter(fileListControl2.Filter, fileListControl2.DirFilterEnable);


            fileListControl2.SetPath(temp);
            fileListControl2.UpdateList();
            fileListControl2.SetFilter(temp2, temp3);
        }

        private void InsertClipboardAsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameDialog rdl = new RenameDialog();
            rdl.StartPosition = FormStartPosition.CenterParent;

            if (rdl.ShowDialog() == DialogResult.OK)
            {
                FileListControl flc = fileListControl1;
                if (fileListControl2.ContainsFocus)
                {
                    flc = fileListControl2;
                }

                var img = Clipboard.GetImage();
                img.Save(Path.Combine(flc.CurrentDirectory.FullName, rdl.Value));

            }
        }

        private void ScannerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScannerWindow s = new ScannerWindow();
            var fc = fileListControl1;
            if (fileListControl2.ContainsFocus)
            {
                fc = fileListControl2;
            }
            s.Init(fc.CurrentDirectory);
            s.MdiParent = MdiParent;
            s.Show();
        }

        private void syncTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void currentDirectoryOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fc = fileListControl1;
            if (fileListControl2.ContainsFocus)
            {
                fc = fileListControl2;
            }

            foreach (var item in fc.CurrentDirectory.GetFiles())
            {
                Stuff.SyncMetaInfo(item);
            }
        }

        private async void recursevelyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fc = fileListControl1;
            if (fileListControl2.ContainsFocus)
            {
                fc = fileListControl2;
            }
            toolStripProgressBar1.Visible = true;
            toolStripProgressLabel.Visible = true;

            await Task.Run(() =>
            {
                var dirs = Stuff.GetAllDirs(fc.CurrentDirectory);
                int cnt = 0;
                toolStripProgressBar1.Maximum = dirs.Count;
                foreach (var item in dirs)
                {
                    cnt++;
                    toolStripProgressBar1.Value = cnt;
                    toolStripProgressLabel.Text = $"{cnt} / {dirs.Count}";
                    foreach (var f in item.GetFiles())
                    {
                        Stuff.SyncMetaInfo(f);
                    }
                }
            });
            toolStripProgressBar1.Visible = false;
            toolStripProgressLabel.Visible = false;
        }
    }


}



