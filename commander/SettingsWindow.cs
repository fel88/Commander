﻿using System;
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
            Stuff.SetDoubleBuffered(listView1);
            Stuff.SetDoubleBuffered(listView2);
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
            var h = e.Item.Tag as HotkeyShortcutInfo;
            if (h.IsEnabled == e.Item.Checked) return;
            h.IsEnabled = e.Item.Checked;
            e.Item.SubItems[0].Text = h.IsEnabled + "";
            Stuff.IsDirty = true;
        }
    }
}
