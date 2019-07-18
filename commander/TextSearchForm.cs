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

        }
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
        public FileListControl FileListControl;

        public void loop(DirectoryInfo _d, Action fileProcessed)
        {

            Queue<DirectoryInfo> q = new Queue<DirectoryInfo>();
            q.Enqueue(_d);
            while (q.Any())
            {
                var d = q.Dequeue();
                label4.Text = d.FullName;
                foreach (var item in d.GetFiles())
                {
                    try
                    {
                        fileProcessed();
                        if (!item.Extension.Contains(textBox3.Text)) continue;
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
                    catch (Exception ex)
                    {

                    }
                }
                foreach (var item in d.GetDirectories())
                {
                    q.Enqueue(item);
                }
            }
        }

        public void rec1(DirectoryInfo d, Action fileProcessed)
        {

            label4.Text = d.FullName;
            foreach (var item in d.GetFiles())
            {
                fileProcessed();
                if (!item.Extension.Contains(textBox3.Text)) continue;
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
            var d = new DirectoryInfo(textBox1.Text);

            searchThread = new Thread(() =>
             {
                 var files = FileListControl.GetAllFiles(d);
                 progressBar1.Value = 0;
                 progressBar1.Maximum = files.Count;

                 listView2.Items.Clear();
                 loop(d, () => { progressBar1.Value++; });
                 //rec1(d, () => { progressBar1.Value++; });
                 toolStripStatusLabel1.Text = "Files found: " + listView2.Items.Count;
                 button1.Text = "Start";
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
                var f = lvi as FileInfo;
                linkLabel1.Text = f.FullName;
                listView3.Items.Clear();
                richTextBox1.Clear();
                richTextBox1.BackColor = Color.Gray;
                richTextBox1.Enabled = false;
                if (f.Length < maxShowableFileSize * 1024)
                {
                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Enabled = true;
                    var lns = File.ReadAllLines(f.FullName);
                    richTextBox1.Lines = lns;

                    int cnt = 0;
                    foreach (var ln in lns)
                    {

                        if (ln.ToLower().Contains(textBox2.Text.ToLower()))
                        {
                            listView3.Items.Add(new ListViewItem(new string[] { ln }) { Tag = cnt });
                        }

                        cnt++;
                    }
                }
                else
                {
                    richTextBox1.Text = "File size exceeded";
                }

            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                var ln = (int)listView3.SelectedItems[0].Tag;
                richTextBox1.SelectionStart = richTextBox1.Find(richTextBox1.Lines[ln]);
                richTextBox1.ScrollToCaret();
                richTextBox1.ScrollToCaret();
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
                var f = listView2.SelectedItems[0].Tag as FileInfo;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.WorkingDirectory = f.DirectoryName;
                psi.FileName = f.FullName;
                Process.Start(psi);
            }
        }

        private void GoToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchThread != null)
            {
                MessageBox.Show("First stop the search.");
                return;
            }
            if (listView2.SelectedItems.Count > 0)
            {
                var f = listView2.SelectedItems[0].Tag as FileInfo;
                FileListControl.UpdateList(f.DirectoryName);
                FileListControl.SetFilter(f.Name);
                Close();
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
    }
}
