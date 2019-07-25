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
    public partial class SettingsWindow : Form
    {
        public SettingsWindow()
        {
            InitializeComponent();
            checkBox1.Checked = FileListControl.AllowHints;
            switch (FileListControl.HintMode)
            {
                case FileListControl.HintModeEnum.Image:
                    comboBox1.SelectedIndex = 0;
                    break;
                case FileListControl.HintModeEnum.Tags:
                    comboBox1.SelectedIndex = 1;
                    break;
            }

            foreach (var item in Stuff.FileContextMenuItems)
            {
                AppendMenuItem(item);
            }
        }

        public void AppendMenuItem(FileContextMenuItem item)
        {
            listView1.Items.Add(new ListViewItem(new string[] { item.Title, item.AppName, item.Arguments }) { Tag = item });
        }

        public static string VsCmdPath;
        public static string GitBashPath;

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            FileListControl.AllowHints = checkBox1.Checked;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) { FileListControl.HintMode = FileListControl.HintModeEnum.Image; }
            if (comboBox1.SelectedIndex == 1) { FileListControl.HintMode = FileListControl.HintModeEnum.Tags; }
        }

        private void AddItemToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var f = new FileContextMenuItem() { Title="new item",AppName="cmd.exe"};            
            Stuff.FileContextMenuItems.Add(f);
            AppendMenuItem(f);
            Stuff.IsDirty = true;

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var f = listView1.SelectedItems[0].Tag as FileContextMenuItem;
                Stuff.FileContextMenuItems.Remove(f);
                Stuff.IsDirty = true;
                listView1.Items.Remove(listView1.SelectedItems[0]);
            }
        }
    }
}
