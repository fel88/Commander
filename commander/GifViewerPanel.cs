using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public partial class GifViewerPanel : UserControl
    {
        public GifViewerPanel()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }

        GifLib.GifContainer container;
        internal void SetImage(IFileInfo file)
        {
            using (var stream = file.Filesystem.OpenReadOnlyStream(file))
            {
                container = GifLib.GifParser.Parse(stream);
                frame = 0;
            }
        }
        int frame = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (container == null) return;
            pictureBox1.Image = container.GetFrame(frame++);
            frame %= container.Frames;
            if (container.LastGceBlock.Delay == 0) { timer1.Interval = 15; }
            else
            {
                timer1.Interval = container.LastGceBlock.Delay;
            }
        }
    }
}
