using PluginLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace commander
{
    public class Stuff
    {
        public static List<FileInfo> GetAllFiles(DirectoryInfo dir, List<FileInfo> files = null)
        {
            if (files == null)
            {
                files = new List<FileInfo>();
            }

            try
            {
                foreach (var d in dir.GetDirectories())
                {
                    GetAllFiles(d, files);
                }
            }
            catch (UnauthorizedAccessException ex)
            {

            }
            catch (Exception ex)
            {
                //generate error
            }

            try
            {
                foreach (var file in dir.GetFiles())
                {
                    files.Add(file);
                }
            }
            catch (Exception ex)
            {

            }
            return files;
        }
        public static string CalcMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

        }
        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        public static string CalcPartMD5(string filename, int bytes)
        {

            var fi = new FileInfo(filename);
            if (fi.Length < bytes) return CalcMD5(filename);
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    byte[] bb = new byte[bytes];
                    stream.Read(bb, 0, bytes);
                    var hash = md5.ComputeHash(bb);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

        }

        public static void ExecuteFile(FileInfo f)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WorkingDirectory = f.DirectoryName;
            psi.FileName = f.FullName;
            Process.Start(psi);
        }

        public static string GetUserFriendlyFileSize(long _v)
        {
            double v = _v;
            string[] sfxs = new[] { "B", "Kb", "Mb", "Gb" };
            for (int i = 0; i < sfxs.Length; i++)
            {
                if (v < 1024)
                {
                    return v.ToString("F") + sfxs[i];
                }
                v /= 1024;
            }
            return v.ToString("F") + sfxs.Last();
        }

        public static TagInfo[] GetTagsOfFile(string fullName)
        {
            return Stuff.Tags.Where(z => (!z.IsHidden || Stuff.ShowHidden) && z.Files.Contains(fullName)).ToArray();
        }

        public static bool IsDirty = false;
        public static List<string> RecentPathes = new List<string>();
        public static List<TabInfo> Tabs = new List<TabInfo>();

        public static void AddTab(TabInfo tab)
        {
            if (Tabs.Contains(tab))
            {
                throw new Exception("tab was already added");
            }
            Tabs.Add(tab);
            IsDirty = true;
        }
        public static void LoadSettings()
        {
            var s = XDocument.Load("settings.xml");
            var fr = s.Descendants("settings").First();
            Stuff.PasswordHash = fr.Attribute("password").Value;
            foreach (var descendant in s.Descendants("path"))
            {
                RecentPathes.Add(descendant.Value);
            }

            foreach (var descendant in s.Descendants("tab"))
            {
                var hint = descendant.Attribute("hint").Value;
                var owner = descendant.Attribute("owner").Value;
                var path = descendant.Attribute("path").Value;
                var filter = descendant.Attribute("filter").Value;

                var tab = new TabInfo() { Filter = filter, Path = path, Hint = hint };
                tab.Owner = owner;
                Stuff.AddTab(tab);
            }

            foreach (var descendant in s.Descendants("library"))
            {
                var name = descendant.Attribute("name").Value;
                var path = descendant.Attribute("path").Value;
                Stuff.Libraries.Add(new FilesystemLibrary() { Name = name, BaseDirectory = path });
            }
            foreach (var descendant in s.Descendants("shortcut"))
            {
                var name = descendant.Attribute("name").Value;

                if (descendant.Attribute("type") != null)
                {
                    var tp = descendant.Attribute("type").Value;
                    switch (tp)
                    {
                        case "url":
                            var url = descendant.Element("url").Value;
                            Stuff.Shortcuts.Add(new UrlShortcutInfo(name, url));
                            break;
                        case "cmd":
                            var wd = descendant.Attribute("workdir").Value;
                            string args = null;
                            if (descendant.Element("args") != null)
                            {
                                args = descendant.Element("args").Value;
                            }

                            string appName = null;
                            if (descendant.Element("app") != null)
                            {
                                appName = descendant.Element("app").Value;
                            }
                            Stuff.Shortcuts.Add(new CmdShortcutInfo(name, args, wd, appName));
                            break;
                    }

                }
                else
                {
                    var path = descendant.Attribute("path").Value;
                    Stuff.Shortcuts.Add(new AppShortcutInfo(path, name));
                }
            }
            foreach (var descendant in s.Descendants("tag"))
            {
                var name = descendant.Attribute("name").Value;

                string flags = "";
                if (descendant.Attribute("flags") != null) { flags = descendant.Attribute("flags").Value; }

                var tag = new TagInfo() { Name = name, IsHidden = flags.Contains("hidden") };
                Stuff.Tags.Add(tag);
                foreach (var item in descendant.Descendants("file"))
                {
                    tag.AddFile(item.Value);
                }
            }
        }
        public static void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine($"<settings password=\"{PasswordHash}\">");
            sb.AppendLine("<tabs>");

            foreach (var item in Stuff.Tabs)
            {
                sb.AppendLine($"<tab hint=\"{item.Hint}\" owner=\"{item.Owner}\" path=\"{item.Path}\" filter=\"{item.Filter}\"/>");
            }

            sb.AppendLine("</tabs>");
            sb.AppendLine("<libraries>");
            foreach (var item in Stuff.Libraries.OfType<FilesystemLibrary>())
            {
                sb.AppendLine($"<library name=\"{item.Name}\" path=\"{item.BaseDirectory}\" />");
            }
            sb.AppendLine("</libraries>");
            sb.AppendLine("<tags>");
            foreach (var item in Stuff.Tags)
            {
                sb.AppendLine($"<tag name=\"{item.Name}\" flags=\"{(item.IsHidden ? "hidden" : "")}\" >");
                foreach (var fitem in item.Files)
                {

                    sb.AppendLine($"<file><![CDATA[{fitem}]]></file>");
                }
                sb.AppendLine($"</tag>");
            }
            sb.AppendLine("</tags>");

            sb.AppendLine("<shortcuts>");
            foreach (var item in Stuff.Shortcuts.OfType<AppShortcutInfo>())
            {
                sb.AppendLine($"<shortcut name=\"{item.Name}\" path=\"{item.Path}\" />");
            }
            foreach (var item in Stuff.Shortcuts.OfType<CmdShortcutInfo>())
            {
                sb.AppendLine($"<shortcut type=\"cmd\" name=\"{item.Name}\" workdir=\"{item.WorkDir}\" >");
                sb.Append("<args><![CDATA[" + item.Args);
                sb.Append("]]></args>");
                sb.AppendLine("</shortcut>");

            }
            foreach (var item in Stuff.Shortcuts.OfType<UrlShortcutInfo>())
            {
                sb.AppendLine($"<shortcut type=\"url\" name=\"{item.Name}\" >");
                sb.Append("<url><![CDATA[" + item.Url);
                sb.Append("]]></url>");
                sb.AppendLine("</shortcut>");

            }
            sb.AppendLine("</shortcuts>");

            sb.AppendLine("</settings>");
            File.WriteAllText("settings.xml", sb.ToString());
        }

        internal static void AddTag(TagInfo tagInfo)
        {
            Tags.Add(tagInfo);
            IsDirty = true;
        }

        public static List<ILibrary> Libraries = new List<ILibrary>();
        public static List<TagInfo> Tags = new List<TagInfo>();
        public static bool ShowHidden = false;

        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static DialogResult Question(string v)
        {
            return MessageBox.Show(v, mdi.MainForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        public static DialogResult Info(string v)
        {
            return MessageBox.Show(v, mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static DialogResult Warning(string v)
        {
            return MessageBox.Show(v, mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static DialogResult Error(string v)
        {
            return MessageBox.Show(v, mdi.MainForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static string PasswordHash { get; internal set; }
        public static string GitBashPath { get; internal set; }
        public static string VsCmdBatPath { get; internal set; } 

        public static List<ShortcutInfo> Shortcuts = new List<ShortcutInfo>();
    }

    public abstract class ShortcutInfo
    {

        public string Name;

        public abstract void Run();

    }
    public class AppShortcutInfo : ShortcutInfo
    {

        public AppShortcutInfo(string p, string n)
        {
            Path = p;
            Name = n;
        }
        public string Path;


        public override void Run()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WorkingDirectory = new FileInfo(Path).DirectoryName;
            psi.FileName = Path;
            Process.Start(psi);
        }
    }
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