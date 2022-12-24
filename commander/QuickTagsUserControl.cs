using PluginLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace commander
{
    public partial class QuickTagsUserControl : UserControl
    {
        public QuickTagsUserControl()
        {
            InitializeComponent();
            Stuff.TagsListChanged += Stuff_TagsListChanged;
            Stuff.SetDoubleBuffered(checkedListBox1);
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
            UpdateCheckList();
        }

        public void Init()
        {
            watermark1.Init();
        }
        void UpdateCheckList()
        {
            checkedListBox1.Items.Clear();
            var arr = watermark1.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();
            List<TagInfoCover> covers = new List<TagInfoCover>();
            foreach (var item in Stuff.Tags)
            {
                covers.Add(new TagInfoCover() { IsMain = true, TagInfo = item, Name = item.Name });
                foreach (var sitem in item.Synonyms)
                {
                    covers.Add(new TagInfoCover() { TagInfo = item, Name = sitem });
                }
            }
            foreach (var item in covers.OrderBy(z => z.Name))
            {
                //if (item.TagInfo.IsHidden && !Stuff.ShowHidden) continue;
                if (arr.Any())
                {
                    if (!arr.Any(z => item.Name.ToLower().Contains(z))) continue;
                }
                checkedListBox1.Items.Add(new ComboBoxItem() { Tag = item.TagInfo, Name = item.Name });

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
                tagg.AddFile(currentFile);
            }
            if (e.NewValue == CheckState.Unchecked)
            {
                tagg.DeleteFile(currentFile.FullName);
            }
            if (Stuff.AllowNTFSStreamsSync)
                Stuff.UpdateFileMetaInfo(currentFile);
            UpdateTagsInfo(currentFile);
        }

        IFileInfo currentFile;

        public void Init(IFileListControl flc, IFileInfo file)
        {
            FileControl = flc;
            if (file != null)
            {
                currentFile = file;
                UpdateTags(file);
            }
            else
            {
                checkedListBox1.Enabled = false;
            }


            flc.SelectedFileChanged += Flc_SelectedFileChanged;
        }

        void UpdateTagsInfo(IFileInfo f)
        {
            if (f == null)
            {
                checkedListBox1.Enabled = false;
                return;
            }
            checkedListBox1.Enabled = true;
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
            if (f == null)
            {
                checkedListBox1.Enabled = false;
                return;
            }
            checkedListBox1.Enabled = true;
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

        private void Watermark1_TextChanged(object sender, EventArgs e)
        {
            UpdateCheckList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(watermark1.Text))
                return;

            Stuff.AddTag(new TagInfo() { Name = watermark1.Text }, !Stuff.AllowNTFSStreamsSync);
        }
    }
}
