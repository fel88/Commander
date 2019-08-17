using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using commander.Properties;
using isoViewer;
using System.Xml.Linq;
using DjvuNet;

namespace commander
{
    public partial class FileListControl : UserControl, IFileListControl
    {
        public FileListControl()
        {
            InitializeComponent();
            listView1.MouseLeave += ListView1_MouseLeave;
            listView1.ColumnClick += ListView1_ColumnClick;
            listView1.HideSelection = false;
            watcher.Changed += Watcher_Changed;
            Stuff.SetDoubleBuffered(listView1);
            tagControl.Init(this);
            listView1.MouseMove += ListView1_MouseMove1;

            tagControl.FollowAction = (x) =>
            {
                if (FollowAction != null)
                {
                    FollowAction(this, x);
                }
            };

            listView1.MouseUp += ListView1_MouseUp;

            listView1.MouseMove += ListView1_MouseMove;
            watcher.Created += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.Changed += Watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.CreationTime |
                NotifyFilters.FileName | NotifyFilters.LastWrite |
                NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.Size;
            filesControl = listView1;
            setTagsToolStripMenuItem.DropDown.Closing += DropDown_Closing;
            if (Stuff.list == null)
            {
                Stuff.list = new ImageList();
                var bmp = DefaultIcons.FolderLarge;

                Stuff.list.TransparentColor = Color.Black;
                Stuff.list.Images.Add(bmp.ToBitmap());
                Stuff.list.Images.Add(Resources._4dS9v);
            }


            foreach (var item in Stuff.FileContextMenuItems)
            {
                var v = new ToolStripMenuItem() { Text = item.Title, Tag = item };
                v.Click += V_Click;
                contextMenuStrip2.Items.Add(v);
            }
            phelper.Append(this, listView1);

        }

        public void Init()
        {
            watermark1.Init();
        }
        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            pressed = false;
        }

        PreviewHelper phelper = new PreviewHelper();

        private void ListView1_MouseMove1(object sender, MouseEventArgs e)
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
        internal void AddSelectedFileChangedAction(Action<IFileInfo> p)
        {
            SelectedFileChangedDelegates.Add(p);
        }

        public event Action<IFileInfo> SelectedFileChanged;
        public event Action<FileListControl, IFileInfo> FollowAction;
        private void ListView1_MouseUp(object sender, MouseEventArgs e)
        {
            pressed = false;
        }

