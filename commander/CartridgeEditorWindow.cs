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
    public partial class CartridgeEditorWindow : Form
    {
        public CartridgeEditorWindow()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.iso|*.iso";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            
        }
    }
}
