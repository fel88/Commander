using System.Diagnostics;

namespace commander
{
    public class CmdShortcutInfo : ShortcutInfo
    {
        public CmdShortcutInfo(string name, string args, string wd, string appName = null)
        {
            if (appName != null)
            {
                AppName = appName;
            }

            WorkDir = wd;
            Args = args;
            Name = name;
        }

        public string AppName = "cmd.exe";
        public string WorkDir;
        public string Args;


        public override void Run()
        {

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = AppName;
            if (Args != null)
            {
                psi.Arguments = Args;
            }

            psi.WorkingDirectory = WorkDir;
            Process.Start(psi);
        }
    }
}