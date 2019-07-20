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
    public partial class ChangePasswordDialog : Form
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            if (textBox2.Text != textBox3.Text)
            {
                MessageBox.Show("Password and repeat not equals", mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Stuff.PasswordHash.ToLower() != Stuff.CreateMD5(textBox1.Text).ToLower())
            {
                MessageBox.Show("Password wrong", mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Stuff.PasswordHash = Stuff.CreateMD5(textBox2.Text);
            Stuff.IsDirty = true;
            MessageBox.Show("Password changed!", mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
