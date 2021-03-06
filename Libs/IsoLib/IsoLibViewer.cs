﻿using IsoLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace isoViewer
{
    public partial class IsoLibViewer : Form
    {
        public IsoLibViewer()
        {
            InitializeComponent();
            SetDoubleBuffered(listView1);

        }
        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private PVD pvdd;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "iso|*.iso";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        IsoReader reader = new IsoReader();
                        reader.Parse(fs);
                        pvdd = reader.Pvds[0];
                        
                        UpdateList(pvdd.RootDir);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Access error.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        public void UpdateList(DirectoryRecord record)
        {
            listView1.Items.Clear();
            if (record.Parent != null)
            {
                listView1.Items.Add(new ListViewItem(new string[] { ".." })
                { Tag = record.Parent, BackColor = Color.LightBlue });
            }
            foreach (var directoryRecord in record.Records.OrderByDescending(z => z.IsDirectory))
            {
                if (directoryRecord.LBA == record.LBA) continue;
                if (record.Parent != null && directoryRecord.LBA == record.Parent.LBA) continue;

                if (string.IsNullOrEmpty(directoryRecord.Name)) continue;
                ListViewItem lvi = null;
                if (directoryRecord.IsDirectory)
                {
                    lvi = new ListViewItem(
                        new string[] { directoryRecord.Name })
                    { Tag = directoryRecord };
                }
                else
                {
                    lvi = new ListViewItem(
                        new string[] { directoryRecord.Name, directoryRecord.DataLength / 1024 + "Kb" })
                    { Tag = directoryRecord };
                }


                listView1.Items.Add(lvi);
                if (directoryRecord.IsDirectory)
                {
                    lvi.BackColor = Color.LightBlue;
                }
            }

        }


        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var rec = listView1.SelectedItems[0].Tag as DirectoryRecord;
                if (rec.IsDirectory)
                {

                    //rec.ReadRecords(fs, pvdd);
                    UpdateList(rec);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var rec = listView1.SelectedItems[0].Tag as DirectoryRecord;
                if (rec.IsFile)
                {
                    if (rec.Data != null && rec.Data.Length < 1024 * 10)
                    {
                        richTextBox1.Text = Encoding.UTF8.GetString(rec.Data);
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.BackColor = Color.LightPink;
                return;
            }

            if (!Directory.Exists(textBox1.Text))
            {
                textBox1.BackColor = Color.LightPink;
                return;
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                textBox2.BackColor = Color.LightPink;
                return;
            }
            var dirs = GetAllDirs(new DirectoryInfo(textBox1.Text));
            throw new NotImplementedException();
            WriteIso(textBox2.Text, dirs.ToArray());
            //generate pvd and records. 
        }

        public void WriteIso(string fileName, DirectoryInfo[] dirs)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                byte[] bb = new byte[0x8000];
                fs.Write(bb, 0, bb.Length);
                PVD pvd = new PVD();
                pvd.LogicBlockSize = 0x800;
                pvd.Save(fs);
            }
        }


        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var rec = listView1.SelectedItems[0].Tag as DirectoryRecord;
                var nm = rec.Name.Split(';')[0];
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = nm;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, rec.Data);
                }
            }
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            if (pvdd == null) { MessageBox.Show("load image first"); return; }
            string dummyFileName = "Save Here";

            SaveFileDialog sf = new SaveFileDialog();

            sf.FileName = dummyFileName;

            if (sf.ShowDialog() == DialogResult.OK)
            {

                string savePath = Path.GetDirectoryName(sf.FileName);
                var ret = DirectoryRecord.GetAllRecords(pvdd.RootDir);

                foreach (var directoryRecord in ret)
                {
                    throw new NotImplementedException();
                    //get path ,create dirs, save files
                    //var pp=Path.Combine   (path,)
                }

            }

        }

        public List<DirectoryInfo> GetAllDirs(DirectoryInfo dir, List<DirectoryInfo> dirs = null)
        {
            if (dirs == null)
            {
                dirs = new List<DirectoryInfo>();
            }
            dirs.Add(dir);
            foreach (var directoryInfo in dir.GetDirectories())
            {
                GetAllDirs(directoryInfo, dirs);
            }
            return dirs;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string dummyFileName = "Load Here";

            SaveFileDialog sf = new SaveFileDialog();

            sf.FileName = dummyFileName;

            if (sf.ShowDialog() == DialogResult.OK)
            {
                string savePath = Path.GetDirectoryName(sf.FileName);

                textBox1.Text = savePath;


            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.White;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.BackColor = Color.White;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "iso|*.iso";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = sf.FileName;
            }
        }
    }
}
