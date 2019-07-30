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
    public partial class MountListWindow : Form
    {
        public MountListWindow()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
            UpdateList();
            
        }

        public void UpdateList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            listView1.SmallImageList = Stuff.list;
            listView1.LargeImageList = Stuff.list;
            foreach (var item in Stuff.MountInfos)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.IsoPath.FullName, item.FullPath }) { Tag = item, ImageIndex = 1 });
            }
            listView1.EndUpdate();
        }

        private void MountToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void UnmountToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
