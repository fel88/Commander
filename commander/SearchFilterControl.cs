using PluginLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace commander
{
    public partial class SearchFilterControl : UserControl
    {
        public SearchFilterControl()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
            comboBox1.SelectedIndex = 0;
        }

        bool expanded = false;

        public FileListControl ParentFileListControl { get; set; }

        public void UpdatePosition()
        {
            if (!expanded)
            {
                Height = (int)tableLayoutPanel1.RowStyles[0].Height;
                Width = 170;
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

                Left = Parent.ClientRectangle.Width - Width;
                Top = Parent.ClientRectangle.Height - Height;
                //if (listView1.ClientRectangle.)
                {
                    //  Top -= SystemInformation.HorizontalScrollBarHeight;
                }
                Dock = DockStyle.None;
            }
        }

        internal void Init(FileListControl flc)
        {
            ParentFileListControl = flc;
            ParentFileListControl.ExFilters = true;
            ParentFileListControl.ExFileFilter = filter;
            ParentFileListControl.ExDirFilter = filter;
        }

        public List<FilterItem> Filters = new List<FilterItem>();
        bool filter(IFileInfo fin)
        {
            var str = fin.Name;
            str = str.ToLower();
            foreach (var item in Filters.Where(z => z.Target == FilterItemTargetEnum.All || z.Target == FilterItemTargetEnum.Files))
            {
                if (item.IsExclude)
                {
                    if (str.Contains(item.Mask)) return false;
                }
                else
                {
                    if (!str.Contains(item.Mask)) return false;
                }
            }
            return true;
        }
        bool filter(IDirectoryInfo din)
        {
            var str = din.Name;
            str = str.ToLower();
            foreach (var item in Filters.Where(z => z.Target == FilterItemTargetEnum.All || z.Target == FilterItemTargetEnum.Dirs))
            {
                if (item.IsExclude)
                {
                    if (str.Contains(item.Mask)) return false;
                }
                else
                {
                    if (!str.Contains(item.Mask)) return false;
                }
            }
            return true;
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            if (expanded)
            {
                Height = (int)tableLayoutPanel1.RowStyles[0].Height;
                Width = 170;
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

                Left = Parent.ClientRectangle.Width - Width;
                Top = Parent.ClientRectangle.Height - Height;
                Dock = DockStyle.None;
            }
            else
            {
                Height = (int)(Parent.Height * 0.4f);
                Dock = DockStyle.Bottom;
            }
            expanded = !expanded;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Filters.Add(new FilterItem() { Mask = textBox1.Text, IsExclude = checkBox1.Checked, Target = (FilterItemTargetEnum)comboBox1.SelectedIndex });
            textBox1.Text = string.Empty;
            UpdateList();
            ParentFileListControl.UpdateList();
        }
        public void UpdateList()
        {
            label1.Text = "Search filters";
            if (Filters.Any())
            {
                label1.Text += $" ({Filters.Count})";
            }
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var item in Filters)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Mask, item.IsExclude + "", item.Target.ToString() }) { Tag = item });
            }
            listView1.EndUpdate();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var f = listView1.SelectedItems[0].Tag as FilterItem;
            Filters.Remove(f);
            UpdateList();
            ParentFileListControl.UpdateList();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Filters.Clear();
            UpdateList();
            ParentFileListControl.UpdateList();
        }
    }

    public class FilterItem
    {
        public string Mask;
        public bool IsExclude;
        public FilterItemTargetEnum Target;
    }

    public enum FilterItemTargetEnum
    {
        Files, Dirs, All
    }
}
