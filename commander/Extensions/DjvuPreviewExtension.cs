using PluginLib;
using System.Windows.Forms;

namespace commander
{
    public class DjvuPreviewExtension : ExplorerPreviewExtension
    {
        DjvuReader control;
        public DjvuPreviewExtension()
        {
            Extensions = new[] { ".djvu" };
          
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new DjvuReader() { Dock = DockStyle.Fill };
                }
                control.Init(x);
                return control;
            };
        }
        public override void Deselect()
        {
            if (control == null) return;
            control.UnloadBook();
        }
    }

}



