using System.Windows.Forms;

namespace commander
{
    public partial class ConsoleOutputWindow : Form
    {
        public ConsoleOutputWindow()
        {
            InitializeComponent();
        }

        public void SetText(string str)
        {
            richTextBox1.Text = str;
        }
    }
}
