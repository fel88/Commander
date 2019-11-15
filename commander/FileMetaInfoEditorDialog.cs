using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using commander.Controls.MetaInfo;
using PluginLib;

namespace commander
{
    public partial class FileMetaInfoEditorDialog : Form
    {
        public FileMetaInfoEditorDialog()
        {
            InitializeComponent();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {

        }

        IFileInfo FileInfo;
        internal void Init(IFileInfo fi)
        {
            FileInfo = fi;
            Text = fi.Name + ": meta info editor";
            UpdateList();
        }


        public void UpdateList()
        {
            listView1.Items.Clear();
            var m = Stuff.GetMetaInfoOfFile(FileInfo);
            if (m == null) return;
            foreach (var item in m.Infos)
            {
                listView1.Items.Add(new ListViewItem(GetName(item)) { Tag = item });
            }
        }

        public static string GetName(MetaInfo tp)
        {
            if (tp is KeywordsMetaInfo) return "keywords";
            throw new NotImplementedException();
        }
        public static Control GetControl(MetaInfo tp)
        {
            if (tp is KeywordsMetaInfo)
            {
                var r = new KeywordsMetaInfoControl();
                r.Init(tp as KeywordsMetaInfo);
                r.Dock = DockStyle.Fill;
                return r;
            }
            throw new NotImplementedException();
        }
        private void KeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var minf = Stuff.GetMetaInfoOfFile(FileInfo);
            if (minf == null)
            {                
                Stuff.MetaInfos.Add(new FileMetaInfo() { File = FileInfo });
                minf = Stuff.MetaInfos.Last();
            }

            if (minf.Infos.Any(z => z is KeywordsMetaInfo)) return;
            minf.Infos.Add(new KeywordsMetaInfo() { Parent = minf });
            Stuff.IsDirty = true;
            UpdateList();
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var m = listView1.SelectedItems[0].Tag as MetaInfo;
            groupBox1.Controls.Clear();
            var c = GetControl(m);
            (c as IMetaInfoEditorControl).ValueChanged += FileMetaInfoEditorDialog_ValueChanged;
            groupBox1.Controls.Add(c);
        }

        private void FileMetaInfoEditorDialog_ValueChanged()
        {
            Stuff.IsDirty = true;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
