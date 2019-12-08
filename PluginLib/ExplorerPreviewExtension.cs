using System;
using System.Windows.Forms;

namespace PluginLib
{
    public class ExplorerPreviewExtension
    {
        public string[] Extensions;
        public Func<IFileInfo, UserControl> Fabric;
        public virtual void Deselect() { }
        public virtual void Release(IFileInfo f) { }
    }
}
