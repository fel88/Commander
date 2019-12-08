using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class VideoPreviewExtension : ExplorerPreviewExtension
    {
        VideoPlayer control;
        public VideoPreviewExtension()
        {
            Extensions = new[] { ".mpg", ".flv", ".wmv", ".mp4", ".avi", ".mkv", ".webm", ".mp3" };          
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new VideoPlayer() { Dock = DockStyle.Fill };
                }
                control.RunVideo(x.FullName);
                return control;
            };
        }

        public override void Deselect()
        {
            if (control == null) return;
            control.StopVideo();
        }
    }
}



