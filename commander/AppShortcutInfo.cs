using System.Diagnostics;
using System.IO;

namespace commander
{
    public class AppShortcutInfo : ShortcutInfo
    {

        public AppShortcutInfo(string p, string n)
        {
            Path = p;
            Name = n;
        }
        public string Path;

        public string TempArguments { get; set; }

        public override void Run()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WorkingDirectory = new FileInfo(Path).DirectoryName;
            psi.FileName = Path;
            if (TempArguments != null)
            {
                psi.Arguments = $"\"{TempArguments}\"";
                TempArguments = null;
            }
            Process.Start(psi);
        }
    }
}