using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class CartridgeEditorWindow : Form
    {
        public CartridgeEditorWindow()
        {
            InitializeComponent();
        }



        private void Button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.zip|*.zip";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            using (var zip = ZipFile.Open(sfd.FileName, ZipArchiveMode.Create))
            {
                if (checkBox1.Checked)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\"?>");
                    sb.AppendLine("<root>");
                    Stuff.GetBookmarks(sb);
                    sb.AppendLine("</root>");
                    var ent1 = zip.CreateEntry("bookmarks.xml");
                    using (var strm = ent1.Open())
                    {
                        using (var wrt = new StreamWriter(strm))
                        {
                            wrt.Write(sb.ToString());
                        }
                    }
                }
            }
            Stuff.Info("Export complete!");
            Close();
        }
      
    }
}
