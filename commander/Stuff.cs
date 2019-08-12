﻿using PluginLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace commander
{
    public class Stuff
    {
        public static Dictionary<string, Tuple<Bitmap, int>> Icons = new Dictionary<string, Tuple<Bitmap, int>>();
        public static Dictionary<string, Tuple<Bitmap, int>> ExeIcons = new Dictionary<string, Tuple<Bitmap, int>>();
        public static Tuple<Bitmap, int> GetBitmapOfFile(string fn)
        {
            var d = Path.GetExtension(fn).ToLower();
            if (d == ".exe" || d == ".ico")
            {
                if (!ExeIcons.ContainsKey(fn))
                {
                    var ico = Stuff.ExtractAssociatedIcon(fn);

                    if (ico != null)
                    {
                        try
                        {
                            var bb = Bitmap.FromHicon(ico.Handle);
                            bb.MakeTransparent();
                            Stuff.list.Images.Add(bb);
                            ExeIcons.Add(fn, new Tuple<Bitmap, int>(bb, Stuff.list.Images.Count - 1));
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                }
                return ExeIcons[fn];
            }
            if (!Icons.ContainsKey(d))
            {
                var ico = Stuff.ExtractAssociatedIcon(fn);

                if (ico != null)
                {
                    try
                    {
                        var bb = Bitmap.FromHicon(ico.Handle);
                        bb.MakeTransparent();
                        Stuff.list.Images.Add(bb);
                        Icons.Add(d, new Tuple<Bitmap, int>(bb, Stuff.list.Images.Count - 1));
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }

            return Icons[d];
        }
        public static IFilesystem DefaultFileSystem = new DiskFilesystem();
        public static List<MountInfo> MountInfos = new List<MountInfo>();
        public static ImageList list = null;
        public static List<IndexInfo> Indexes = new List<IndexInfo>();
        public static Icon ExtractAssociatedIcon(String filePath)
        {
            try
            {
                int index = 0;

                Uri uri;
                if (filePath == null)
                {
                    throw new ArgumentException(String.Format("'{0}' is not valid for '{1}'", "null", "filePath"), "filePath");
                }
                try
                {
                    uri = new Uri(filePath);
                }
                catch (UriFormatException)
                {
                    filePath = Path.GetFullPath(filePath);
                    uri = new Uri(filePath);
                }
                //if (uri.IsUnc)
                //{
                //  throw new ArgumentException(String.Format("'{0}' is not valid for '{1}'", filePath, "filePath"), "filePath");
                //}
                if (uri.IsFile)
                {
                    if (!File.Exists(filePath))
                    {
                        //IntSecurity.DemandReadFileIO(filePath);
                        throw new FileNotFoundException(filePath);
                    }

                    StringBuilder iconPath = new StringBuilder(260);
                    iconPath.Append(filePath);

                    IntPtr handle = SafeNativeMethods.ExtractAssociatedIcon(new HandleRef(null, IntPtr.Zero), iconPath, ref index);
                    if (handle != IntPtr.Zero)
                    {
                        //IntSecurity.ObjectFromWin32Handle.Demand();
                        return Icon.FromHandle(handle);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static List<IFileInfo> GetAllFiles(IDirectoryInfo dir, List<IFileInfo> files = null, int? level = null, int? maxlevel = null)
        {

            if (files == null)
            {
                files = new List<IFileInfo>();
            }
            if (level != null && maxlevel != null)
            {
                if (level.Value > maxlevel.Value) return files;
            }
            try
            {
                if (level != null) { level++; }
                foreach (var d in dir.GetDirectories())
                {

                    GetAllFiles(d, files, level, maxlevel);
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
        public static List<IDirectoryInfo> GetAllDirs(IDirectoryInfo dir, List<IDirectoryInfo> dirs = null)
        {
            if (dirs == null)
            {
                dirs = new List<IDirectoryInfo>();
            }

            try
            {
                dirs.Add(dir);
                foreach (var d in dir.GetDirectories())
                {
                    GetAllDirs(d, dirs);
                }
            }
            catch (UnauthorizedAccessException ex)
            {

            }
            catch (Exception ex)
            {
                //generate error
            }


            return dirs;
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
        public static string CalcMD5(MemoryStream ms)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(ms);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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


        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }


        public static void ExecuteFile(IFileInfo f)
        {
            if (f is VirtualFileInfo)
            {
                var vf = f as VirtualFileInfo;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.WorkingDirectory = f.Directory.FullName;
                psi.FileName = vf.FileInfo.FullName; ;
                Process.Start(psi);
            }
            else
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.WorkingDirectory = f.DirectoryName;
                psi.FileName = f.FullName;
                Process.Start(psi);
            }
        }

        public static void DeleteTag(TagInfo t)
        {
            Tags.Remove(t);
            IsDirty = true;
            if (TagsListChanged != null)
            {
                TagsListChanged();
            }
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

            foreach (var descendant in s.Descendants("hotkey"))
            {
                var path = descendant.Attribute("path").Value;
                var enabled = bool.Parse(descendant.Attribute("enabled").Value);
                var key = (Keys)Enum.Parse(typeof(Keys), descendant.Attribute("key").Value);
                Stuff.Hotkeys.Add(new HotkeyShortcutInfo() { IsEnabled = enabled, Hotkey = key, Path = path });
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

            foreach (var descendant in s.Descendants("fileContextMenuItem"))
            {
                var title = descendant.Attribute("title").Value;
                var appName = descendant.Attribute("appName").Value;
                string args = null;
                if (descendant.Element("arguments") != null)
                {
                    args = descendant.Element("arguments").Value;
                }
                var f = new FileContextMenuItem() { Title = title, AppName = appName, Arguments = args };
                Stuff.FileContextMenuItems.Add(f);
            }
            foreach (var descendant in s.Descendants("site"))
            {
                var path = descendant.Value;
                var f = new OfflineSiteInfo() { Path = path };
                Stuff.OfflineSites.Add(f);
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

            #region hotkeys            
            sb.AppendLine("<hotkeys>");
            foreach (var item in Stuff.Hotkeys)
            {
                sb.AppendLine($"<hotkey path=\"{item.Path}\" key=\"{item.Hotkey}\" enabled=\"{item.IsEnabled}\"/>");
            }
            sb.AppendLine("</hotkeys>");
            #endregion            

            sb.AppendLine("<fileContextMenuItems>");
            foreach (var item in Stuff.FileContextMenuItems)
            {
                if (string.IsNullOrEmpty(item.Arguments))
                {
                    sb.AppendLine($"<fileContextMenuItem title=\"{item.Title}\" appName=\"{item.AppName}\" />");
                }
                else
                {
                    sb.AppendLine($"<fileContextMenuItem title=\"{item.Title}\" appName=\"{item.AppName}\" >");
                    sb.AppendLine($"<arguments><![CDATA[{item.Arguments}]]></arguments>");
                    sb.AppendLine($"</fileContextMenuItem>");
                }
            }
            sb.AppendLine("</fileContextMenuItems>");
            sb.AppendLine("<offlineSites>");

            foreach (var item in Stuff.OfflineSites)
            {
                sb.AppendLine($"<site>");
                sb.Append("<![CDATA[" + item.Path);
                sb.Append("]]>");
                sb.AppendLine("</site>");

            }
            sb.AppendLine("</offlineSites>");
            sb.AppendLine("</settings>");
            File.WriteAllText("settings.xml", sb.ToString());
        }

        public static event Action TagsListChanged;
        internal static TagInfo AddTag(TagInfo tagInfo)
        {
            if (Tags.Any(z => z.Name == tagInfo.Name))
            {
                return Tags.First(z => z.Name == tagInfo.Name);
            }

            Tags.Add(tagInfo);
            IsDirty = true;
            if (TagsListChanged != null)
            {
                TagsListChanged();
            }
            return tagInfo;
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
        public static List<FileContextMenuItem> FileContextMenuItems { get; internal set; } = new List<FileContextMenuItem>();
        public static List<OfflineSiteInfo> OfflineSites { get; } = new List<OfflineSiteInfo>();
        public static List<HotkeyShortcutInfo> Hotkeys { get; internal set; } = new List<HotkeyShortcutInfo>();

        public static List<ShortcutInfo> Shortcuts = new List<ShortcutInfo>();

        public static void AppendFileToIso(FileStream fs, string path, byte[] bb)
        {
            var rep = path.Trim(new char[] { '\\' });
            var nm = Encoding.BigEndianUnicode.GetBytes(rep);
            fs.WriteByte((byte)rep.Length);
            fs.Write(nm, 0, nm.Length);
            var nn = BitConverter.GetBytes(bb.Length);
            fs.Write(nn, 0, nn.Length);
            fs.Write(bb, 0, bb.Length);
        }
        private static string GenerateMetaXml(IDirectoryInfo dir)
        {
            var fls = Stuff.GetAllFiles(dir);
            List<TagInfo> tags = new List<TagInfo>();
            foreach (var item in fls)
            {
                var ww = Stuff.Tags.Where(z => z.Files.Contains(item.FullName));
                tags.AddRange(ww);
            }
            tags = tags.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"/>");
            sb.AppendLine("<root>");
            sb.AppendLine("<tags>");
            foreach (var item in tags)
            {
                sb.AppendLine($"<tag name=\"{item.Name}\">");
                var aa = fls.Where(z => item.Files.Contains(z.FullName)).ToArray();
                foreach (var aitem in aa)
                {
                    sb.AppendLine($"<file><![CDATA[{aitem.FullName.Replace(dir.FullName, "").Trim(new char[] { '\\' })}]]></file>");
                }
                sb.AppendLine($"</tag>");
            }
            sb.AppendLine("</tags>");
            sb.AppendLine("</root>");

            return sb.ToString();

        }

        internal static void PackToIso(IDirectoryInfo dir, string path)
        {
            var fls = Stuff.GetAllFiles(dir);
            var drs = Stuff.GetAllDirs(dir);
            //generate meta.xml
            var mm = GenerateMetaXml(dir);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                AppendFileToIso(fs, ".indx\\meta.xml", Encoding.UTF8.GetBytes(mm));
                foreach (var item in fls)
                {
                    if (item.Length > 1024 * 1024 * 10) continue;//10 Mb

                    var bb = dir.Filesystem.ReadAllBytes(item.FullName);
                    var rep = item.FullName.Replace(dir.FullName, "").Trim(new char[] { '\\' });
                    AppendFileToIso(fs, rep, bb);
                }
            }
        }
    }

    public class OfflineSiteInfo
    {
        public string Path;
        public string Uri;
    }


    public class IndexInfo
    {
        public string Path;
        public string Text;
    }

}