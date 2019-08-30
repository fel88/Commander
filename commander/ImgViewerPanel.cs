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
        public void ResetImage()
        {
            Image temp = null;
            if (pictureBox1.Image != null)
            {
                temp = pictureBox1.Image;
            }
            pictureBox1.Image = null;
            CurrentFile = null;
            if (temp != null)
            {
                temp.Dispose();
            }
        }
        internal void SetImage(Image image)
        {
            pictureBox1.Image = image;
            CurrentFile= null;
        }
        public IFileInfo CurrentFile;
        internal void SetImage(IFileInfo fl)
        {
            Image temp = null;
            if (pictureBox1.Image != null)
            {
                temp = pictureBox1.Image;
            }
            CurrentFile = fl;
            pictureBox1.Image = fl.Filesystem.BitmapFromFile(fl);
            if (temp != null)
            {
                temp.Dispose();
            }
        }
        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            mdi.MainForm.OpenWindow(this);
        }
    }
}
