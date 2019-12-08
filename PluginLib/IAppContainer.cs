using System.Windows.Forms;

namespace PluginLib
{
    public interface IAppContainer
    {
        MenuStrip MainMenu { get; }
        void OpenWindow(Form frm);
        void OpenWindow(Control cc);

        void AddExplorerPreviewExtension(ExplorerPreviewExtension e);

        ILibrary[] Libraries { get; }
        ITagInfo[] Tags { get; }

        bool ShowHiddenTags { get; }
    }
}
