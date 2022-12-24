using PluginLib;
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
            Text = $"Tags: {file.Name}";
            u.Init(flc,file);
            flc.SelectedFileChanged += Flc_SelectedFileChanged; ;
        }

        private void Flc_SelectedFileChanged(IFileInfo file)
        {
            Text = $"Tags: {file.Name}";
        }
    }
}
