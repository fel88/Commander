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

namespace commander
{
    public partial class FileListControl : UserControl
    {
        public FileListControl()
        {
            InitializeComponent();
            listView1.ColumnClick += ListView1_ColumnClick;
            watcher.Changed += Watcher_Changed;
            Stuff.SetDoubleBuffered(listView1);
            tagControl.Init(this);
            watcher.Created += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            filesControl = listView1;
            setTagsToolStripMenuItem.DropDown.Closing += DropDown_Closing;
            if (list == null)
            {
                list = new ImageList();
                var bmp = DefaultIcons.FolderLarge;
                list.TransparentColor = Color.Black;
                list.Images.Add(bmp.ToBitmap());
            }

        }



        private void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (!forceCloseMenu)
            {
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
            UpdateList(textBox1.Text, textBox2.Text);
        }

        public void SetPath(string path)
        {
            textBox1.Text = path;

        }

        public void NavigateTo(string path)
        {
            SetPath(path);
            UpdateList(textBox1.Text, textBox2.Text);
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
                listView1.ListViewItemSorter = new Sorter3(listView1.Sorting);
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
                listView1.ListViewItemSorter = new Sorter1(listView1.Sorting);
                listView1.Sort();
            }
        }

        public DirectoryInfo CurrentDirectory;
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
            UpdateList(path, textBox2.Text);
        }

        public static Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();

        public static Bitmap GetBitmapOfFile(string fn)
        {
            var d = Path.GetExtension(fn);
            if (!Icons.ContainsKey(d))
            {
                var bb = Bitmap.FromHicon(Icon.ExtractAssociatedIcon(fn).Handle);
                bb.MakeTransparent();
                list.Images.Add(bb);
                IconDictionary.Add(d, list.Images.Count - 1);
                Icons.Add(d, bb);
            }

            return Icons[d];
        }

        public static Dictionary<string, int> IconDictionary = new Dictionary<string, int>();
        public void UpdateList(string path, string filter)
        {
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Directory is not exist. ", "Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                watcher.Path = path;
                watcher.EnableRaisingEvents = true;

                listView1.BeginUpdate();
                listView1.Items.Clear();
                textBox1.Text = path;

                var p = new DirectoryInfo(path);
                CurrentDirectory = p;


                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = list;
                listView1.LargeImageList = list;
                listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = p.Parent });



                AppendDirsToList(p.GetDirectories(), fltrs);
                AppendFilesToList(p.GetFiles(), fltrs);
                listView1.EndUpdate();
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

        public void AppendDirsToList(DirectoryInfo[] dirs, string[] fltrs)
        {
            foreach (var directoryInfo in dirs)
            {
                if (checkBox1.Checked && !IsFilterPass(directoryInfo.Name, fltrs)) continue;
                listView1.Items.Add(new ListViewItem(new string[] { directoryInfo.Name, "", directoryInfo.LastWriteTime.ToString() })
                {
                    Tag = directoryInfo,
                    ImageIndex = 0
                });
            }
        }

        public void AppendFilesToList(FileInfo[] files, string[] fltrs)
        {
            foreach (var directoryInfo in files)
            {
                try
                {
                    var bmp = GetBitmapOfFile(directoryInfo.FullName);
                    var ext = Path.GetExtension(directoryInfo.FullName);

                    if (!IsFilterPass(directoryInfo.Name, fltrs)) continue;
                    var astr = File.GetAttributes(directoryInfo.FullName).ToString();
                    if (astr.ToLower().Contains("spar"))
                    {

                    }
                    var attrs = astr.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate("", (x, y) => x + y[0]);
                    listView1.Items.Add(
                        new ListViewItem(new string[]
                        {
                        directoryInfo.Name, Stuff.GetUserFriendlyFileSize(directoryInfo.Length) , directoryInfo.LastWriteTime.ToString(),"",
                        astr
                        })
                        {
                            Tag = directoryInfo,
                            ImageIndex = IconDictionary[ext]

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
        public static ImageList list = null;
        public void UpdateLibrariesList(string path, string filter = "")
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
                watcher.Path = path;
                watcher.EnableRaisingEvents = true;
                listView1.Items.Clear();
                textBox1.Text = path;

                var p = new DirectoryInfo(path);
                CurrentDirectory = p;


                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = list;
                listView1.LargeImageList = list;
                if (Stuff.Libraries.OfType<FilesystemLibrary>().Any(z => z.BaseDirectory == p.FullName))
                {


                    listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = libraryRootObject });
                }



                AppendDirsToList(p.GetDirectories(), fltrs);
                AppendFilesToList(p.GetFiles(), fltrs);
            }
        }

        internal void UpdateAllLists()
        {
            tagControl.UpdateList(CurrentTag);
        }

        public DirectoryInfo SelectedDirectory
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is DirectoryInfo)
                    {
                        return si as DirectoryInfo;
                    }
                }

                return null;
            }
        }

        public FileInfo SelectedFile
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is FileInfo)
                    {
                        return si as FileInfo;
                    }
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateList(textBox1.Text, textBox2.Text);
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (Mode == ViewModeEnum.Tags)
            {
                tagControl.UpdateList(CurrentTag);
            }
            else
            {
                UpdateList(CurrentDirectory.FullName, textBox2.Text);
            }
        }

        public void SetFilter(string mask, bool dirFilter = false)
        {
            textBox2.Text = mask;
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
                return textBox2.Text;
            }
        }

        public List<TabInfo> Tabs = new List<TabInfo>();

        public void AddTab(TabInfo tinf)
        {

            Tabs.Add(tinf);
            var b = new Button() { Text = tinf.Hint };
            b.ContextMenuStrip = contextMenuStrip1;
            b.Click += (x, z) =>
            {
                var bb = x as Button;
                var tabinf = bb.Tag as TabInfo;
                textBox2.Text = tabinf.Filter;
                UpdateList(tabinf.Path, tabinf.Filter);

            };
            b.Tag = tinf;
            //b.Width = 140;
            int ww = 0;
            for (int i = 0; i < panel2.Controls.Count; i++)
            {
                ww += panel2.Controls[i].Width;
            }

            b.Left = ww;//panel2.Controls.Count * 140;
            using (var gr = CreateGraphics())
            {
                var msr = gr.MeasureString(b.Text, b.Font, new SizeF(500, 40));
                b.Width = (int)msr.Width + 20;
            }

            //b.AutoSize = true;
            panel2.Controls.Add(b);
        }
        private void button1_Click(object sender, EventArgs e)
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

        public string TabOwnerString;
        private Control todel;
        private void removeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = contextMenuStrip1.SourceControl;
            Stuff.Tabs.Remove(p.Tag as TabInfo);
            Tabs.Remove(p.Tag as TabInfo);
            Stuff.IsDirty = true;
            panel2.Controls.Remove(p);
        }

        private void openExplorerHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    Process.Start(d.FullName);
                }
                else
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as FileInfo;
                    Process.Start(d.DirectoryName);
                }

            }

            else if (CurrentDirectory != null && CurrentDirectory.Exists)
            {
                Process.Start(CurrentDirectory.FullName);
            }
        }

        private void openInHexToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void searcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    TextSearchForm tsf = new TextSearchForm();
                    tsf.SetPath(d.FullName);
                    tsf.FileListControl = this;
                    tsf.Show();

                }
                else
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as FileInfo;
                    TextSearchForm tsf = new TextSearchForm();
                    tsf.SetPath(d.DirectoryName);
                    tsf.FileListControl = this;
                    tsf.Show();
                }

            }

        }

        private void openInImageViewerToolStripMenuItem_Click(object sender, EventArgs e)
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
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    RunCmd(d.FullName);
                }
                else
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as FileInfo;
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
            UpdateList(CurrentDirectory.FullName, textBox2.Text);
        }

        private void calcMemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (listView1.SelectedItems.Count == 1 && (tag is DirectoryInfo || tag is FilesystemLibrary))
                {
                    DirectoryInfo d = null;
                    if (tag is FilesystemLibrary)
                    {
                        var l = tag as FilesystemLibrary;
                        d = new DirectoryInfo(l.BaseDirectory);
                    }
                    else
                    {
                        d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    }
                    List<FileInfo> files = new List<FileInfo>();
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
                    bool allFiles = true;
                    List<FileInfo> files = new List<FileInfo>();
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        var tag1 = listView1.SelectedItems[i].Tag;
                        if (!(tag1 is FileInfo))
                        {
                            allFiles = false;
                            break;
                        }
                        files.Add(tag1 as FileInfo);
                    }
                    if (allFiles)
                    {
                        Stuff.Info("Total size: " + Stuff.GetUserFriendlyFileSize(files.Sum(z => z.Length)));
                    }
                }
            }
        }

        private void findFileRepeatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is DirectoryInfo || tag is FilesystemLibrary)
                {

                    DirectoryInfo d = null;
                    if (tag is FilesystemLibrary)
                    {
                        var l = tag as FilesystemLibrary;
                        d = new DirectoryInfo(l.BaseDirectory);
                    }
                    else
                    {
                        d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    }

                    var groups = RepeatsWindow.FindRepeats(d);
                    if (groups.Count() == 0)
                    {
                        Stuff.Info("No repeates found.");
                    }
                    else
                    {
                        RepeatsWindow rp = new RepeatsWindow();
                        rp.MdiParent = mdi.MainForm;
                        rp.SetRepeats(d, groups.ToArray());
                        rp.Show();
                    }
                }
            }
        }


        void ExecuteSelected()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as FileInfo;
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
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    try
                    {
                        var f = listView1.SelectedItems[0].Tag as DirectoryInfo;
                        UpdateList(f.FullName, textBox2.Text);
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
                            UpdateLibrariesList(null, textBox2.Text);
                        }
                        else
                        {
                            var f = listView1.SelectedItems[0].Tag as FilesystemLibrary;
                            UpdateLibrariesList(f.BaseDirectory, textBox2.Text);
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

        private void CalcMd5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as FileInfo;
                    var md5 = Stuff.CalcMD5(f.FullName);
                    MessageBox.Show("MD5: " + md5, "Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void NewTextFileToolStripMenuItem_Click(object sender, EventArgs e)
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

        void DeleteSelected()
        {
            if (!ListView.Focused) return;



            ////////////
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {

                    var f = listView1.SelectedItems[0].Tag as FileInfo;

                    if (Stuff.Question("Delete file: " + SelectedFile.FullName + "?") == DialogResult.Yes)
                    {
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
                            File.Delete(f.FullName);
                            UpdateList(CurrentDirectory.FullName);
                        }
                    }

                }
                else
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    if (MessageBox.Show("Delete " + f.Name + " directory and all contents?", "Commander", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Directory.Delete(f.FullName, true);
                        UpdateList(CurrentDirectory.FullName);
                    }
                }
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        public Action<FileInfo> SelectedFileChanged;
        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (SelectedFileChanged != null)
            {
                SelectedFileChanged(SelectedFile);
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
            if (listView1.SelectedItems.Count == 1)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (!(tag is FileInfo)) return;
                var fi = tag as FileInfo;

                foreach (var item in Stuff.Tags)
                {
                    if (item.IsHidden && !Stuff.ShowHidden) continue;

                    var ss = new ToolStripMenuItem(item.Name) { CheckOnClick = true, CheckState = item.Files.Contains(fi.FullName) ? CheckState.Checked : CheckState.Unchecked };
                    ss.Tag = new Tuple<TagInfo, FileInfo>(item, fi);
                    ss.CheckedChanged += Ss_CheckedChanged;
                    setTagsToolStripMenuItem.DropDownItems.Add(ss);
                }
            }
            if (listView1.SelectedItems.Count> 1)
            {
                //todo:set multiple tags assign
            }
        }

        private void Ss_CheckedChanged(object sender, EventArgs e)
        {
            var f = (sender as ToolStripMenuItem).Tag as Tuple<TagInfo, FileInfo>;
            if (f.Item1.ContainsFile(f.Item2.FullName))
            {
                f.Item1.DeleteFile(f.Item2.FullName);
            }
            else
            {
                f.Item1.AddFile(f.Item2.FullName);
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
        }

        private void AddShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedFile != null)
            {
                Stuff.Shortcuts.Add(new ShortcutInfo(SelectedFile.FullName , SelectedFile.Name  ));
                Stuff.IsDirty = true;
                mdi.MainForm.AppendShortCutPanelButton(Stuff.Shortcuts.Last());

            }
        }

        
    }

    public class TabInfo
    {
        public string Hint;
        public string Path;
        public string Filter;
        public string Owner;
    }
    public enum ViewModeEnum
    {
        Filesystem, Libraries, Tags
    }

    public class ComboBoxItem
    {
        public object Tag;
        public string Name;
        public override string ToString()
        {
            return Name;
        }
    }
}
