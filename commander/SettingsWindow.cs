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
            textBox1.Text = Stuff.GitBashPath;
            textBox2.Text = Stuff.VsCmdBatPath;
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

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Stuff.GitBashPath = textBox1.Text;
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            Stuff.VsCmdBatPath = textBox2.Text;
        }
    }
}
