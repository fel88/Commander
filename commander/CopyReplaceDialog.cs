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
    public partial class CopyReplaceDialog : Form
    {
        public CopyReplaceDialog()
        {
            InitializeComponent();            
        }

        
        public CopyReplaceDialogResultEnum Result;
        private void Button1_Click(object sender, EventArgs e)
        {
            Result = CopyReplaceDialogResultEnum.Replace;
            Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Result = CopyReplaceDialogResultEnum.Skip;
            Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Result = CopyReplaceDialogResultEnum.ReplaceAll;
            Close();
        }

        internal void Init(string target)
        {
            label1.Text = "File " + target + " already exist. Overwrite?";
            Width = label1.Width + label1.Left + 40;
        }

        private void CopyReplaceDialog_Load(object sender, EventArgs e)
        {

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Result = CopyReplaceDialogResultEnum.SkipAll;
            Close();
        }
    }

    public enum CopyReplaceDialogResultEnum
    {
        Replace, Skip, ReplaceAll, SkipAll
    }
}
