using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Drawing.Imaging;
using commander.Properties;
using isoViewer;
using System.Xml.Linq;

namespace commander
{
    public partial class FileListControl : UserControl
    {
        public FileListControl()
        {
            InitializeComponent();
            listView1.ColumnClick += ListView1_ColumnClick;
            listView1.HideSelection = false;
            watcher.Changed += Watcher_Changed;
            Stuff.SetDoubleBuffered(listView1);
            tagControl.Init(this);

            listView1.MouseUp += ListView1_MouseUp;
            listView1.MouseLeave += ListView1_MouseLeave;
            listView1.MouseMove += ListView1_MouseMove;
            watcher.Created += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
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
            RunPreview();
        }

        internal void AddSelectedFileChangedAction(Action<IFileInfo> p)
        {
            SelectedFileChangedDelegates.Add(p);
        }

        public event Action<IFileInfo> SelectedFileChanged;

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

        void RunPreview()
        {
            PreviewThreadLoader = new Thread(ThreadLoader);
            PreviewThreadLoader.IsBackground = true;
            PreviewThreadLoader.Start();
        }

        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            HideHint();
            pressed = false;
        }
        #region preview
        private ListViewItem lastHovered = null;
        private void HideHint()
        {
            if (lastHighlighted != null)
            {
                lastHighlighted.BackColor = lastColor;
            }
            preview.Parent = null;
        }
        PreviewControl preview = new PreviewControl();
        public bool ShowPreview = true;

        public static bool AllowHints = true;
        public static HintModeEnum HintMode = HintModeEnum.Tags;
        public enum HintModeEnum
        {
            Tags, Image
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!AllowHints) return;

            //preview.Location = new Point(e.X + 20, e.Y);
            var itemat = listView1.GetItemAt(e.Location.X, e.Location.Y);
            if (itemat == null) { HideHint(); return; }
            if (!(itemat.Tag is IFileInfo)) { HideHint(); return; }

            if (itemat == lastHovered)
            {
                return;
            }

            lastHovered = itemat;
            var s = itemat.Tag as IFileInfo;

