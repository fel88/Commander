using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using PluginLib;

namespace commander
{
    public partial class TextPreviewer : UserControl
    {
        public TextPreviewer()
        {
            InitializeComponent();
            richTextBox1.Font = new Font("Consolas", 11);
        }

        IFileInfo FileInfo;
        internal void LoadFile(IFileInfo x)
        {
            richTextBox1.Enabled = true;
            var txt = x.Filesystem.ReadAllText(x);
            richTextBox1.Text = txt;
            path = x.FullName;
            FileInfo = x;
        }

        string path;

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            if (FileInfo.Filesystem.IsReadOnly)
            {
                Stuff.Warning("Filesystem of file is readonly.");
                return;
            }
            if (MessageBox.Show("Save file " + path + "?", mdi.MainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FileInfo.Filesystem.WriteAllText(FileInfo, richTextBox1.Text);                
            }
        }

        internal void Disable()
        {
            richTextBox1.Enabled = false;
        }
    }
}
