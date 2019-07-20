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

namespace commander
{
    public partial class FileListControl : UserControl
    {
        public FileListControl()
        {
            InitializeComponent();
            listView1.ColumnClick += ListView1_ColumnClick;
            watcher.Changed += Watcher_Changed;

            tagControl.Init(this);
            watcher.Created += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            filesControl = listView1;
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
                Icons.Add(d, Bitmap.FromHicon(Icon.ExtractAssociatedIcon(fn).Handle));
            }
            return Icons[d];
        }
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
                ImageList list = new ImageList();
                list.TransparentColor = Color.Black;
                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = list;
                listView1.LargeImageList = list;
                listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = p.Parent });


                foreach (var directoryInfo in p.GetDirectories())
                {

                    bool was = false;
                    var bmp = DefaultIcons.FolderLarge;

                    list.Images.Add(bmp.ToBitmap());
                    was = true;

                    if (checkBox1.Checked && !IsFilterPass(directoryInfo.Name, fltrs)) continue;

                    listView1.Items.Add(new ListViewItem(new string[] { directoryInfo.Name, "", directoryInfo.LastWriteTime.ToString() })
                    {
                        Tag = directoryInfo,
                        ImageIndex = was ? (list.Images.Count - 1) : -1
                    });

                }
                foreach (var directoryInfo in p.GetFiles())
                {
                    try
                    {
                        var bmp = GetBitmapOfFile(directoryInfo.FullName);
                        bmp.MakeTransparent();
                        list.Images.Add(bmp);
                        if (!IsFilterPass(directoryInfo.Name, fltrs)) continue;
                        var len = directoryInfo.Length / 1024;
                        listView1.Items.Add(
                            new ListViewItem(new string[]
                            {
                        directoryInfo.Name, len+"Kb", directoryInfo.LastWriteTime.ToString()
                            })
                            {
                                Tag = directoryInfo,
                                ImageIndex = list.Images.Count - 1

                            });
                    }
                    catch (Exception ex)
                    {

                    }
                }
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

        public void UpdateTagsList()
        {
            tagControl.UpdateList(CurrentTag);
        }

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

                listView1.Items.Clear();
                textBox1.Text = path;

                var p = new DirectoryInfo(path);
                CurrentDirectory = p;
                ImageList list = new ImageList();
                list.TransparentColor = Color.Black;
                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = list;
                listView1.LargeImageList = list;
                if (Stuff.Libraries.OfType<FilesystemLibrary>().Any(z => z.BaseDirectory == p.FullName))
                {


                    listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = libraryRootObject });
                }


                foreach (var directoryInfo in p.GetDirectories())
                {

                    bool was = false;
                    var bmp = DefaultIcons.FolderLarge;

                    list.Images.Add(bmp.ToBitmap());
                    was = true;

                    if (checkBox1.Checked && !IsFilterPass(directoryInfo.Name, fltrs)) continue;

                    listView1.Items.Add(new ListViewItem(new string[] { directoryInfo.Name, "", directoryInfo.LastWriteTime.ToString() })
                    {
                        Tag = directoryInfo,
                        ImageIndex = was ? (list.Images.Count - 1) : -1
                    });

                }
                foreach (var directoryInfo in p.GetFiles())
                {
                    var ico = Icon.ExtractAssociatedIcon(directoryInfo.FullName);
                    var bmp = Bitmap.FromHicon(ico.Handle);
                    bmp.MakeTransparent();
                    list.Images.Add(bmp);
                    if (!IsFilterPass(directoryInfo.Name, fltrs)) continue;
                    var len = directoryInfo.Length / 1024;
                    listView1.Items.Add(
                        new ListViewItem(new string[]
                        {
                        directoryInfo.Name, len+"Kb", directoryInfo.LastWriteTime.ToString()
                        })
                        {
                            Tag = directoryInfo,
                            ImageIndex = list.Images.Count - 1

                        });
                }
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
            UpdateList(CurrentDirectory.FullName, textBox2.Text);
        }

        public void SetFilter(string mask)
        {
            textBox2.Text = mask;
        }

        public void UpdateDrivesList()
        {
            var drivs = System.IO.DriveInfo.GetDrives();
            comboBox1.Items.Clear();
            foreach (var item in drivs)
            {
                comboBox1.Items.Add(item);
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
            Stuff.IsDirty = true;
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
                AddTab(new TabInfo()
                {
                    Hint = atw.Hint,
                    Path = atw.Path,
                    Filter = atw.Filter
                });
            }
        }

        private Control todel;
        private void removeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = contextMenuStrip1.SourceControl;
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

                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    List<FileInfo> files = new List<FileInfo>();
                    GetAllFiles(d, files);
                    var total = files.Sum(z => z.Length);
                    MessageBox.Show("total mem: " + total / 1024 / 1024 + " Mb");
                }
            }
        }

        public static List<FileInfo> GetAllFiles(DirectoryInfo dir, List<FileInfo> files = null)
        {
            if (files == null)
            {
                files = new List<FileInfo>();
            }

            try
            {
                foreach (var d in dir.GetDirectories())
                {
                    GetAllFiles(d, files);
                }
            }
            catch (UnauthorizedAccessException ex)
            {

            }


            foreach (var file in dir.GetFiles())
            {
                files.Add(file);
            }
            return files;
        }

        private void findFileRepeatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    var d = listView1.SelectedItems[0].Tag as DirectoryInfo;
                    List<FileInfo> files = new List<FileInfo>();
                    GetAllFiles(d, files);

                    var arr = files.GroupBy(z => Stuff.CalcMD5(z.FullName)).ToArray();
                    var cnt = arr.Count(z => z.Count() > 1);
                    var gr = arr.Where(z => z.Count() > 1).ToArray();
                    if (cnt == 0)
                    {
                        MessageBox.Show("no repeates found");
                    }
                    else
                    {
                        RepeatsWindow rp = new RepeatsWindow();

                        rp.SetRepeats(gr.Select(z => z.ToArray()).ToArray());
                        rp.ShowDialog();
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
        }


        private void ComboBox1_DropDown(object sender, EventArgs e)
        {
            UpdateDrivesList();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = comboBox1.Text;
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

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is FileInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as FileInfo;
                    if (MessageBox.Show("Delete " + f.Name + "?", "Commander", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        File.Delete(f.FullName);
                        UpdateList(CurrentDirectory.FullName);
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
    }

    public class TabInfo
    {
        public string Hint;
        public string Path;
        public string Filter;
    }
    public enum ViewModeEnum
    {
        Filesystem, Libraries, Tags
    }
}
