using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace commander
{
    public partial class TextPreviewer : UserControl
    {
        public TextPreviewer()
        {
            InitializeComponent();
            richTextBox1.Font = new Font("Consolas", 11);
        }

        internal void LoadFile(FileInfo x)
        {
            richTextBox1.Enabled = true;
            richTextBox1.Text = File.ReadAllText(x.FullName);
            path = x.FullName;
        }

        string path;

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Save file " + path + "?", mdi.MainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                File.WriteAllText(path, richTextBox1.Text);
            }
        }

        internal void Disable()
        {
            richTextBox1.Enabled = false;
        }
    }
}
