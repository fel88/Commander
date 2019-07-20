using System.Windows.Forms;

namespace PluginLib
{
    public interface IAppContainer
    {
        MenuStrip MainMenu { get; }
        void OpenWindow(Form frm);
        void OpenWindow(Control cc);
    }
}
