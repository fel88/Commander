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
using System.IO.Compression;
using System.Xml.Linq;

namespace commander
{
    public partial class OdtPreviewer : UserControl
    {
        public OdtPreviewer()
        {
            InitializeComponent();
            richTextBox1.Font = new Font("Consolas", 11);
        }

        IFileInfo FileInfo;
        internal void LoadFile(IFileInfo x)
        {
            richTextBox1.Enabled = true;
            var txt = ExtractContent(x);
            
            richTextBox1.Text = txt;
            path = x.FullName;
            FileInfo = x;
        }

        string ExtractContent(IFileInfo f)
        {
            StringBuilder sb = new StringBuilder();

            using (var file = f.Filesystem.OpenReadOnlyStream(f))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name == "content.xml")
                    {
                        using (var stream = entry.Open())
                        {
                            var doc = XDocument.Load(stream);
                            foreach (var item in doc.Descendants().Where(z => z.Name.LocalName == "text"))
                            {
                                sb.Append(item.Value);
                            }

                        }
                    }
                }
            }
            return sb.ToString();
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
