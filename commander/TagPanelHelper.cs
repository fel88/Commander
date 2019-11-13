using PluginLib;
using System;
using System.Windows.Forms;

namespace commander
{
    public partial class TagPanelHelper : UserControl
    {
        public TagPanelHelper()
        {
            InitializeComponent();
        }

        public void Init()
        {
            quickTagsUserControl1.Init();
        }

        internal void Init(IFileListControl fileListControl, IFileInfo p)
        {
            quickTagsUserControl1.Init(fileListControl, p);
            
        }
        bool expanded = false;
        public void UpdatePosition()
        {
            if (!expanded)
            {
                Height = (int)tableLayoutPanel1.RowStyles[0].Height;
                Width = 170;
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                int shift = 0;
                for (int i = 0; i < Parent.Controls.Count; i++)
                {
                    if (Parent.Controls[i] == this) { break; }
                    if (!Parent.Controls[i].Visible) continue;
                    shift += Parent.Controls[i].Height;
                }
                
                Left = Parent.ClientRectangle.Width - Width;
                Top = Parent.ClientRectangle.Height - Height-shift;                
                Dock = DockStyle.None;
            }
            else
            {
                Width = (int)(Parent.Width * 0.3f);
                //Dock = DockStyle.Right;

                Height = Parent.ClientRectangle.Height - SystemInformation.CaptionHeight;
                Left = Parent.ClientRectangle.Width - Width;
                Top = Parent.ClientRectangle.Top + SystemInformation.CaptionHeight;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {           
            expanded = !expanded;            
            UpdatePosition();
        }
    }
}
