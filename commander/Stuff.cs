using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace commander
{
    public class Stuff
    {
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
        public static bool IsDirty = false;
        public static List<string> RecentPathes = new List<string>();
        public static void LoadSettings(FileListControl fileListControl1, FileListControl fileListControl2)
        {
            var s = XDocument.Load("settings.xml");
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
                FileListControl fc = fileListControl1;
                if (owner == "right")
                {
                    fc = fileListControl2;
                }
                fc.AddTab(new TabInfo() { Filter = filter, Path = path, Hint = hint });
            }

            foreach (var descendant in s.Descendants("library"))
            {
                var name = descendant.Attribute("name").Value;
                var path = descendant.Attribute("path").Value;
                Stuff.Libraries.Add(new FilesystemLibrary() { Name = name, BaseDirectory = path });
            }
        }
        public static void SaveSettings(FileListControl flc1, FileListControl flc2)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<settings>");
            sb.AppendLine("<tabs>");
            foreach (var item in flc1.Tabs)
            {
                sb.AppendLine($"<tab hint=\"{item.Hint}\" owner=\"left\" path=\"{item.Path}\" filter=\"{item.Filter}\"/>");
            }
            foreach (var item in flc2.Tabs)
            {
                sb.AppendLine($"<tab hint=\"{item.Hint}\" owner=\"right\" path=\"{item.Path}\" filter=\"{item.Filter}\"/>");
            }
            sb.AppendLine("</tabs>");
            sb.AppendLine("<libraries>");
            foreach (var item in Stuff.Libraries.OfType<FilesystemLibrary>())
            {
                sb.AppendLine($"<library name=\"{item.Name}\" path=\"{item.BaseDirectory}\" />");
            }
            sb.AppendLine("</libraries>");
            sb.AppendLine("</settings>");
            File.WriteAllText("settings.xml", sb.ToString());
        }


        public static List<ILibrary> Libraries = new List<ILibrary>();
    }
}