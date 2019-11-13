using PluginLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class TextSearchForm : Form
    {
        public TextSearchForm()
        {
            InitializeComponent();
            EnableDoubleBuffer(listView2);

            textBox8.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox8.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var acsc = new AutoCompleteStringCollection();
            acsc.AddRange(Stuff.Tags.Where(z => !z.IsHidden || (z.IsHidden && Stuff.ShowHidden)).Select(z => z.Name).ToArray());
            textBox8.AutoCompleteCustomSource = acsc;
            previewPbox.SizeMode = PictureBoxSizeMode.Zoom;
            previewPbox.Dock = DockStyle.Fill;
            pictureBox1.Click += PictureBox1_Click;
            pictureBox1.PreviewKeyDown += PictureBox1_PreviewKeyDown;
        }

        private void PictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
            {
                var img = Clipboard.GetImage();
                if (img != null)
                {
                    pictureBox1.Image = img;
                }
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }



        PictureBox previewPbox = new PictureBox();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr handle, int messg, int wparam, int lparam);

        public static void SetExtendedStyle(Control control, ListViewExtendedStyles exStyle)
        {
            ListViewExtendedStyles styles;
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            styles |= exStyle;
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
        public enum ListViewExtendedStyles
        {
            /// <summary>
            /// LVS_EX_GRIDLINES
            /// </summary>
            GridLines = 0x00000001,
            /// <summary>
            /// LVS_EX_SUBITEMIMAGES
            /// </summary>
            SubItemImages = 0x00000002,
            /// <summary>
            /// LVS_EX_CHECKBOXES
            /// </summary>
            CheckBoxes = 0x00000004,
            /// <summary>
            /// LVS_EX_TRACKSELECT
            /// </summary>
            TrackSelect = 0x00000008,
            /// <summary>
            /// LVS_EX_HEADERDRAGDROP
            /// </summary>
            HeaderDragDrop = 0x00000010,
            /// <summary>
            /// LVS_EX_FULLROWSELECT
            /// </summary>
            FullRowSelect = 0x00000020,
            /// <summary>
            /// LVS_EX_ONECLICKACTIVATE
            /// </summary>
            OneClickActivate = 0x00000040,
            /// <summary>
            /// LVS_EX_TWOCLICKACTIVATE
            /// </summary>
            TwoClickActivate = 0x00000080,
            /// <summary>
            /// LVS_EX_FLATSB
            /// </summary>
            FlatsB = 0x00000100,
            /// <summary>
            /// LVS_EX_REGIONAL
            /// </summary>
            Regional = 0x00000200,
            /// <summary>
            /// LVS_EX_INFOTIP
            /// </summary>
            InfoTip = 0x00000400,
            /// <summary>
            /// LVS_EX_UNDERLINEHOT
            /// </summary>
            UnderlineHot = 0x00000800,
            /// <summary>
            /// LVS_EX_UNDERLINECOLD
            /// </summary>
            UnderlineCold = 0x00001000,
            /// <summary>
            /// LVS_EX_MULTIWORKAREAS
            /// </summary>
            MultilWorkAreas = 0x00002000,
            /// <summary>
            /// LVS_EX_LABELTIP
            /// </summary>
            LabelTip = 0x00004000,
            /// <summary>
            /// LVS_EX_BORDERSELECT
            /// </summary>
            BorderSelect = 0x00008000,
            /// <summary>
            /// LVS_EX_DOUBLEBUFFER
            /// </summary>
            DoubleBuffer = 0x00010000,
            /// <summary>
            /// LVS_EX_HIDELABELS
            /// </summary>
            HideLabels = 0x00020000,
            /// <summary>
            /// LVS_EX_SINGLEROW
            /// </summary>
            SingleRow = 0x00040000,
            /// <summary>
            /// LVS_EX_SNAPTOGRID
            /// </summary>
            SnapToGrid = 0x00080000,
            /// <summary>
            /// LVS_EX_SIMPLESELECT
            /// </summary>
            SimpleSelect = 0x00100000
        }

        public enum ListViewMessages
        {
            First = 0x1000,
            SetExtendedStyle = (First + 54),
            GetExtendedStyle = (First + 55),
        }
        public static void EnableDoubleBuffer(Control control)
        {
            ListViewExtendedStyles styles;
            // read current style
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            // enable double buffer and border select
            styles |= ListViewExtendedStyles.DoubleBuffer | ListViewExtendedStyles.BorderSelect;
            // write new style
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
        //public FileListControl FileListControl;


        public bool IsExtFilterPass(string filter, string name)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            var aa = filter.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            return aa.Any(z => name.Contains(z));
        }

        public class QueueItem
        {
            public QueueItem(IDirectoryInfo d, int l)
            {
                Level = l;
                Info = d;
            }
            public int Level;
            public IDirectoryInfo Info;
        }

        public HashSet<string> searchHash = new HashSet<string>();

        public void loop(IDirectoryInfo _d, Action fileProcessed, int? maxLevel)
        {

            Queue<QueueItem> q = new Queue<QueueItem>();
            q.Enqueue(new commander.TextSearchForm.QueueItem(_d, 0));
            while (q.Any())
            {
                var dd = q.Dequeue();
                if (maxLevel != null)
                {
                    if (dd.Level > maxLevel) continue;
                }
                var d = dd.Info;
                label4.Invoke((Action)(() =>
                {
                    label4.Text = d.FullName;
                }));

                try
                {
                    foreach (var item in d.GetFiles())
                    {
                        try
                        {
                            fileProcessed();
                            if (!IsExtFilterPass(textBox3.Text, item.Extension)) continue;
                            if (!IsMd5FilterPass(textBox6.Text, item)) continue;
                            if (!IsImageFilter(item)) continue;
                            if (tagStorageSearchMode)
                            {

                                if (!tagFilters.All(z => z.ContainsFile(item.FullName)))
                                {
                                    continue;

                                }
                                if (!exceptTagFilters.All(z => !z.ContainsFile(item.FullName)))
                                {
                                    continue;

                                }
                                if (!searchHash.Add(item.FullName))
                                {
                                    continue;
                                }
                            }
                            //if (!item.Extension.Contains(textBox3.Text)) continue;
                            if (!item.Name.ToLower().Contains(textBox4.Text.ToLower())) continue;


                            if (!item.Name.ToLower().Contains(textBox4.Text.ToLower())) continue;

                            bool add = false;
                            if (!string.IsNullOrEmpty(textBox2.Text))
                            {

                                var t = d.Filesystem.ReadAllLines(item.FullName);
                                if (t.Any(z => z.ToUpper().Contains(textBox2.Text.ToUpper())))
                                {
                                    add = true;
                                }
                            }
                            else
                            {
                                add = true;
                            }


                            #region meta check
                            if (!string.IsNullOrEmpty(textBox9.Text))
                            {
                                var mt = Stuff.GetMetaInfoOfFile(item);
                                if (mt == null) continue;
                                if (mt.Infos.Any(z => z is KeywordsMetaInfo))
                                {
                                    var k = mt.Infos.First(z => z is KeywordsMetaInfo) as KeywordsMetaInfo;
                                    if (!k.Keywords.ToLower().Contains(textBox9.Text.ToLower()))
                                    {
                                        continue;
                                    }
                                }
                                else { continue; }
                            }
                            #endregion

                            if (add)
                            {
                                listView2.BeginUpdate();
                                listView2.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                                listView2.EndUpdate();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    foreach (var item in d.GetDirectories())
                    {
                        q.Enqueue(new QueueItem(item, dd.Level + 1));
                    }
                }
                catch (UnauthorizedAccessException ex)
                {

                }
            }
        }

        private bool IsMd5FilterPass(string text, IFileInfo item)
        {
            if (string.IsNullOrEmpty(text)) return true;
            if (!(item is FileInfoWrapper)) return true;
            var md5 = Stuff.CalcMD5((item as FileInfoWrapper).FullName);
            return (md5 == text);

        }
        private bool IsImageFilter(IFileInfo item)
        {
            if (!checkBox3.Checked) return true;
            var h = ImagesDeduplicationWindow.GetImageHash(item.FullName);
            return Diff(h, imgHashInt) < 800;

        }

        public int Diff(int[] ar1, int[] ar2)
        {
            int d = 0;
            for (int i = 0; i < ar1.Length; i++)
            {
                d += Math.Abs(ar1[i] - ar2[i]);
            }
            return d;
        }
        public void rec1(DirectoryInfo d, Action fileProcessed)
        {

            label4.Text = d.FullName;
            foreach (var item in d.GetFiles())
            {
                fileProcessed();
                if (!IsExtFilterPass(textBox3.Text, item.Extension)) continue;
                //if (!item.Extension.Contains(textBox3.Text)) continue;
                if (!item.Name.ToLower().Contains(textBox4.Text.ToLower())) continue;
                bool add = false;
                if (!string.IsNullOrEmpty(textBox2.Text))
                {
                    var t = File.ReadAllLines(item.FullName);
                    if (t.Any(z => z.ToUpper().Contains(textBox2.Text.ToUpper())))
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }

                if (add)
                {
                    listView2.BeginUpdate();
                    listView2.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                    listView2.EndUpdate();
                }
            }
            foreach (var item in d.GetDirectories())
            {
                rec1(item, fileProcessed);
            }
        }

        Thread searchThread = null;
        //string imgHash = null;
        int[] imgHashInt = null;
        private void button1_Click(object sender, EventArgs e)
        {


            if (searchThread != null)
            {
                button1.Text = "Start";
                searchThread.Abort();
                searchThread = null;
                return;


            }
            button1.Text = "Stop";

            var spl = textBox1.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var dd = (spl.Select(z => new DirectoryInfoWrapper(z))).OfType<IDirectoryInfo>().ToList();
            if (tagStorageSearchMode)
            {
                dd = Stuff.Tags.Select(z => new VirtualDirectoryInfo(null) { ChildsFiles = z.Files.Select(u => u).OfType<IFileInfo>().ToList() }).OfType<IDirectoryInfo>().ToList();
            }

            if (checkBox2.Checked)
            {
                dd.Clear();
                var vfs = new VirtualFilesystem() { UseIndexes = true };

                var vdir = new VirtualDirectoryInfo(vfs);
                vdir.Name = "indexes";
                vdir.FullName = "indexes";


                foreach (var item in Stuff.Indexes)
                {
                    vdir.ChildsFiles.Add(new VirtualFileInfo(new FileInfoWrapper(item.Path), vdir));

                }
                vfs.Files.AddRange(vdir.ChildsFiles.OfType<VirtualFileInfo>());
                dd.Add(vdir);
            }

            searchThread = new Thread(() =>
         {
             List<IFileInfo> files = new List<IFileInfo>();

             if (tagStorageSearchMode)
             {
                 foreach (var item in Stuff.Tags)
                 {
                     files.AddRange(item.Files.Select(z => z));
                 }
             }
             else
             {
                 foreach (var item in dd)
                 {
                     files.AddRange(Stuff.GetAllFiles(item, level: 0, maxlevel: (int)numericUpDown1.Value));
                 }
             }

             progressBar1.Invoke((Action)(() =>
             {
                 progressBar1.Value = 0;
                 progressBar1.Maximum = files.Count;
             }));


             listView2.Items.Clear();
             searchHash.Clear();

             if (checkBox3.Checked)
             {
                 var b = pictureBox1.Image as Bitmap;
                 b.SetResolution(96, 96);
                 imgHashInt = ImagesDeduplicationWindow.GetImageHash2D(b);
                 //imgHash = ImagesDeduplicationWindow.ToHash(imgHashInt);
             }
             foreach (var d in dd)
             {
                 loop(d, () =>
                  {
                      progressBar1.Invoke((Action)(() =>
                      {
                          progressBar1.Value++;
                      }));
                  }, (int)numericUpDown1.Value);
             }

             //rec1(d, () => { progressBar1.Value++; });
             progressBar1.Invoke((Action)(() =>
         {
             toolStripStatusLabel1.Text = "Files found: " + listView2.Items.Count;
             button1.Text = "Start";
         }));
             searchThread = null;
         });
            searchThread.IsBackground = true;
            searchThread.Start();


        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var lvi = listView2.SelectedItems[0].Tag;
                var f = lvi as IFileInfo;
                linkLabel1.Text = f.FullName;
                listView3.Items.Clear();
                richTextBox1.Clear();
                richTextBox1.BackColor = Color.Gray;
                richTextBox1.Enabled = false;
                panel2.Controls.Clear();
                if (f.Extension.EndsWith("png") || f.Extension.EndsWith("jpg"))
                {
                    previewPbox.Image = Bitmap.FromFile(f.FullName);
                    panel2.Controls.Add(previewPbox);
                }
                else
                {
                    panel2.Controls.Add(richTextBox1);
                }
                if (f.Length < maxShowableFileSize * 1024)
                {
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Enabled = true;
                    var lns = f.Filesystem.ReadAllLines(f.FullName);
                    richTextBox1.Lines = lns;

                    int cnt = 0;
                    if (!string.IsNullOrEmpty(textBox2.Text))
                    {
                        foreach (var ln in lns)
                        {
                            if (ln.ToLower().Contains(textBox2.Text.ToLower()))
                            {
                                listView3.Items.Add(new ListViewItem(new string[] { ln }) { Tag = new FlatTextSearchPositionInfo(cnt) });
                            }

                            cnt++;
                        }
                    }
                }
                else
                {
                    richTextBox1.Text = "File size exceeded";
                }
            }
        }




        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                var ln = listView3.SelectedItems[0].Tag as ITextSearcPositionInfo;
                if (ln is FlatTextSearchPositionInfo)
                {
                    ln.Navigate(richTextBox1);
                }
                if (ln is DjvuTextSearchPositionInfo)
                {

                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(linkLabel1.Text);
        }



        public void SetPath(string dFullName)
        {

            textBox1.Text = dFullName;
        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void ListView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Execute();
        }

        private void ExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Execute();
        }

        void Execute()
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var f = listView2.SelectedItems[0].Tag as IFileInfo;
                Stuff.ExecuteFile(f);
            }
        }

        private void GoToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (searchThread != null)
            {
                MessageBox.Show("First stop the search.");
                return;
            }*/
            if (listView2.SelectedItems.Count > 0)
            {
                var f = listView2.SelectedItems[0].Tag as IFileInfo;
                var exps = mdi.MainForm.MdiChildren.OfType<Explorer>().ToArray();
                if (exps.Any())
                {
                    var e1 = exps.First();

                    e1.FileListControls[0].UpdateList(f.DirectoryName);
                    e1.FileListControls[0].SetFilter(f.Name, true);
                    e1.Activate();
                }
                // Close();
            }
        }

        int maxShowableFileSize = 10;
        private void TextBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                maxShowableFileSize = int.Parse(textBox5.Text);
            }
            catch (Exception ex)
            {

            }
        }
        List<TagInfo> tagFilters = new List<TagInfo>();
        List<TagInfo> exceptTagFilters = new List<TagInfo>();

        public bool tagStorageSearchMode = false;
        private void Button2_Click(object sender, EventArgs e)
        {
            var fr = Stuff.Tags.FirstOrDefault(z => z.Name == textBox8.Text);
            if (fr == null) return;
            if (tagFilters.Contains(fr)) return;
            if (exceptTagFilters.Contains(fr)) return;
            tagFilters.Add(fr);
            UpdateTagFiltersText();
        }

        void UpdateTagFiltersText()
        {
            textBox7.Text = tagFilters.Aggregate("", (x, y) => x + "[+" + y.Name + "] ");
            textBox7.Text += exceptTagFilters.Aggregate("", (x, y) => x + "[-" + y.Name + "] ");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            tagFilters.Clear();
            exceptTagFilters.Clear();
            UpdateTagFiltersText();
        }

        private void TextBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !checkBox1.Checked;
            tagStorageSearchMode = checkBox1.Checked;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var fr = Stuff.Tags.FirstOrDefault(z => z.Name == textBox8.Text);
            if (fr == null) return;
            if (tagFilters.Contains(fr)) return;
            if (exceptTagFilters.Contains(fr)) return;
            exceptTagFilters.Add(fr);
            UpdateTagFiltersText();
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public interface ITextSearcPositionInfo
    {
        void Navigate(Control c);
    }
    public class FlatTextSearchPositionInfo : ITextSearcPositionInfo
    {
        public int Line;


        public FlatTextSearchPositionInfo(int cnt)
        {
            Line = cnt;
        }

        public void Navigate(Control c)
        {
            var rtb = c as RichTextBox;
            rtb.SelectionStart = rtb.Find(rtb.Lines[Line]);
            rtb.ScrollToCaret();
        }
    }

    public class DjvuTextSearchPositionInfo : ITextSearcPositionInfo
    {
        public int Page;
        public Rectangle Area;

        public void Navigate(Control c)
        {

        }
    }
}
