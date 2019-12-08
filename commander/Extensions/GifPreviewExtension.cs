using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class GifPreviewExtension : ExplorerPreviewExtension
    {
        GifViewerPanel control;
        public GifPreviewExtension()
        {
            Extensions = new[] { ".gif" };
           
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new GifViewerPanel() { Dock = DockStyle.Fill };
                }
                control.SetImage(x);
                return control;
            };
        }

        public override void Deselect()
        {
            if (control == null) return;
            control.SetImage(null);
        }
    }
}