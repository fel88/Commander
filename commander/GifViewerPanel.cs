using System;
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
            label1 = new Label();
            label1.ForeColor = System.Drawing.Color.Red;
            pictureBox1.Controls.Add(label1);
            label1.Visible = false;
        }
        Label label1;


        GifLib.GifContainer container;
        internal void SetImage(IFileInfo file)
        {
            if (file == null) { container = null; return; }
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
            try
            {
                pictureBox1.Image = container.GetFrame(frame++);
                frame %= container.Frames;
                if (container.LastGceBlock.Delay == 0) { timer1.Interval = 15; }
                else
                {
                    timer1.Interval = container.LastGceBlock.Delay;
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
                label1.Visible = true;
            }
        }
    }
}
