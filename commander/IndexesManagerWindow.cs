using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class IndexesManagerWindow : Form
    {
        public IndexesManagerWindow()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
            UpdateList();
            Stuff.IndexAdded += Stuff_IndexAdded;
        }

        private void Stuff_IndexAdded(IndexInfo obj)
        {
            AppendItem(obj);
        }

        void AppendItem(IndexInfo item)
        {
            listView1.Items.Add(new ListViewItem(new string[] { item.Path, item.Text.Length + " symbols" }) { Tag = item });
        }
        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in Stuff.Indexes)
            {
                AppendItem(item);
            }
        }

        private void ToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var ii = listView1.SelectedItems[0].Tag as IndexInfo;
            Clipboard.SetText(ii.Text);            
                    
        }
    }
}