        bool pressed = false;
        Point lastDragPos = new Point(0, 0);
        private void ListView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!pressed) return;
            if (lastDragPos.X != e.Location.X || lastDragPos.Y != e.Location.Y)
            {
                if (fileDrag != null)
                {
                    listView1.DoDragDrop(fileDrag.FullName, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

        private void V_Click(object sender, EventArgs e)
        {
            try
            {
                var t = (sender as ToolStripMenuItem);
                var m = (t.Tag as FileContextMenuItem);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = m.AppName;
                if (m.Arguments != null)
                {
                    psi.Arguments = m.Arguments;
                }
                psi.WorkingDirectory = CurrentDirectory.FullName;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Stuff.Error("Error: " + ex.Message);
            }

        }

        public void FollowToFile(IFileInfo f)
        {
            SetPath(f.DirectoryName);
            SetFilter(f.Name, true);
            UpdateList(f.DirectoryName);
        }



        private void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (!forceCloseMenu)
            {
                foreach (var item in setTagsToolStripMenuItem.DropDownItems)
                {
                    if (!(item is ToolStripMenuItem)) continue;
                    var mi = (item as ToolStripMenuItem);
                    if (mi != sender && !mi.Pressed)
                    {
                        return;
                    }
                }
                if (setTagsToolStripMenuItem.Pressed) e.Cancel = true;
            }
        }

        public void Rename()
        {

            if (Mode == ViewModeEnum.Tags)
            {
                if (!tagControl.ContainsFocus) return;
                if (tagControl.SelectedTag != null)
                {
                    RenameDialog rd = new RenameDialog();
                    rd.Value = tagControl.SelectedTag.Name;
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        tagControl.SelectedTag.Name = rd.Value;
                    }
                    tagControl.UpdateList(null);
                }
            }
            else
            {
                if (!ListView.Focused) return;
                if (SelectedFile != null)
                {
                    RenameDialog rd = new RenameDialog();
                    rd.Value = SelectedFile.Name;
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        File.Move(SelectedFile.FullName, Path.Combine(SelectedFile.Directory.FullName, rd.Value));
                    }
                    UpdateList(CurrentDirectory.FullName);
                }
                else if (SelectedDirectory != null)
                {
                    RenameDialog rd = new RenameDialog();
                    rd.Value = SelectedDirectory.Name;
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        Directory.Move(SelectedDirectory.FullName, Path.Combine(SelectedDirectory.Parent.FullName, rd.Value));
                    }
                    UpdateList(CurrentDirectory.FullName);
                }
            }
        }

        ListView filesControl;


        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            listView1.Invoke((Action)(() =>
            {
                UpdateList(new DirectoryInfoWrapper(CurrentDirectory.FullName), watermark1.Text);
            }));

        }

        public void SetPath(string path)
        {
            textBox1.Text = path;

        }

        
        public void NavigateTo(string path)
        {
            SetPath(path);
            UpdateList(new DirectoryInfoWrapper(textBox1.Text), Filter);
        }

        public bool DirFilterEnable
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                if (listView1.Sorting == SortOrder.Ascending)
                {
                    listView1.Sorting = SortOrder.Descending;
                }
                else
                {
                    listView1.Sorting = SortOrder.Ascending;
                }
                listView1.ListViewItemSorter = new Sorter2(listView1.Sorting);
                listView1.Sort();
            }
            if (e.Column == 1)
            {

                if (listView1.Sorting == SortOrder.Ascending)
                {
                    listView1.Sorting = SortOrder.Descending;
                }
                else
                {
                    listView1.Sorting = SortOrder.Ascending;
                }
                listView1.ListViewItemSorter = new TypeSorter(listView1.Sorting);
                listView1.Sort();
            }
            if (e.Column == 2)
            {

                if (listView1.Sorting == SortOrder.Ascending)
                {
                    listView1.Sorting = SortOrder.Descending;
                }
                else
                {
                    listView1.Sorting = SortOrder.Ascending;
                }
                listView1.ListViewItemSorter = new Sorter3(listView1.Sorting);
                listView1.Sort();
            }
            if (e.Column == 3)
            {

                if (listView1.Sorting == SortOrder.Ascending)
                {
                    listView1.Sorting = SortOrder.Descending;
                }
                else
                {
                    listView1.Sorting = SortOrder.Ascending;
                }
                listView1.ListViewItemSorter = new Sorter1(listView1.Sorting);
                listView1.Sort();
            }
        }

        public IDirectoryInfo CurrentDirectory;
        public TagInfo CurrentTag;
        public bool IsFilterPass(string str, string[] filters)
        {
            str = str.ToLower();
            if (filters.Length == 0) return true;
            return filters.Any(z => str.Contains(z));
        }

        public void UpdateList(string path)
        {
            try
            {
                watcher.Path = path;
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {

            }
            UpdateList(new DirectoryInfoWrapper(path), Filter);
        }

        public bool SaveSorting = false;
        public void UpdateList(string path, string filter)
        {
            if (Stuff.MountInfos.Any(z => path.StartsWith(z.FullPath)))
            {
                var fr = Stuff.MountInfos.First(z => path.StartsWith(z.Path));
                UpdateList(new IsoDirectoryInfoWrapper(fr, fr.Reader.Pvds.Last().RootDir), filter);
            }
            else
            {
                UpdateList(new DirectoryInfoWrapper(path), filter);
            }
            UpdateStatus();
        }
        public void UpdateList(IDirectoryInfo path, string filter)
        {
            if (!path.Exists)
            {
                MessageBox.Show("Directory is not exist. ", "Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                if (path is DirectoryInfoWrapper)
                {
                    watcher.Path = path.FullName;
                    watcher.EnableRaisingEvents = true;
                }
                else
                {
                    watcher.EnableRaisingEvents = false;
                }


                listView1.BeginUpdate();
                listView1.Items.Clear();
                if (!SaveSorting)
                {
                    listView1.ListViewItemSorter = null;
                    listView1.Sorting = SortOrder.None;
                }
                textBox1.Text = path.FullName;
                itemsCount = 0;

                CurrentDirectory = path;


                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = Stuff.list;
                listView1.LargeImageList = Stuff.list;


                if (path.FullName != path.Root.FullName)
                {

                    listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = path.Parent });
                }

                var dd = path.GetDirectories().ToList();

                var fr = Stuff.MountInfos.Where(z => Path.Equals(z.Path.Trim(new char[] { '\\' }), path.FullName.Trim(new char[] { '\\' })));
                if (fr.Any())
                {
                    foreach (var f in fr)
                    {
                        if (f.Reader == null)
                        {
                            isoViewer.IsoReader reader = new isoViewer.IsoReader();
                            reader.Parse(f.IsoPath.FullName);
                            f.Reader = reader;
                        }
                        var r = new IsoDirectoryInfoWrapper(f, f.Reader.WorkPvd.RootDir);
                        dd.Add(r);
                    }
                }

                AppendDirsToList(dd.ToArray(), fltrs);
                AppendFilesToList(path.GetFiles().ToArray(), fltrs);
                UpdateIcons();
                listView1.EndUpdate();
                UpdateStatus();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                listView1.EndUpdate();
            }
        }


        public void AppendDirsToList(IDirectoryInfo[] dirs, string[] fltrs)
        {
            foreach (var directoryInfo in dirs)
            {
                int imgind = 0;
                if (directoryInfo is IsoDirectoryInfoWrapper)
                {
                    if ((directoryInfo as IsoDirectoryInfoWrapper).Parent is DirectoryInfoWrapper)
                    {
                        imgind = 1;
                    }
                }

                if (checkBox1.Checked && !IsFilterPass(directoryInfo.Name, fltrs)) continue;
                listView1.Items.Add(new ListViewItem(new string[] { directoryInfo.Name, "", "", directoryInfo.LastWriteTime.ToString() })
                {
                    Tag = directoryInfo,
                    ImageIndex = imgind
                });
            }
        }

        public void AppendFilesToList(IFileInfo[] files, string[] fltrs)
        {
            foreach (var fileInfo in files)
            {
                try
                {
                    Tuple<Bitmap, int> tp = null;
                    if (File.Exists(fileInfo.FullName))
                    {
                        //tp = GetBitmapOfFile(fileInfo.FullName);                        
                    }

                    var ext = Path.GetExtension(fileInfo.FullName);

                    if (!IsFilterPass(fileInfo.Name, fltrs)) continue;
                    var astr = (fileInfo.Attributes).ToString();




                    int iindex = -1;
                    if (tp != null)
                    {
                        iindex = tp.Item2;
                    }
                    var attrs = astr.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate("", (x, y) => x + y[0]);
                    itemsCount++;
                    listView1.Items.Add(
                        new ListViewItem(new string[]
                        {
                        fileInfo.Name,
                        fileInfo.Extension,
                            Stuff.GetUserFriendlyFileSize(fileInfo.Length) ,
                            fileInfo.LastWriteTime.ToString(),"",
                        astr
                        })
                        {
                            Tag = fileInfo,
                            ImageIndex = iindex

                        });
                }
                catch (Exception ex)
                {

                }
            }
        }



        public void UpdateTagsList()
        {
            tagControl.UpdateList(CurrentTag);
        }

        public void UpdateLibrariesList(IDirectoryInfo path, string filter = "")
        {

            if (path == null)
            {
                listView1.Items.Clear();

                foreach (var item in Stuff.Libraries)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                }
            }
            else
            {
                watcher.Path = path.FullName;
                watcher.EnableRaisingEvents = true;
                listView1.Items.Clear();
                textBox1.Text = path.FullName;


                CurrentDirectory = path;


                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = Stuff.list;
                listView1.LargeImageList = Stuff.list;
                if (Stuff.Libraries.OfType<FilesystemLibrary>().Any(z => z.BaseDirectory == path.FullName))
                {


                    listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = libraryRootObject });
                }



                AppendDirsToList(path.GetDirectories().ToArray(), fltrs);
                AppendFilesToList(path.GetFiles().ToArray(), fltrs);
            }
        }

        internal void UpdateAllLists()
        {
            tagControl.UpdateList(CurrentTag);
        }

        public IDirectoryInfo SelectedDirectory
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is IDirectoryInfo)
                    {
                        return si as IDirectoryInfo;
                    }
                }

                return null;
            }
        }
        public IDirectoryInfo[] SelectedDirectories
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    List<IDirectoryInfo> dirs = new List<IDirectoryInfo>();
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        var si = listView1.SelectedItems[i].Tag;
                        if (si is IDirectoryInfo)
                        {
                            dirs.Add(si as IDirectoryInfo);
                        }
                    }
                    return dirs.ToArray();
                }

                return null;
            }
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
        public IFileInfo[] SelectedFiles
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    List<IFileInfo> files = new List<IFileInfo>();
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        var si = listView1.SelectedItems[i].Tag;
                        if (si is IFileInfo)
                        {
                            files.Add(si as IFileInfo);
                        }
                    }
                    return files.ToArray();
                }

                return null;
            }
        }
        public ListViewItem[] SelectedLvis
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    List<ListViewItem> lvis = new List<ListViewItem>();
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        var si = listView1.SelectedItems[i];
                        lvis.Add(si);
                    }
                    return lvis.ToArray();
                }
                return null;
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ExecuteSelected();
            }
        }

        public void ParentClosing()
        {
            phelper.Stop();
            tagControl.ParentClosing();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateList();
            }
        }

        public void UpdateList()
        {
            UpdateList(textBox1.Text, Filter);

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (Mode == ViewModeEnum.Tags)
            {
                tagControl.UpdateList(CurrentTag);
            }
            else
            {
                UpdateList(CurrentDirectory.FullName, Filter);
            }
        }

        public void SetFilter(string mask, bool dirFilter = false)
        {
            watermark1.Text = mask;
            checkBox1.Checked = dirFilter;
        }

        public void UpdateDrivesList()
        {
            var drivs = System.IO.DriveInfo.GetDrives();
            comboBox1.Items.Clear();
            foreach (var item in drivs)
            {

                comboBox1.Items.Add(new ComboBoxItem()
                {
                    Tag = item,
                    Name = item.Name +
                    (string.IsNullOrEmpty(item.VolumeLabel) ? "" : ("(" + item.VolumeLabel + ")")) + " " + Stuff.GetUserFriendlyFileSize(item.AvailableFreeSpace) + " / " + Stuff.GetUserFriendlyFileSize(item.TotalSize)
                });
            }
        }

        public ListView ListView
        {
            get { return listView1; }
        }

        public string Filter
        {
            get
            {
                return watermark1.Text;
            }
        }

        public List<TabInfo> Tabs = new List<TabInfo>();

        public void AddTab(TabInfo tinf)
        {

            Tabs.Add(tinf);
            var b = new MyToolStripButton() { Text = tinf.Hint, DisplayStyle = ToolStripItemDisplayStyle.Text };

            b.ContextMenuStrip = contextMenuStrip1;
            b.Click += (x, z) =>
            {
                var bb = x as ToolStripButton;
                var tabinf = bb.Tag as TabInfo;
                watermark1.Text = tabinf.Filter;
                UpdateList(tabinf.Path, tabinf.Filter);

            };
            b.Tag = tinf;
            b.ToolTipText = tinf.Path;
            using (var gr = CreateGraphics())
            {
                var msr = gr.MeasureString(b.Text, b.Font, new SizeF(500, 40));
                b.Width = (int)msr.Width + 20;
            }


            toolStrip1.Items.Add(b);
        }


        public string TabOwnerString;
        private Control todel;
        private void removeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var p = contextMenuStrip1.Tag as MyToolStripButton;
            Stuff.Tabs.Remove(p.Tag as TabInfo);
            Tabs.Remove(p.Tag as TabInfo);
            Stuff.IsDirty = true;
            toolStrip1.Items.Remove(p);
        }

        private void openExplorerHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                    Process.Start(d.FullName);
                }
                else
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IFileInfo;
                    Process.Start(d.DirectoryName);
                }

            }

            else if (CurrentDirectory != null && CurrentDirectory.Exists)
            {
                Process.Start(CurrentDirectory.FullName);
            }
        }



        void RunCmd(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = path,
                FileName = "cmd.exe",

            };
            Process.Start(startInfo);
        }

        private void openCmdHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                    RunCmd(d.FullName);
                }
                else
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IFileInfo;
                    RunCmd(d.DirectoryName);
                }
            }
            else if (CurrentDirectory != null && CurrentDirectory.Exists)
            {
                RunCmd(CurrentDirectory.FullName);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateList(CurrentDirectory.FullName, Filter);
        }


        void ExecuteSelected()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as IFileInfo;
                    if (f.Extension.Contains("bat") && MessageBox.Show("show internal console?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        ConsoleOutputWindow cow = new ConsoleOutputWindow();
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = f.DirectoryName;
                        psi.FileName = "cmd";
                        psi.Arguments = "/c " + f.FullName;
                        psi.WorkingDirectory = f.DirectoryName;
                        //Process.Start(f.FullName);
                        psi.RedirectStandardOutput = true;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        Process proc = new Process();
                        proc.StartInfo = psi;

                        proc.Start();

                        StringBuilder sb = new StringBuilder();
                        var output = proc.StandardOutput.ReadToEnd();
                        /*
                        while (!proc.StandardOutput.EndOfStream)
                        {
                            string line = proc.StandardOutput.ReadLine();
                            sb.AppendLine(line);

                        }
                        */
                        //cow.SetText("> Start\nProcess complete.. 100%\n > Exit");
                        cow.SetText(sb.ToString());
                        cow.SetText(output);

                        mdi.MainForm.OpenWindow(cow);
                    }
                    else if (f.Extension.ToLower().EndsWith(".iso") && MessageBox.Show("use internal viewer?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        IsoLibViewer frm = new IsoLibViewer() { Dock = DockStyle.Fill };

                        frm.LoadIso(f.FullName);
                        frm.MdiParent = mdi.MainForm;

                        frm.Show();
                    }
                    /*else if (f.Extension.Contains("mp4") && MessageBox.Show("use internal player?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        VideoPlayer vp = new VideoPlayer() { Dock=DockStyle.Fill};
                        var frm = new Form();
                        frm.Width = 600;
                        frm.Height = 600;
                        vp.RunVideo(f.FullName);
                        frm.MdiParent = mdi.MainForm;
                        frm.Controls.Add(vp);
                        frm.Show();
                    }*/
                    else
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = f.DirectoryName;
                        psi.FileName = f.FullName;
                        //Process.Start(f.FullName);
                        Process.Start(psi);
                    }
                }
                else
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    try
                    {
                        var f = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                        UpdateList(f, Filter);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
                else
                if (listView1.SelectedItems[0].Tag is FilesystemLibrary || listView1.SelectedItems[0].Tag == libraryRootObject)
                {
                    try
                    {

                        if (listView1.SelectedItems[0].Tag == libraryRootObject)
                        {
                            UpdateLibrariesList(null, Filter);
                        }
                        else
                        {
                            var f = listView1.SelectedItems[0].Tag as FilesystemLibrary;
                            UpdateLibrariesList(new DirectoryInfoWrapper(f.BaseDirectory), Filter);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(ex.Message, "Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
            }
        }

        private object libraryRootObject = new object();
        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelected();
            }
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
            }
        }

        int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0;
            int temp = 0;
            Label label1 = new Label();

            foreach (var obj in myCombo.Items)
            {
                label1.Text = obj.ToString();
                temp = label1.PreferredWidth;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            label1.Dispose();
            return maxWidth;
        }
        private void ComboBox1_DropDown(object sender, EventArgs e)
        {
            UpdateDrivesList();
            comboBox1.DropDownWidth = DropDownWidth(comboBox1);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var citem = comboBox1.SelectedItem as ComboBoxItem;
            var di = citem.Tag as DriveInfo;
            textBox1.Text = di.Name;
            UpdateList(textBox1.Text);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {

        }



        private void TxtFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = 0;
            while (true)
            {
                string name = "noname" + index + ".txt";
                string path = Path.Combine(CurrentDirectory.FullName, name);
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "");
                    break;
                }
                index++;
            }

            UpdateList(CurrentDirectory.FullName);

        }

        FileSystemWatcher watcher = new FileSystemWatcher();
        private void FolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = 0;
            while (true)
            {
                string name = "folder" + index;
                string path = Path.Combine(CurrentDirectory.FullName, name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    break;
                }
                index++;
            }

            UpdateList(CurrentDirectory.FullName);
        }

        void DeleteItemFromListView(ListViewItem lvi)
        {
            listView1.Items.Remove(lvi);
        }
        void DeleteItemsFromListView(ListViewItem[] lvis)
        {
            listView1.BeginUpdate();
            foreach (var lvi in lvis)
            {
                listView1.Items.Remove(lvi);
            }
            listView1.EndUpdate();
        }
        void DeleteSelected()
        {
            if (!ListView.Focused) return;


            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var lvi = listView1.SelectedItems[0];
                    var f = lvi.Tag as IFileInfo;

                    if (Stuff.Question("Delete file: " + SelectedFile.FullName + "?") == DialogResult.Yes)
                    {
                        DeleteFileAction(this, f);
                        var attr = File.GetAttributes(SelectedFile.FullName);
                        bool allow = true;
                        if (attr.HasFlag(FileAttributes.ReadOnly))
                        {
                            if (Stuff.Question("File is read-only, do you want to delete it anyway?") != DialogResult.Yes)
                            {
                                allow = false;
                            }
                            else
                            {
                                File.SetAttributes(SelectedFile.FullName, FileAttributes.Normal);
                            }
                        }
                        if (allow)
                        {
                            //remove tags
                            foreach (var item in Stuff.Tags)
                            {
                                if (item.ContainsFile(f.FullName))
                                {
                                    item.DeleteFile(f.FullName);
                                }
                            }

                            watcher.EnableRaisingEvents = false;
                            File.Delete(f.FullName);
                            watcher.EnableRaisingEvents = true;
                            DeleteItemFromListView(lvi);

                            //UpdateList(CurrentDirectory.FullName);
                        }
                    }

                }
                else
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                    if (Stuff.Question("Delete " + f.Name + " directory and all contents?") == DialogResult.Yes)
                    {
                        Directory.Delete(f.FullName, true);

                        if (Stuff.Question("Delete all tags if exist?") == DialogResult.Yes)
                        {
                            var fls2 = Stuff.GetAllFiles(f);
                            //remove tags
                            foreach (var zitem in fls2)
                            {
                                foreach (var titem in Stuff.Tags)
                                {
                                    if (titem.ContainsFile(zitem.FullName))
                                    {

                                        titem.DeleteFile(zitem.FullName);
                                    }
                                }
                            }
                        }

                        UpdateList(CurrentDirectory.FullName);
                    }
                }
                return;
            }
            ////////////
            var self = SelectedFiles;
            var lvis = SelectedLvis;
            var seld = SelectedDirectories;
            List<IFileInfo> fls = new List<IFileInfo>();
            List<IDirectoryInfo> drs = new List<IDirectoryInfo>();

            if (self != null)
            {
                fls.AddRange(self);
            }
            if (seld != null)
            {
                drs.AddRange(seld);
            }
            if (Stuff.Question("Are your sure to delete: " + drs.Count + " directories and " + fls.Count + " files?") == DialogResult.Yes)
            {
                var res = Stuff.Question("Delete all tags if exist?") == DialogResult.Yes;

                watcher.EnableRaisingEvents = false;
                foreach (var item in drs)
                {
                    var fls2 = Stuff.GetAllFiles(item);
                    if (res)
                    {
                        //remove tags
                        foreach (var zitem in fls2)
                        {
                            foreach (var titem in Stuff.Tags)
                            {
                                if (titem.ContainsFile(zitem.FullName))
                                {
                                    titem.DeleteFile(zitem.FullName);
                                }
                            }
                        }
                    }
                    Directory.Delete(item.FullName, true);
                }
                bool yesToAll = false;
                foreach (var fitem in fls)
                {
                    DeleteFileAction(this, fitem);
                    var attr = File.GetAttributes(fitem.FullName);
                    bool allow = true;
                    if (attr.HasFlag(FileAttributes.ReadOnly))
                    {
                        if (yesToAll || Stuff.Question("Some files are read-only, do you want to delete it anyway?") == DialogResult.Yes)
                        {
                            yesToAll = true;
                            File.SetAttributes(SelectedFile.FullName, FileAttributes.Normal);

                        }
                        else
                        {
                            allow = false;
                        }
                    }
                    if (allow)
                    {
                        if (res)
                        {
                            //remove tags
                            foreach (var item in Stuff.Tags)
                            {
                                if (item.ContainsFile(fitem.FullName))
                                {
                                    item.DeleteFile(fitem.FullName);
                                }
                            }
                        }

                        File.Delete(fitem.FullName);

                        //UpdateList(CurrentDirectory.FullName);
                    }
                }
                watcher.EnableRaisingEvents = true;
                DeleteItemsFromListView(lvis);                
            }

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        public List<Action<IFileInfo>> SelectedFileChangedDelegates = new List<Action<IFileInfo>>();
        int itemsCount;

        public void UpdateStatus()
        {
            toolStripStatusLabel1.Text = "Files: " + itemsCount;
            if (listView1.SelectedItems.Count > 1)
            {
                toolStripStatusLabel1.Text += " selected: " + listView1.SelectedItems.Count;
            }
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            UpdateIcons();
            UpdateStatus();
            if (SelectedFileChanged != null)
            {
                SelectedFileChanged(SelectedFile);
            }
            if (SelectedFileChangedDelegates.Any())
            {

                foreach (var item in SelectedFileChangedDelegates)
                {
                    item(SelectedFile);
                }

            }
        }

        TagListViewControl tagControl = new TagListViewControl() { Dock = DockStyle.Fill };
        void SetTagMode()
        {
            Mode = ViewModeEnum.Tags;
            panel3.Controls.Clear();
            panel3.Controls.Add(tagControl);
        }
        void SetFilesystemMode()
        {
            Mode = ViewModeEnum.Filesystem;
            panel3.Controls.Clear();
            panel3.Controls.Add(filesControl);
        }

        void SetLibrariesMode()
        {
            Mode = ViewModeEnum.Libraries;
            panel3.Controls.Clear();
            panel3.Controls.Add(filesControl);
        }
        public ViewModeEnum Mode = ViewModeEnum.Filesystem;
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)//filesystem
            {
                SetFilesystemMode();
                UpdateList(textBox1.Text);
            }
            if (comboBox2.SelectedIndex == 1)//libraries
            {
                SetLibrariesMode();
                UpdateLibrariesList(null);
            }
            if (comboBox2.SelectedIndex == 2)//tags
            {
                SetTagMode();
                tagControl.UpdateList(null);
            }
        }

        private void MakeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ExecuteSelected();
            }
        }

        private void SetTagsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            setTagsToolStripMenuItem.DropDownItems.Clear();
            forceCloseMenu = false;
            if (listView1.SelectedItems.Count == 0) return;



            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var item in listView1.SelectedItems)
            {
                var tag = (item as ListViewItem).Tag;
                if (!(tag is IFileInfo)) continue;
                files.Add(tag as IFileInfo);
            }

            if (!files.Any()) return;


            List<TagInfo> cands = new List<TagInfo>();
            foreach (var item in Stuff.Tags.OrderBy(z => z.Name))
            {
                if (item.IsHidden && !Stuff.ShowHidden) continue;
                cands.Add(item);
            }

            if (cands.Count > 20)
            {
                int grps = cands.Count / 20;
                int remain = cands.Count;
                int index = 0;
                List<ToolStripMenuItem> ii = new List<ToolStripMenuItem>();
                for (int i = 0; i <= grps; i++)
                {
                    ToolStripMenuItem grp1 = new ToolStripMenuItem("Group #" + i);
                    setTagsToolStripMenuItem.DropDownItems.Add(grp1);
                    grp1.DropDown.Closing += DropDown_Closing;
                    for (int j = 0; j < 20; j++)
                    {
                        if (remain == 0) break;
                        var item = cands[index++];
                        var ww = files.Count(z => item.ContainsFile(z.FullName));
                        var state = CheckState.Indeterminate;
                        if (ww == 0) { state = CheckState.Unchecked; }
                        if (ww == files.Count) { state = CheckState.Checked; }

                        var ss = new ToolStripMenuItem(item.Name)
                        {
                            CheckOnClick = true,
                            CheckState = state
                        };
                        ss.Tag = new Tuple<TagInfo, IFileInfo[]>(item, files.ToArray());
                        ss.CheckedChanged += Ss_CheckedChanged;
                        grp1.DropDownItems.Add(ss);
                        remain--;
                    }

                }



            }
            else
            {
                foreach (var item in cands)
                {
                    var ww = files.Count(z => item.ContainsFile(z.FullName));
                    var state = CheckState.Indeterminate;
                    if (ww == 0) { state = CheckState.Unchecked; }
                    if (ww == files.Count) { state = CheckState.Checked; }

                    var ss = new ToolStripMenuItem(item.Name)
                    {
                        CheckOnClick = true,
                        CheckState = state
                    };
                    ss.Tag = new Tuple<TagInfo, IFileInfo[]>(item, files.ToArray());
                    ss.CheckedChanged += Ss_CheckedChanged;
                    setTagsToolStripMenuItem.DropDownItems.Add(ss);
                }
            }


        }

        private void Ss_CheckedChanged(object sender, EventArgs e)
        {
            var f = (sender as ToolStripMenuItem).Tag as Tuple<TagInfo, IFileInfo[]>;
            foreach (var item in f.Item2)
            {
                if (f.Item1.ContainsFile(item.FullName))
                {
                    f.Item1.DeleteFile(item.FullName);
                }
                else
                {
                    f.Item1.AddFile(item.FullName);
                }
            }
        }

        bool forceCloseMenu = true;
        private void ContextMenuStrip2_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            /*if (setTagsToolStripMenuItem.Pressed) e.Cancel = true;
            if (!allowCloseMenu) e.Cancel = true;
            if (e.Cancel == false)
            {
                setTagsToolStripMenuItem.DropDown.Close();
            }*/
        }


        private void ListView1_MouseDown(object sender, MouseEventArgs e)
        {
            forceCloseMenu = true;
            contextMenuStrip2.Close();
            var item = listView1.GetItemAt(e.Location.X, e.Location.Y);

            if (item != null && e.Button == MouseButtons.Left)
            {
                var f = item.Tag as IFileInfo;
                if (f != null)
                {
                    pressed = true;
                    lastDragPos = e.Location;
                    fileDrag = f;
                }
            }
        }
        IFileInfo fileDrag;

        private void AddShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile != null)
            {
                Stuff.Shortcuts.Add(new AppShortcutInfo(SelectedFile.FullName, SelectedFile.Name));
                Stuff.IsDirty = true;
                mdi.MainForm.AppendShortCutPanelButton(Stuff.Shortcuts.Last());

            }
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            AddTabWindow atw = new AddTabWindow();
            atw.Path = textBox1.Text;
            var dir = new DirectoryInfo(textBox1.Text);
            atw.Hint = dir.Name;
            if (atw.ShowDialog() == DialogResult.OK)
            {
                var tab = new TabInfo()
                {
                    Hint = atw.Hint,
                    Path = atw.Path,
                    Filter = atw.Filter,
                    Owner = TabOwnerString
                };
                Stuff.AddTab(tab);
                AddTab(tab);
            }
        }

        private void PackToIsoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public Action<FileListControl, IFileInfo> MountIsoAction;
        public event Action<FileListControl, IFileInfo> DeleteFileAction;
        public Action<FileListControl, IFileInfo> IsoExtractAction;
        private void MountIsoToRightToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ContextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var l = contextMenuStrip2.Left;
            var t = contextMenuStrip2.Top;
            var p = listView1.PointToClient(new Point(l, t));
            p = listView1.PointToClient(Cursor.Position);
            var ch = listView1.GetItemAt(p.X, p.Y);
            bool en = true;
            if (ch == null) { en = false; }
            else
            {
                var ff = (ch.Tag as IFileInfo);
                if (ff == null) { en = false; }
                else
                {
                    if (!ff.Extension.EndsWith("iso")) { en = false; }
                }
            }
            mountToolStripMenuItem.Enabled = en;
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            SaveSorting = checkBox2.Checked;
        }

        private void TextBox1_ImeModeChanged(object sender, EventArgs e)
        {

        }

        private void PackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedDirectory == null) return;
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //todo: pack SelectedDirectory to iso
            }
        }

        private void MountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (!SelectedFile.Extension.Contains("iso")) return;
            if (MountIsoAction != null)
            {
                MountIsoAction(this, SelectedFile);
            }
        }

        private void ExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (!SelectedFile.Extension.Contains("iso")) return;
            if (IsoExtractAction != null)
            {
                IsoExtractAction(this, SelectedFile);
            }


        }

        private void UnmountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedDirectory == null) return;
            var fr = Stuff.MountInfos.FirstOrDefault(z => z.FullPath == SelectedDirectory.FullName);
            if (fr != null)
            {
                Stuff.MountInfos.Remove(fr);
                UpdateList(CurrentDirectory.FullName);
            }
        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null && SelectedDirectory == null) { return; }
            if (SelectedFile != null)
            {
                Stuff.ShowFileProperties(SelectedFile.FullName);
            }
            else
            {
                Stuff.ShowFileProperties(SelectedDirectory.FullName);
            }
        }

        private void addSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (Stuff.OfflineSites.Any(z => z.Path == SelectedFile.FullName))
            {
                Stuff.Warning("Already exist"); ;
            }
            else
            {
                Stuff.OfflineSites.Add(new commander.OfflineSiteInfo() { Path = SelectedFile.FullName });
                Stuff.IsDirty = true;
            }
        }

        private void autotegToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("feature is not implemented yet");
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null && SelectedDirectory == null) { return; }
            if (SelectedFile != null)
            {
                Clipboard.SetText(SelectedFile.FullName);
            }
            else
            {
                Clipboard.SetText(SelectedDirectory.FullName);
            }
        }

        private void calcMemToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (listView1.SelectedItems.Count == 1 && (tag is DirectoryInfoWrapper || tag is FilesystemLibrary))
                {
                    IDirectoryInfo d = null;
                    if (tag is FilesystemLibrary)
                    {
                        var l = tag as FilesystemLibrary;
                        d = new DirectoryInfoWrapper(l.BaseDirectory);
                    }
                    else
                    {
                        d = (listView1.SelectedItems[0].Tag as DirectoryInfoWrapper);
                    }
                    List<IFileInfo> files = new List<IFileInfo>();
                    Stuff.GetAllFiles(d, files);
                    var total = files.Sum(z => z.Length);
                    if (Stuff.Question("Total size: " + Stuff.GetUserFriendlyFileSize(total) + ", show report?") == DialogResult.Yes)
                    {
                        MemInfoReport rep = new MemInfoReport();
                        rep.MdiParent = mdi.MainForm;
                        rep.Init(d);
                        rep.Show();
                    }
                }
                else
                {
                    //bool allFiles = true;
                    List<IFileInfo> files = new List<IFileInfo>();
                    long total = 0;
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        var tag1 = listView1.SelectedItems[i].Tag;
                        if (tag1 is IDirectoryInfo)
                        {
                            var list = Stuff.GetAllFiles(tag1 as IDirectoryInfo);
                            total += list.Sum(z => z.Length);
                        }
                        if (tag1 is IFileInfo)
                        {
                            files.Add(tag1 as IFileInfo);
                            // allFiles = false;
                            //break;
                        }

                    }

                    total += files.Sum(z => z.Length);
                    //if (allFiles)
                    {
                        Stuff.Info("Total size: " + Stuff.GetUserFriendlyFileSize(total));
                    }
                }
            }
            else if (CurrentDirectory != null && CurrentDirectory is DirectoryInfoWrapper)
            {
                var d = CurrentDirectory;




                List<IFileInfo> files = new List<IFileInfo>();
                Stuff.GetAllFiles(d, files);
                var total = files.Sum(z => z.Length);
                if (Stuff.Question("Total size: " + Stuff.GetUserFriendlyFileSize(total) + ", show report?") == DialogResult.Yes)
                {
                    MemInfoReport rep = new MemInfoReport();
                    rep.MdiParent = mdi.MainForm;
                    rep.Init(d);
                    rep.Show();
                }
            }
        }

        private void deduplicationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is IDirectoryInfo || tag is FilesystemLibrary)
                {

                    IDirectoryInfo d = null;
                    if (tag is FilesystemLibrary)
                    {
                        var l = tag as FilesystemLibrary;
                        d = new DirectoryInfoWrapper(l.BaseDirectory);
                    }
                    else
                    {
                        d = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                    }
                    DedupContext ctx = new DedupContext(new[] { d }, new IFileInfo[] { });
                    var groups = RepeatsWindow.FindRepeats(ctx);
                    if (groups.Count() == 0)
                    {
                        Stuff.Info("No duplicates found.");
                    }
                    else
                    {
                        RepeatsWindow rp = new RepeatsWindow();
                        rp.MdiParent = mdi.MainForm;
                        rp.SetRepeats(ctx, groups.ToArray());
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
                    var groups = RepeatsWindow.FindRepeats(ctx);
                    if (groups.Count() == 0)
                    {
                        Stuff.Info("No duplicates found.");
                    }
                    else
                    {
                        RepeatsWindow rp = new RepeatsWindow();
                        rp.MdiParent = mdi.MainForm;
                        rp.SetRepeats(ctx, groups.ToArray());
                        rp.Show();
                    }
                }
            }
        }

        private void calcMd5ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as IFileInfo;
                    var md5 = Stuff.CalcMD5(f.FullName);
                    Clipboard.SetText(md5);
                    MessageBox.Show("MD5: " + md5, "Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IDirectoryInfo;
                    TextSearchForm tsf = new TextSearchForm();
                    tsf.SetPath(d.FullName);
                    tsf.Show();

                }
                else
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as IFileInfo;
                    TextSearchForm tsf = new TextSearchForm();
                    tsf.SetPath(d.DirectoryName);
                    tsf.Show();
                }
            }
        }

        private void opeInHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as FileInfo;
                    HexEditor hex = new HexEditor();
                    hex.OpenFile(d.FullName);
                    hex.Show();

                }

            }
        }

        private void openInImageViewerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as FileInfo;
                    ImageViewer tsf = new ImageViewer();
                    tsf.SetBitmap(Bitmap.FromFile(d.FullName) as Bitmap);
                    tsf.Show();
                }
            }
        }

        private void MakeLibraryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    if (MessageBox.Show("Make " + f.Name + " library?", "Commander", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Stuff.Libraries.Add(new FilesystemLibrary() { BaseDirectory = f.FullName, Name = f.Name });
                        Stuff.IsDirty = true;
                    }
                }
            }
        }

        private void IsoMergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (!SelectedFile.Extension.EndsWith("iso")) { Stuff.Warning("Choose iso image library file."); return; }
            //merge tags, files exract (or not extract use as mount drive)
            //read .indx dir and get meta.xml file. get tags and files. then extract all files some where,or not extract.

            isoViewer.IsoReader reader = new IsoReader();
            reader.Parse(SelectedFile.FullName);
            var recs = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
            var meta = recs.FirstOrDefault(z => z.IsFile && z.Name == "meta.xml");
            //extract first iso totaly, then merge tags info
            using (FileStream fs = new FileStream(SelectedFile.FullName, FileMode.Open, FileAccess.Read))
            {
                var data = meta.GetFileData(fs, reader.WorkPvd);
                var xml = Encoding.UTF8.GetString(data);
                var doc = XDocument.Parse(xml);
                foreach (var item in doc.Descendants("tag"))
                {
                    var n = item.Attribute("name").Value;
                    var tag = Stuff.AddTag(new TagInfo() { Name = n });
                    foreach (var fitem in item.Elements("file"))
                    {
                        var path = fitem.Value;
                        if (!tag.ContainsFile(path))
                        {
                            tag.AddFile(path);
                        }
                    }
                }
            }
        }


        private void PackAsLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedDirectory == null) return;


            //pack with .indx directory (+tags,+meta infos,etc.)
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "iso";
            sfd.Filter = "iso images|*.iso";
            //save all as one big file info.          

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Stuff.PackToIso(SelectedDirectory, sfd.FileName);
            }
        }

        private void TaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            QuickTagsWindow q = new QuickTagsWindow();

            q.Init(this, SelectedFile);
            q.MdiParent = mdi.MainForm;
            q.TopLevel = false;
            q.Show();
        }

        private void WindowsMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void WindowsMenuToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            if (SelectedFile != null)
            {
                ShellContextMenu ctxMnu = new ShellContextMenu();
                FileInfo[] arrFI = new FileInfo[1];
                arrFI[0] = new FileInfo(SelectedFile.FullName);
                var ee = Cursor.Position;
                //ee = listView1.SelectedItems[0].Position;
                ctxMnu.ShowContextMenu(arrFI, (new Point(ee.X, ee.Y)));
            }
            else
            if (SelectedDirectory != null)
            {
                ShellContextMenu ctxMnu = new ShellContextMenu();
                DirectoryInfo[] arrFI = new DirectoryInfo[1];
                arrFI[0] = new DirectoryInfo(SelectedDirectory.FullName);
                var ee = Cursor.Position;
                //ee = listView1.SelectedItems[0].Position;

                ctxMnu.ShowContextMenu(arrFI, new Point(ee.X, ee.Y));
            }
            else if (CurrentDirectory != null && CurrentDirectory.Parent != null)
            {
                if (CurrentDirectory.Parent is DirectoryInfoWrapper)
                {
                    if ((CurrentDirectory.Parent as DirectoryInfoWrapper).DirectoryInfo == null)
                    {
                        return;
                    }
                }
                ShellContextMenu ctxMnu = new ShellContextMenu();
                DirectoryInfo[] arrFI = new DirectoryInfo[1];
                arrFI[0] = new DirectoryInfo(CurrentDirectory.FullName);
                var ee = Cursor.Position;

                ctxMnu.ShowContextMenu(arrFI, new Point(ee.X, ee.Y));
            }
        }

        private void OpenInTextEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void IndexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //get all info
            if (SelectedFile == null) return;
            Stuff.IndexFile(SelectedFile);

        }

        private void Watermark1_TextChanged(object sender, EventArgs e)
        {
            if (Mode == ViewModeEnum.Tags)
            {
                tagControl.UpdateList(CurrentTag);
            }
            else
            {
                UpdateList(CurrentDirectory.FullName, watermark1.Text);
            }
        }
    }
    public enum ViewModeEnum
    {
        Filesystem, Libraries, Tags
    }

    public class FileContextMenuItem
    {
        public string Title;
        public string AppName;
        public string Arguments;
    }
}

