using System.Windows.Forms;
using commander.Controls;
using PluginLib;

namespace commander
{
    public class WebpPreviewExtension : ExplorerPreviewExtension
    {
        WebpViewer control;
        public WebpPreviewExtension()
        {
            Extensions = new[] { ".webp" };
            Fabric = (x) =>
            {
                if (control == null || control.IsDisposed)
                {
                    control = new WebpViewer() { Dock = DockStyle.Fill };
                }
                control.Init(x.FullName);
                return control;
            };
        }

        public override void Deselect()
        {
            if (control == null) return;
            control.ResetImage();
            control = null;
        }
    }
}



