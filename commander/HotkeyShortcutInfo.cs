using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace commander
{
    public class HotkeyShortcutInfo
    {
        public string Path;
        public bool IsEnabled;
        public Keys Hotkey;

        internal void Execute(IFileListControl fl)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            string path = Path;
            psi.WorkingDirectory = new FileInfo(path).DirectoryName;
            psi.FileName = path;

            psi.Arguments = $"\"{fl.SelectedFile.FullName}\"";

            Process.Start(psi);
        }
    }

}