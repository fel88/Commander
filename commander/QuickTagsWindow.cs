using System;
using System.Windows.Forms;

namespace commander
{
    public partial class QuickTagsWindow : Form
    {
        public QuickTagsWindow()
        {
            InitializeComponent();
            u = new QuickTagsUserControl() { Dock = DockStyle.Fill };
            Controls.Add(u);
            Shown += QuickTagsWindow_Shown;
        }

        private void QuickTagsWindow_Shown(object sender, EventArgs e)
        {
            u.Init();
        }        

        QuickTagsUserControl u;
        public void Init(IFileListControl flc, IFileInfo file)
        {
            u.Init(flc,file);            
        }
    }
}
