using System;
using System.Windows.Forms;

namespace commander.Controls.MetaInfo
{
    public partial class KeywordsMetaInfoControl : UserControl
    {
        public KeywordsMetaInfoControl()
        {
            InitializeComponent();
        }

        KeywordsMetaInfo Info;
        public void Init(KeywordsMetaInfo k)
        {
            Info = k;
            textBox1.Text = Info.Keywords;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Info.Keywords = textBox1.Text;            
        }
    }
}
