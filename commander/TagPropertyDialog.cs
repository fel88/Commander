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
    public partial class TagPropertyDialog : Form
    {
        public TagPropertyDialog()
        {
            InitializeComponent();
        }

        TagInfo TagInfo;
        internal void Init(TagInfo ti)
        {
            TagInfo = ti;
            Text = ti.Name + ": properties";
            listBox1.Items.Clear();
            foreach (var item in ti.Synonyms)
            {
                listBox1.Items.Add(item);
            }
        }

        public bool Changed = false;
        private void SetAsMainTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndices.Count == 0) return;
            var a = listBox1.Items[listBox1.SelectedIndices[0]] as string;
            var t = TagInfo.Name;
            TagInfo.Name = a;
            TagInfo.Synonyms.Remove(a);
            TagInfo.Synonyms.Add(t);
            Stuff.IsDirty = true;
            Init(TagInfo);
            Changed = true;
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
           
        }
    }
}
