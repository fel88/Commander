using System.Diagnostics;

namespace commander
{
    public class UrlShortcutInfo : ShortcutInfo
    {
        public string Url;
        public UrlShortcutInfo(string name, string url)
        {
            Url = url;
            Name = name;
        }

        public override void Run()
        {
            Process.Start(Url);
        }
    }
}