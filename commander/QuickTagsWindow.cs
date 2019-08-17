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
    public partial class QuickTagsWindow : Form
    {
        public QuickTagsWindow()
        {
            InitializeComponent();
            Stuff.TagsListChanged += Stuff_TagsListChanged;
            Stuff.SetDoubleBuffered(checkedListBox1);
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
            UpdateCheckList();
        }

        void UpdateCheckList()
        {
            checkedListBox1.Items.Clear();
            foreach (var item in Stuff.Tags.OrderBy(z => z.Name))
            {
                if (item.IsHidden && !Stuff.ShowHidden) continue;

                checkedListBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.Name });

            }
        }
        private void Stuff_TagsListChanged()
        {
            UpdateCheckList();
            UpdateTags();
        }

        bool allow = true;
        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!allow) return;
            var t = checkedListBox1.Items[e.Index] as ComboBoxItem;
            var tagg = t.Tag as TagInfo;
            if (e.NewValue == CheckState.Checked)
            {
                tagg.AddFile(currentFile.FullName);
            }
            if (e.NewValue == CheckState.Unchecked)
            {
                tagg.DeleteFile(currentFile.FullName);
            }
            UpdateTagsInfo(currentFile);
        }

        IFileInfo currentFile;

        public void Init(IFileListControl flc, IFileInfo file)
        {
            FileControl = flc;
            currentFile = file;
            UpdateTags(file);


            flc.SelectedFileChanged += Flc_SelectedFileChanged;
        }

        void UpdateTagsInfo(IFileInfo f)
        {
            listView1.Items.Clear();
            var tt = Stuff.Tags.Where(z => z.ContainsFile(f.FullName));
            foreach (var item in tt)
            {
                listView1.Items.Add(item.Name);
            }
        }

        public void UpdateTags()
        {
            UpdateTags(currentFile);
        }

        public void UpdateTags(IFileInfo f)
        {

            UpdateTagsInfo(f);
            allow = false;
            Text = "Tags of: " + f.Name;
            var tt = Stuff.Tags.Where(z => z.ContainsFile(f.FullName));
            
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                var item = checkedListBox1.Items[i] as ComboBoxItem;
                var tagg = item.Tag as TagInfo;
                if (tt.Contains(tagg))
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
                else
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
            allow = true;
        }
        private void Flc_SelectedFileChanged(IFileInfo obj)
        {
            currentFile = obj;
            
            UpdateTags(obj);
            
          
        }

        public IFileListControl FileControl;

        private void CheckedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
