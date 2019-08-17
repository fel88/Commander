using isoViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            fileListControl1.IsoExtractAction = isoExtractAction;
            Shown += Explorer_Shown;

            fileListControl1.DeleteFileAction += FileListControl1_DeleteFileAction;
            fileListControl2.DeleteFileAction += FileListControl1_DeleteFileAction;

            fileListControl1.FollowAction += followAction;
            fileListControl2.FollowAction += followAction;
            UpdateTabs();

            previewer = new ImgViewerPanel() { Dock = DockStyle.Fill };
            gpreviewer = new GifViewerPanel() { Dock = DockStyle.Fill };
            vpreviewer = new VideoPlayer() { Dock = DockStyle.Fill };
            textPreviewer = new TextPreviewer() { Dock = DockStyle.Fill };
            dpreviewer = new DjvuReader() { Dock = DockStyle.Fill };

            fileListControl1.AddSelectedFileChangedAction((x) =>
           {
               if (!IsPreviewMode) return;

               string[] exts = new string[] { ".txt", ".cs", ".js", ".xml", ".htm", ".bat", ".html", ".log", ".csproj", ".config", ".resx", ".sln", ".settings", ".md", ".cpp", ".h", ".asm" };
               splitContainer1.Panel2.Controls.Remove(gpreviewer);
               splitContainer1.Panel2.Controls.Remove(previewer);
               splitContainer1.Panel2.Controls.Remove(vpreviewer);
               splitContainer1.Panel2.Controls.Remove(textPreviewer);
               splitContainer1.Panel2.Controls.Remove(dpreviewer);
               if (exts.Contains(x.Extension))
               {
                   splitContainer1.Panel2.Controls.Add(textPreviewer);
                   textPreviewer.LoadFile(x);
               }
               else
               {
                   textPreviewer.Disable();
               }


               string[] gexts = new string[] { ".jpg", ".png", ".bmp" };
               if (new string[] { ".djvu" }.Contains(x.Extension.ToLower()))
               {
                   splitContainer1.Panel2.Controls.Add(dpreviewer);
                   dpreviewer.Init(x.FullName);
               }
               else
               {
                   dpreviewer.UnloadBook();
               }
               if (gexts.Contains(x.Extension.ToLower()))
               {
                   splitContainer1.Panel2.Controls.Add(previewer);
                   previewer.SetImage(x);
               }
               if (new string[] { ".gif" }.Contains(x.Extension.ToLower()))
               {
                   splitContainer1.Panel2.Controls.Add(gpreviewer);
                   gpreviewer.SetImage(x.FullName);
               }
               if (new string[] { ".mpg", ".flv", ".wmv", ".mp4", ".avi", ".mkv" }.Contains(x.Extension.ToLower()))
               {
                   splitContainer1.Panel2.Controls.Add(vpreviewer);
                   vpreviewer.RunVideo(x.FullName);
               }
               else
               {
                   vpreviewer.StopVideo();
               }

           });
        }

        private void Explorer_Shown(object sender, EventArgs e)
        {
            fileListControl1.Init();
            fileListControl2.Init();
        }

        private void FileListControl1_DeleteFileAction(FileListControl arg1, IFileInfo arg2)
        {
            if (previewer.CurrentFile == null) return;
            if (previewer.CurrentFile.FullName == arg2.FullName)
            {
                previewer.ResetImage();

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

        private void mouseIsoAction(FileListControl sender, IFileInfo obj)
        {
            var t = fileListControl1;
            if (t == sender)
            {
                t = fileListControl2;
            }
            Stuff.MountInfos.Add(new MountInfo() { IsoPath = obj, Path = t.CurrentDirectory.FullName });
            t.UpdateList(t.CurrentDirectory.FullName);
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
                        var fn = fileListControl1.SelectedFile.FullName;
                        if (!fileListControl2.CurrentTag.Files.Contains(fn))
                        {
                            fileListControl2.CurrentTag.AddFile(fn);
                            MessageBox.Show(Path.GetFileName(fn) + " tagged as " + fileListControl2.CurrentTag.Name);
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

        ImgViewerPanel previewer;
        GifViewerPanel gpreviewer;
        VideoPlayer vpreviewer;
        DjvuReader dpreviewer;
        TextPreviewer textPreviewer = new TextPreviewer() { Dock = DockStyle.Fill };
        bool IsPreviewMode = false;
        private void TablePreviewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsPreviewMode = true;
            RemoveSecondTab();
            splitContainer1.Panel2.Controls.Add(previewer);
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

        private void TableTextEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {

            RemoveSecondTab();
            splitContainer1.Panel2.Controls.Add(textPreviewer);
        }

        private void Explorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileListControl1.ParentClosing();
            fileListControl2.ParentClosing();
        }

        private void HideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stuff.ShowHidden = false;
            fileListControl1.UpdateAllLists();
            fileListControl2.UpdateAllLists();
        }

        private void UnhideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasswordDialog pdlg = new PasswordDialog();
            if (pdlg.ShowDialog() == DialogResult.OK)
            {
                Stuff.ShowHidden = true;
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
            fileListControl1.SetPath(fileListControl2.CurrentDirectory.FullName);
            fileListControl1.UpdateList();
            fileListControl2.SetPath(temp);
            fileListControl2.UpdateList();
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
    }
}


