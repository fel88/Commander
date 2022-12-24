using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinet.Core.IO.Ntfs;

namespace commander.Forms
{
    public partial class NTFSStreamsEditor : Form
    {
        public NTFSStreamsEditor()
        {
            InitializeComponent();
        }

        IFileInfo _file;
        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in new FileInfo(_file.FullName).ListAlternateDataStreams())
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
            }
        }

        public void Init(IFileInfo file)
        {
            _file = file;
            UpdateList();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var ads = listView1.SelectedItems[0].Tag as AlternateDataStreamInfo;
            ads.Delete();
            UpdateList();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var ads = listView1.SelectedItems[0].Tag as AlternateDataStreamInfo;
            using (var s = ads.OpenText())
            {                
                richTextBox1.Text = s.ReadToEnd();
                toolStripStatusLabel1.Text = $"Stream size: {ads.Size} bytes";
            }
        }
    }
}
