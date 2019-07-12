using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            var drvs = System.IO.DriveInfo.GetDrives();
            fileListControl1.UpdateList(drvs[0].Name, "");
            fileListControl2.UpdateList(drvs[0].Name, "");
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = XDocument.Load("settings.xml");
            foreach (var descendant in s.Descendants("path"))
            {
                RecentPathes.Add(descendant.Value);
            }

            foreach (var descendant in s.Descendants("tab"))
            {
                var hint = descendant.Attribute("hint").Value;
                var owner = descendant.Attribute("owner").Value;
                var path = descendant.Attribute("path").Value;
                var filter = descendant.Attribute("filter").Value;
                FileListControl fc = fileListControl1;
                if (owner == "right")
                {
                    fc = fileListControl2;
                }
                fc.AddTab(new TabInfo() { Filter = filter, Path = path, Hint = hint });

            }


        }

        public List<string> RecentPathes = new List<string>();

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

                if (fc != null && fc.ListView.Focused)
                {
                    if (fc.SelectedFile != null)
                    {

                        RenameDialog rd = new RenameDialog();
                        rd.Value = fc.SelectedFile.Name;
                        if (rd.ShowDialog() == DialogResult.OK)
                        {
                            File.Move(fc.SelectedFile.FullName, Path.Combine(fc.SelectedFile.Directory.FullName, rd.Value));
                        }
                        fc.UpdateList(fc.CurrentDirectory.FullName);
                    }
                    else if (fc.SelectedDirectory != null)
                    {
                        RenameDialog rd = new RenameDialog();
                        rd.Value = fc.SelectedDirectory.Name;
                        if (rd.ShowDialog() == DialogResult.OK)
                        {
                            Directory.Move(fc.SelectedDirectory.FullName, Path.Combine(fc.SelectedDirectory.Parent.FullName, rd.Value));
                        }
                        fc.UpdateList(fc.CurrentDirectory.FullName);

                    }
                }
            }
            if (e.KeyCode == Keys.F5)
            {//copy
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
            base.OnKeyDown(e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
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


        public void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<settings>");
            foreach (var item in fileListControl1.Tabs)
            {
                sb.AppendLine($"<tab hint=\"{item.Hint}\" owner=\"left\" path=\"{item.Path}\" filter=\"{item.Filter}\"/>");
            }
            foreach (var item in fileListControl2.Tabs)
            {
                sb.AppendLine($"<tab hint=\"{item.Hint}\" owner=\"right\" path=\"{item.Path}\" filter=\"{item.Filter}\"/>");
            }
            sb.AppendLine("</settings>");
            File.WriteAllText("settings.xml", sb.ToString());
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //save ui and all tabs to settings config
            if (MessageBox.Show("Replace settings?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SaveSettings();
            }

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
    }
}