            if (HintMode == HintModeEnum.Tags)
            {
                var ss = Stuff.GetTagsOfFile(s.FullName);
                if (!ss.Any())
                {
                    HideHint();
                    return;
                }
            }
            if (HintMode == HintModeEnum.Image)
            {
                if (!File.Exists(s.FullName) || !(s.Extension.ToLower().EndsWith("png") || s.Extension.ToLower().EndsWith("jpg") || s.Extension.ToLower().EndsWith("bmp")))
                {
                    HideHint();
                    return;
                }
            }
            if (ShowPreview) ShowHint(e, itemat, s);
        }

        private int _previewWidth = 200;
        private object FLoaderPathLocker = new object();
        private int _previewHeight = 200;
        private ListViewItem lastHighlighted = null;
        private Color lastColor;
        private IFileInfo previewCut;
        private string loaderPath = "";
        private string previewPath = "";

        private void LoadAsyncPreview(IFileInfo cut)
        {
            previewPath = cut.FullName;
            lock (FLoaderPathLocker)
            {
                previewCut = cut;
                loaderPath = cut.FullName;
            }

            if (preview.Bitmap != null)
            {
                Bitmap bmp = preview.Bitmap;
                preview.Bitmap = null;
                bmp.Dispose();
            }
        }

        private void ShowHint(MouseEventArgs e, ListViewItem node, IFileInfo fi)
        {
            if (lastHighlighted != null)
            {

                lastHighlighted.BackColor = lastColor;
            }
            lastHighlighted = node;

            lastColor = node.BackColor;
            node.BackColor = SystemColors.Highlight;

            Control toplevel = this.TopLevelControl;
            toplevel = listView1;
            if (toplevel != null)
            {
                LoadAsyncPreview(fi);

                preview.BackColor = SystemColors.Highlight;
                preview.Width = _previewWidth + PreviewControl.PreviewGap * 2;
                preview.Height = _previewHeight + PreviewControl.PreviewGap * 2;

                UpdatePreviewLocation(node);

                toplevel.Controls.Add(preview);
                toplevel.Controls.SetChildIndex(preview, 0);
            }
            else
            {
                preview.Parent = null;
            }
        }

        void UpdatePreviewLocation(ListViewItem node)
        {
            Control toplevel = this.TopLevelControl;
            toplevel = listView1;
            Point p_node = toplevel.PointToClient(listView1.PointToScreen(node.Bounds.Location));
            var bnds = node.Bounds;
            preview.Location =
                  new Point(
                      Math.Min(
                          listView1.Left + listView1.Width - preview.Width - SystemInformation.VerticalScrollBarWidth - 10,
                          listView1.Left + bnds.Width
                      ),
                      Math.Min(p_node.Y, listView1.Height - preview.Height));
        }

        private Thread PreviewThreadLoader;

        private bool StopThreadLoader = false;

        private void ThreadLoader()
        {
            while (!StopThreadLoader)
            {
                Thread.MemoryBarrier();
                try
                {
                    string lp = "";
                    lock (FLoaderPathLocker)
                    {
                        lp = loaderPath;
                        loaderPath = "";
                    }

                    if (lp != "")
                    {
                        string lpbuff = lp;

                        string ext = Path.GetExtension(lp);

                        Bitmap bmp = null;
                        if (HintMode == HintModeEnum.Tags)
                        {
                            bmp = GetTagHint(this, lp);
                        }

                        if (HintMode == HintModeEnum.Image)
                        {
                            bmp = GetPreviewBmp(this, lp);
                        }

                        BeginInvoke((MethodInvoker)delegate { PreviewLoadedInvoked(lpbuff, bmp); });
                    }
                }
                catch { }
                Thread.Sleep(10);
            }
        }

        private Bitmap GetTagHint(FileListControl fileListControl, string lp)
        {

            var tags = Stuff.GetTagsOfFile(previewCut.FullName);
            var grr = CreateGraphics();
            float w = 0;
            var font = new Font("Arial", 8);
            List<List<TagInfo>> ttt = new List<List<TagInfo>>();

            for (int i = 0; i < tags.Length; i++)
            {
                if ((i % 3) == 0) { ttt.Add(new List<TagInfo>()); }
                ttt.Last().Add(tags[i]);
            }
            foreach (var item in ttt)
            {

                float w0 = 0;
                foreach (var zitem in item)
                {
                    var ms = grr.MeasureString(zitem.Name, font);
                    w0 += ms.Width + 5;
                }
                w = Math.Max(w, w0);
            }

            grr.Dispose();

            var bmp = new Bitmap((int)w, ttt.Count() * 14, PixelFormat.Format32bppArgb);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.Transparent);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float xx = 0;
            float yy = 0;
            foreach (var zitem in ttt)
            {
                foreach (var item in zitem)
                {
                    var ms = gr.MeasureString(item.Name, font);

                    gr.FillRectangle(Brushes.White, xx, yy, ms.Width + 3, ms.Height);
                    gr.DrawRectangle(Pens.Black, xx, yy, ms.Width + 3, ms.Height);
                    gr.DrawString(item.Name, font, Brushes.Blue, xx, yy);
                    xx += gr.MeasureString(item.Name, font).Width + 5;
                }
                yy += 14;
                xx = 0;
            }
            gr.Dispose();
            _previewWidth = bmp.Width;
            _previewHeight = bmp.Height;
            preview.Width = _previewWidth + PreviewControl.PreviewGap * 2;
            preview.Height = _previewHeight + PreviewControl.PreviewGap * 2;
            UpdatePreviewLocation(lastHovered);
            return bmp;
        }


        private void PreviewLoadedInvoked(string drawingname, Bitmap bmp)
        {

            try
            {
                if (previewPath == drawingname)
                {
                    if (preview.Bitmap != null)
                    {
                        Bitmap bmp_old = preview.Bitmap;
                        preview.Bitmap = bmp;
                        bmp_old.Dispose();
                    }
                    else
                    {
                        preview.Bitmap = bmp;
                    }
                }
            }
            catch
            { }
        }

        private Bitmap GetPreviewBmp(object sender, string drawingName)
        {
            try
            {

                var bmp0 = Bitmap.FromFile(previewCut.FullName);
                var aspect = bmp0.Height / (float)bmp0.Width;
                _previewHeight = (int)(_previewWidth * aspect);

                Bitmap bmp = new Bitmap(_previewWidth, _previewHeight);
                var gr = Graphics.FromImage(bmp);
                var koef = bmp.Width / (float)bmp0.Width;
                gr.ScaleTransform(koef, koef);


                gr.DrawImage(bmp0, 0, 0);

                string dim = bmp0.Width.ToString("0.#") + "x" + bmp0.Height.ToString("0.#");
                bmp0.Dispose();
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Font fontDim = new Font("Arial", 8, FontStyle.Regular);
                Brush brushDim = new SolidBrush(Color.FromArgb(255, 255, 255));
                Brush brushDimBack = new SolidBrush(Color.FromArgb(164, 64, 64, 164));
                var sfDim = new StringFormat();

                sfDim.LineAlignment = StringAlignment.Far;
                sfDim.Alignment = StringAlignment.Near;
                gr.ResetTransform();
                gr.FillRectangle(brushDimBack, new RectangleF(0, bmp.Height - 14, gr.MeasureString(dim, fontDim).Width, 14));
                gr.DrawString(dim, fontDim, brushDim, new PointF(0, bmp.Height), sfDim);
                gr.Dispose();

                preview.Width = _previewWidth + 10;
                preview.Height = _previewHeight + 10;

                return bmp;
            }
            catch (Exception ex)
            {
                Bitmap bmp = new Bitmap(_previewWidth, _previewHeight);
                return bmp;
            }
        }
        #endregion

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
            UpdateList(new DirectoryInfoWrapper(CurrentDirectory.FullName), textBox2.Text);
        }

        public void SetPath(string path)
        {
            textBox1.Text = path;

        }

        public void NavigateTo(string path)
        {
            SetPath(path);
            UpdateList(new DirectoryInfoWrapper(textBox1.Text), textBox2.Text);
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
            UpdateList(new DirectoryInfoWrapper(path), textBox2.Text);
        }

        public static Dictionary<string, Tuple<Bitmap, int>> Icons = new Dictionary<string, Tuple<Bitmap, int>>();
        public static Dictionary<string, Tuple<Bitmap, int>> ExeIcons = new Dictionary<string, Tuple<Bitmap, int>>();
        public static Tuple<Bitmap, int> GetBitmapOfFile(string fn)
        {
            var d = Path.GetExtension(fn).ToLower();
            if (d == ".exe" || d == ".ico")
            {
                if (!ExeIcons.ContainsKey(fn))
                {
                    var bb = Bitmap.FromHicon(Stuff.ExtractAssociatedIcon(fn).Handle);
                    bb.MakeTransparent();
                    Stuff.list.Images.Add(bb);
                    ExeIcons.Add(fn, new Tuple<Bitmap, int>(bb, Stuff.list.Images.Count - 1));
                }
                return ExeIcons[fn];
            }
            if (!Icons.ContainsKey(d))
            {
                var bb = Bitmap.FromHicon(Stuff.ExtractAssociatedIcon(fn).Handle);
                bb.MakeTransparent();
                Stuff.list.Images.Add(bb);
                Icons.Add(d, new Tuple<Bitmap, int>(bb, Stuff.list.Images.Count - 1));
            }

            return Icons[d];
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
                        tp = GetBitmapOfFile(fileInfo.FullName);
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
                UpdateList();
            }
        }

        public void UpdateList()
        {
            UpdateList(textBox1.Text, textBox2.Text);

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
            var b = new MyToolStripButton() { Text = tinf.Hint, DisplayStyle = ToolStripItemDisplayStyle.Text };

            b.ContextMenuStrip = contextMenuStrip1;
            b.Click += (x, z) =>
            {
                var bb = x as ToolStripButton;
                var tabinf = bb.Tag as TabInfo;
                textBox2.Text = tabinf.Filter;
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
            UpdateList(CurrentDirectory.FullName, textBox2.Text);
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
                        UpdateList(f, textBox2.Text);
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
                            UpdateLibrariesList(new DirectoryInfoWrapper(f.BaseDirectory), textBox2.Text);
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

        void DeleteSelected()
        {
            if (!ListView.Focused) return;



            ////////////
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag is IFileInfo)
                {

                    var f = listView1.SelectedItems[0].Tag as IFileInfo;

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
                if (listView1.SelectedItems[0].Tag is IDirectoryInfo)
                {
                    var f = listView1.SelectedItems[0].Tag as IDirectoryInfo;
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
            UpdateStatus();
            if (SelectedFileChangedDelegates.Any())
            {
                if (SelectedFileChanged != null)
                {
                    SelectedFileChanged(SelectedFile);
                }
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
            if (listView1.SelectedItems.Count == 1)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (!(tag is IFileInfo)) return;
                var fi = tag as IFileInfo;

                List<TagInfo> cands = new List<TagInfo>();
                foreach (var item in Stuff.Tags.OrderBy(z => z.Name))
                {
                    if (item.IsHidden && !Stuff.ShowHidden) continue;
                    cands.Add(item);
                }

                if (cands.Count > 20)
                {
                    int grps = cands.Count / 20;
                    int index = 0;
                    List<ToolStripMenuItem> ii = new List<ToolStripMenuItem>();
                    for (int i = 0; i < grps; i++)
                    {
                        ToolStripMenuItem grp1 = new ToolStripMenuItem("Group #" + i);
                        setTagsToolStripMenuItem.DropDownItems.Add(grp1);
                        grp1.DropDown.Closing += DropDown_Closing;
                        for (int j = 0; j < 20; j++)
                        {
                            var item = cands[index++];
                            var ss = new ToolStripMenuItem(item.Name) { CheckOnClick = true, CheckState = item.Files.Contains(fi.FullName) ? CheckState.Checked : CheckState.Unchecked };
                            ss.Tag = new Tuple<TagInfo, IFileInfo>(item, fi);
                            ss.CheckedChanged += Ss_CheckedChanged;
                            grp1.DropDownItems.Add(ss);
                        }
                    }


                }
                else
                {
                    foreach (var item in cands)
                    {
                        var ss = new ToolStripMenuItem(item.Name) { CheckOnClick = true, CheckState = item.Files.Contains(fi.FullName) ? CheckState.Checked : CheckState.Unchecked };
                        ss.Tag = new Tuple<TagInfo, IFileInfo>(item, fi);
                        ss.CheckedChanged += Ss_CheckedChanged;
                        setTagsToolStripMenuItem.DropDownItems.Add(ss);
                    }
                }
            }
            if (listView1.SelectedItems.Count > 1)
            {
                //todo:set multiple tags assign
            }
        }

        private void Ss_CheckedChanged(object sender, EventArgs e)
        {
            var f = (sender as ToolStripMenuItem).Tag as Tuple<TagInfo, IFileInfo>;
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
                        Stuff.Info("No repeates found.");
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
                        Stuff.Info("No repeates found.");
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
                        if (!tag.Files.Contains(path))
                        {
                            tag.AddFile(path);
                        }
                    }
                }
            }
        }

        void AppendFileToIso(FileStream fs, string path, byte[] bb)
        {
            var rep = path.Trim(new char[] { '\\' });
            var nm = Encoding.BigEndianUnicode.GetBytes(rep);
            fs.WriteByte((byte)rep.Length);
            fs.Write(nm, 0, nm.Length);
            var nn = BitConverter.GetBytes(bb.Length);
            fs.Write(nn, 0, nn.Length);
            fs.Write(bb, 0, bb.Length);
        }
        private void PackAsLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedDirectory == null) return;
            var dir = SelectedDirectory;
            var fls = Stuff.GetAllFiles(dir);
            var drs = Stuff.GetAllDirs(dir);
            //pack with .indx directory (+tags,+meta infos,etc.)
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "iso";
            sfd.Filter = "iso images|*.iso";
            //save all as one big file info.
            //generate meta.xml
            var mm = GenerateMetaXml(dir);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                {
                    AppendFileToIso(fs, ".indx\\meta.xml", Encoding.UTF8.GetBytes(mm));
                    foreach (var item in fls)
                    {
                        if (item.Length > 1024 * 1024 * 10) continue;//10 Mb

                        var bb = File.ReadAllBytes(item.FullName);
                        var rep = item.FullName.Replace(dir.FullName, "").Trim(new char[] { '\\' });
                        AppendFileToIso(fs, rep, bb);
                    }
                }
            }
        }

        private string GenerateMetaXml(IDirectoryInfo dir)
        {
            var fls = Stuff.GetAllFiles(dir);
            List<TagInfo> tags = new List<TagInfo>();
            foreach (var item in fls)
            {
                var ww = Stuff.Tags.Where(z => z.Files.Contains(item.FullName));
                tags.AddRange(ww);
            }
            tags = tags.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"/>");
            sb.AppendLine("<root>");
            sb.AppendLine("<tags>");
            foreach (var item in tags)
            {
                sb.AppendLine($"<tag name=\"{item.Name}\">");
                var aa = fls.Where(z => item.Files.Contains(z.FullName)).ToArray();
                foreach (var aitem in aa)
                {
                    sb.AppendLine($"<file><![CDATA[{aitem.FullName.Replace(dir.FullName, "").Trim(new char[] { '\\' })}]]></file>");
                }
                sb.AppendLine($"</tag>");
            }
            sb.AppendLine("</tags>");
            sb.AppendLine("</root>");

            return sb.ToString();

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

    public class FileContextMenuItem
    {
        public string Title;
        public string AppName;
        public string Arguments;
    }
}

