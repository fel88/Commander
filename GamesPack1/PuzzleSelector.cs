using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GamesPack1
{
    public partial class PuzzleSelector : Form
    {
        public PuzzleSelector()
        {
            InitializeComponent();
            if (GamePack1Plugin.Container == null)
            {
                button1.Enabled = false;
            }
            else
            {
                if (GamePack1Plugin.Container.Libraries.Count() == 0)
                {
                    button1.Enabled = false;
                }
            }

            foreach (var item in GamePack1Plugin.Container.Libraries.OrderBy(z => z.Name))
            {
                comboBox1.Items.Add(new ComboBoxItem() { Name = item.Name, Tag = item });
            }

            foreach (var item in GamePack1Plugin.Container.Tags.OrderBy(z => z.Name))
            {
                if (item.IsHidden && !GamePack1Plugin.Container.ShowHiddenTags) continue;
                comboBox2.Items.Add(new ComboBoxItem() { Name = item.Name, Tag = item });
            }

            comboBox1.DropDownWidth = DropDownWidth(comboBox1);
            comboBox2.DropDownWidth = DropDownWidth(comboBox2);
        }
        int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0;
            int temp = 0;
            Label label1 = new Label();

            foreach (var obj in myCombo.Items)
            {
                label1.Text = obj.ToString();
                temp = label1.PreferredWidth;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            label1.Dispose();
            return maxWidth;
        }

        public string FileName;

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
        Random r = new Random();

        public string[] Images;

        private void Button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (comboBox1.SelectedItem == null)
                {
                    Warning("Select source.");
                    return;
                }
                var item = (comboBox1.SelectedItem as ComboBoxItem).Tag as ILibrary;

                var cands = item.EnumerateFiles().Where(z => z.EndsWith(".jpg") || z.EndsWith(".png")).ToArray();

                if (!cands.Any()) { Error("There are no images!"); return; }

                var c = cands[r.Next(cands.Count())];
                FileName = c;
                DialogResult = DialogResult.OK;
                Images = cands;
                Close();

            }
            else
            {
                if (comboBox2.SelectedItem == null)
                {
                    Warning("Select source.");
                    return;
                }
                var item = (comboBox2.SelectedItem as ComboBoxItem).Tag as ITagInfo;

                var cands = item.Files.Where(z => z.FullName.EndsWith(".jpg") || z.FullName.EndsWith(".png")).ToArray();

                if (!cands.Any()) { Error("There are no images!"); return; }

                var c = cands[r.Next(cands.Count())];
                FileName = c.FullName;
                DialogResult = DialogResult.OK;
                Images = cands.Select(z => z.FullName).ToArray();
                Close();
            }
        }

        public DialogResult Warning(string v)
        {
            return MessageBox.Show(v, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public DialogResult Info(string v)
        {
            return MessageBox.Show(v, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public DialogResult Error(string v)
        {
            return MessageBox.Show(v, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
