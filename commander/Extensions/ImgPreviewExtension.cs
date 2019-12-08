using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class ImgPreviewExtension : ExplorerPreviewExtension
    {
        ImgViewerPanel control;
        public ImgPreviewExtension()
        {
            Extensions = new[] { ".jpg", ".png", ".bmp" };
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new ImgViewerPanel() { Dock = DockStyle.Fill };
                }
                control.SetImage(x);
                return control;
            };
        }

        public override void Release(IFileInfo f)
        {
            if (control == null) return;
            if (control.CurrentFile != null && control.CurrentFile.FullName == f.FullName)
            {
                control.ResetImage();
            }
        }
    }
}