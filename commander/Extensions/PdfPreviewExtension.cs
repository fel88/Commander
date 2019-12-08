using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class PdfPreviewExtension : ExplorerPreviewExtension
    {
        PdfPreviewer control;
        public PdfPreviewExtension()
        {
            Extensions = new[] { ".pdf" };
            
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new PdfPreviewer() { Dock = DockStyle.Fill };
                }
                control.Init(x);
                return control;
            };
        }

        public override void Release(IFileInfo f)
        {
            if (control == null) return;
            if (control.CurrentFile != null && control.CurrentFile.FullName == f.FullName)
            {
                control.Reset();
            }            
        }
    }
}