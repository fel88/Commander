using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            Stuff.SetDoubleBuffered(listView1);
            Stuff.SetDoubleBuffered(listView2);
            checkBox4.Checked = mdi.MainForm.TopMost;

            allowFire = false;
            textBox2.Text = Stuff.LibreOfficePath;
            checkBox2.Checked = Stuff.FiltersHelperVisible;
            checkBox3.Checked = Stuff.TagsHelperVisible;
            allowFire = true;

            checkBox1.Checked = PreviewHelper.AllowHints;
            switch (PreviewHelper.HintMode)
            {
                case PreviewHelper.HintModeEnum.Image:
                    comboBox1.SelectedIndex = 0;
                    break;
                case PreviewHelper.HintModeEnum.Tags:
                    comboBox1.SelectedIndex = 1;
                    break;
            }

            foreach (var item in Stuff.FileContextMenuItems)
            {
                AppendMenuItem(item);
            }

            UpdateHotkeys();
        }


        private void UpdateHotkeys()
        {
            listView2.Items.Clear();
            foreach (var item in Stuff.Hotkeys)
            {
                listView2.Items.Add(new ListViewItem(new string[] { item.IsEnabled + "", item.Path, item.Hotkey + "" }) { Tag = item, Checked = item.IsEnabled });
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
            PreviewHelper.AllowHints = checkBox1.Checked;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) { PreviewHelper.HintMode = PreviewHelper.HintModeEnum.Image; }
            if (comboBox1.SelectedIndex == 1) { PreviewHelper.HintMode = PreviewHelper.HintModeEnum.Tags; }
        }

        private void AddItemToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var f = new FileContextMenuItem() { Title = "new item", AppName = "cmd.exe" };
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

        private void ListView2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int cnt = 0;
            while (true)
            {
                if (!File.Exists($"settings{cnt}_backup.xml"))
                {
                    break;
                }
                cnt++;
            }
            var path = $"settings{cnt}_backup.xml";
            File.Copy("settings.xml", path);
            toolStripStatusLabel1.Text = "Backup saved to: " + path;
        }

        private void ListView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var item = listView2.Items[e.Index];
            var h = listView2.Items[e.Index].Tag as HotkeyShortcutInfo;
            //var h = e.Index.Tag as HotkeyShortcutInfo;
            var b = e.NewValue == CheckState.Checked;
            if (h.IsEnabled == b) return;
            h.IsEnabled = b;
            item.SubItems[0].Text = h.IsEnabled + "";
            Stuff.IsDirty = true;
        }

        bool allowFire = false;
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            Stuff.FiltersHelperVisible = checkBox2.Checked;
            Stuff.OnHelperVisibleChanged(HelperEnum.FiltersHelper);
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            Stuff.TagsHelperVisible = checkBox3.Checked;
            Stuff.OnHelperVisibleChanged(HelperEnum.TagsHelper);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml|*.xml";
            if (ofd.ShowDialog() != DialogResult.Yes) return;
            //show what sections we need to import: bookmarks/tabs/metainfo.. etc
            throw new NotImplementedException();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            mdi.MainForm.TopMost = checkBox4.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            Stuff.LibreOfficePath = textBox2.Text;
        }
    }

    public enum HelperEnum
    {
        TagsHelper, FiltersHelper
    }
}
