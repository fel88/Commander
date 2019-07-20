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
            fileListControl1.UpdateList(drvs[0].Name, "");
            fileListControl2.UpdateList(drvs[0].Name, "");
            previewer = new ImgViewerPanel() { Dock = DockStyle.Fill };
            textPreviewer = new TextPreviewer() { Dock = DockStyle.Fill };

            fileListControl1.SelectedFileChanged = (x) =>
            {
                if (splitContainer1.Panel2.Controls.Contains(textPreviewer))
                {
                    string[] exts = new string[] { ".txt", ".cs", ".js", ".xml", ".htm", ".bat", ".html", ".log", ".csproj", ".config", ".resx", ".sln", ".settings", ".md", ".cpp", ".h", ".asm" };
                    if (exts.Contains(x.Extension))
                    {
                        textPreviewer.LoadFile(x);
                    }
                    else
                    {
                        textPreviewer.Disable();
                    }
                }
                if (splitContainer1.Panel2.Controls.Contains(previewer))
                {
                    string[] exts = new string[] { ".jpg", ".png", ".bmp" };
                    if (exts.Contains(x.Extension))
                    {

                        previewer.SetImage(Bitmap.FromFile(x.FullName));
                    }
                }
            };
            Stuff.LoadSettings(fileListControl1, fileListControl2);
            Stuff.IsDirty = false;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FileListControl fc = null;
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

                }

            }

            if (e.KeyCode == Keys.Delete)
            {
                FileListControl fc = null;
                if (fileListControl1.ContainsFocus)
                {
                    fc = fileListControl1;
                }
                if (fileListControl2.ContainsFocus)
                {
                    fc = fileListControl2;
                }

                if (fc != null && fc.ListView.Focused)
                {
                    if (MessageBox.Show("Delete file: " + fc.SelectedFile.FullName + "?", Text,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        File.Delete(fc.SelectedFile.FullName);
                    }
                }
            }

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
            if (e.KeyCode == Keys.F5)
            {//copy

                if (fileListControl1.Mode == ViewModeEnum.Filesystem && fileListControl2.Mode == ViewModeEnum.Filesystem)
                {
                    if (fileListControl1.SelectedFile != null)
                    {
                        bool allow = true;
                        var p1 = Path.Combine(fileListControl2.CurrentDirectory.FullName,
                            fileListControl1.SelectedFile.Name);
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
                            File.Copy(fileListControl1.SelectedFile.FullName, p1, true);
                            fileListControl2.UpdateList(fileListControl2.CurrentDirectory.FullName);
                        }
                    }
                }
                if (fileListControl1.Mode == ViewModeEnum.Filesystem && fileListControl2.Mode == ViewModeEnum.Tags)
                {
                    if (fileListControl1.SelectedFile != null && fileListControl2.CurrentTag != null)
                    {
                        var fn = fileListControl1.SelectedFile.FullName;
                        if (!fileListControl2.CurrentTag.Files.Contains(fn))
                        {
                            fileListControl2.CurrentTag.Files.Add(fn);
                            MessageBox.Show(Path.GetFileName(fn) + " tagged as " + fileListControl2.CurrentTag.Name);
                            Stuff.IsDirty = true;
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
                tsf.FileListControl = fileListControl2;
                tsf.SetPath(fileListControl2.CurrentDirectory.FullName);
            }
            else
            {
                tsf.FileListControl = fileListControl1;
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
            RemoveSecondTab();
            splitContainer1.Panel2.Controls.Add(fileListControl2);

        }

        void RemoveSecondTab()
        {
            splitContainer1.Panel2.Controls.Clear();
        }

        ImgViewerPanel previewer;
        TextPreviewer textPreviewer = new TextPreviewer() { Dock = DockStyle.Fill };
        private void TablePreviewerToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
        }

        private void TableTextEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {

            RemoveSecondTab();
            splitContainer1.Panel2.Controls.Add(textPreviewer);
        }

        private void Explorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Stuff.IsDirty)
            {
                var res = MessageBox.Show("Save changes (tabs, libraries etc.)?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (res)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.Yes:
                        Stuff.SaveSettings(fileListControl1, fileListControl2);
                        break;

                }
            }
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
    }
}
