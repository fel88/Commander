using System;
using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public partial class LibraryPropertiesDialog : Form
    {
        public LibraryPropertiesDialog()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        public void Init(ILibrary library)
        {
            Text = library.Name + ": properties";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
