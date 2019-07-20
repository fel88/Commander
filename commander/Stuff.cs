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

            foreach (var descendant in s.Descendants("tag"))
            {
                var name = descendant.Attribute("name").Value;

                string flags = "";
                if (descendant.Attribute("flags") != null) { flags = descendant.Attribute("flags").Value; }

                var tag = new TagInfo() { Name = name, IsHidden = flags.Contains("hidden") };
                Stuff.Tags.Add(tag);
                foreach (var item in descendant.Descendants("file"))
                {
                    tag.Files.Add(item.Value);
                }
            }
        }
        public static void SaveSettings(FileListControl flc1, FileListControl flc2)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine($"<settings password=\"{PasswordHash}\">");
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
        public static string PasswordHash { get; internal set; }
    }

    public class TagInfo
    {
        public string Name;
        public bool IsHidden;
        public List<string> Files = new List<string>();
    }
}