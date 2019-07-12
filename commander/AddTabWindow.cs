using System;
using System.Drawing;
using System.Windows.Forms;

namespace commander
{
    public partial class AddTabWindow : Form
    {
        public AddTabWindow()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        public string Path
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public string Hint
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
        public string Filter
        {
            get { return textBox3.Text; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Hint))
            {
                textBox2.BackColor = Color.Red;
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.BackColor = Color.White;
        }
    }
}
