using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public class TextPreviewExtension : ExplorerPreviewExtension
    {
        TextPreviewer control;
        public TextPreviewExtension()
        {
            Extensions = new[] { ".txt", ".cs", ".js", ".xml", ".htm", ".bat", ".html", ".log", ".csproj", ".config", ".resx", ".sln", ".settings", ".md", ".cpp", ".h", ".asm" };
            Fabric = (x) =>
            {
                if (control == null)
                {
                    control = new TextPreviewer() { Dock = DockStyle.Fill };
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