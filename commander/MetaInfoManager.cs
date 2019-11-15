using commander.Controls.MetaInfo;
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
    public partial class MetaInfoManager : Form
    {
        public MetaInfoManager()
        {
            InitializeComponent();
            UpdateList();
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in Stuff.MetaInfos)
            {
                listView1.Items.Add(new ListViewItem(item.File.FullName) { Tag = item });
            }
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var f = listView1.SelectedItems[0].Tag as FileMetaInfo;
            listView3.Items.Clear();
            foreach (var item in f.Infos)
            {                
                listView3.Items.Add(new ListViewItem(FileMetaInfoEditorDialog.GetName(item)) { Tag = item });
            }
        }

        private void ListView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            var f = listView3.SelectedItems[0].Tag as MetaInfo;
            groupBox1.Controls.Clear();
            var c = FileMetaInfoEditorDialog.GetControl(f);
            (c as IMetaInfoEditorControl).ValueChanged += MetaInfoManager_ValueChanged;
            groupBox1.Controls.Add(c);
        }

        private void MetaInfoManager_ValueChanged()
        {
            Stuff.IsDirty = true;
        }
    }
}
