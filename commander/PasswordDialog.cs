using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class PasswordDialog : Form
    {
        public PasswordDialog()
        {
            InitializeComponent();
            DialogResult = DialogResult.No;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PasswordDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                e.Cancel = true;
                return;
            }

            var hash = Stuff.CreateMD5(textBox1.Text);           

            if (Stuff.PasswordHash.ToLower() == hash.ToLower())
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Password wrong!", mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
