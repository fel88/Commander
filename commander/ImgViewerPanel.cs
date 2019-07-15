using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class ImgViewerPanel : UserControl
    {
        public ImgViewerPanel()
        {
            InitializeComponent();
            
        }

        
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = (PictureBoxSizeMode)((((int)pictureBox1.SizeMode) + 1) % Enum.GetValues(typeof(PictureBoxSizeMode)).Length);
        }

        internal void SetImage(Image image)
        {
            pictureBox1.Image = image;
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            mdi.MainForm.OpenWindow(this);
        }
    }
}
