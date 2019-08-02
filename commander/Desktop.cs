using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class Desktop : Form
    {
        public Desktop()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            MinimumSize = new Size(100, SystemInformation.CaptionHeight+20);
            
            SizeChanged += Desktop_SizeChanged;
        }

        private void Desktop_SizeChanged(object sender, EventArgs e)
        {          
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
        }

        Bitmap bmp;
        Graphics gr;

        private void timer1_Tick(object sender, EventArgs e)
        {
            gr.Clear(Color.White);
            pictureBox1.Image = bmp;
        }
    }
}
