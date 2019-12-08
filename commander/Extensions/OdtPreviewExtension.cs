using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class OdtPreviewExtension : ExplorerPreviewExtension
    {
        OdtPreviewer control;
        public OdtPreviewExtension()
        {
            Extensions = new[] { ".odt" };
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new OdtPreviewer() { Dock = DockStyle.Fill };
                }
                control.LoadFile(x);
                return control;
            };
        }

        public override void Deselect()
        {
            if (control == null) return;
            control.Disable();
        }
    }
}