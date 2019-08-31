using System;
using System.Windows.Forms;

namespace commander
{
    public partial class RenameDialog : Form
    {
        public RenameDialog()
        {
            InitializeComponent();
        }


        public string Value
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public Func<string, Tuple<bool,string>> Validation;
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.KeyCode == Keys.Enter)
            {
                if (Validation != null)
                {
                    var res = Validation(textBox1.Text);
                    if (res.Item1)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(res.Item2))
                        {
                            Stuff.Error(res.Item2);
                        }
                        else
                        {
                            Stuff.Error("Invalid name.");
                        }
                    }
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
