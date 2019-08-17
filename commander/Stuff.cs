using DjvuNet;
using PluginLib;
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


        public static string[] ParseHtmlItems(string lns, string key1, string key2)
        {
            string accum = "";
            bool inside = false;
            StringBuilder accum2 = new StringBuilder();
            List<string> list = new List<string>();
            for (int i = 0; i < lns.Length; i++)
            {
                if (accum.Length > Math.Max(key1.Length, key2.Length))
                {
                    accum = accum.Remove(0, 1);
                }
                accum += lns[i];
                if (accum.EndsWith(key1))
                {
                    inside = true;
                }

                if (inside && accum.EndsWith(key2))
                {
                    inside = false;
                    list.Add(accum2.ToString());
                    accum2.Clear();
                }

                if (inside)
                {
                    accum2.Append(lns[i]);
                }
            }
            return list.ToArray();
        }
        internal static void DeleteBookmark(UrlBookmark bm)
        {
            UrlBookmarks.Remove(bm);
            Stuff.IsDirty = true;
        }

        public static List<UrlBookmark> UrlBookmarks = new List<UrlBookmark>();
        internal static void AddUrlBookmark(UrlBookmark b)
        {
            UrlBookmarks.Add(b);
            IsDirty = true;
        }

        public static IFilesystem DefaultFileSystem = new DiskFilesystem();
        public static List<MountInfo> MountInfos = new List<MountInfo>();
        public static ImageList list = null;
        public static List<IndexInfo> Indexes = new List<IndexInfo>();

        public static event Action<IndexInfo> IndexAdded;
        public static void AddIndex(IndexInfo ii)
        {
            if (Indexes.Any(z => z.Path == ii.Path)) return;
            Indexes.Add(ii);
            if (IndexAdded != null)
            {
                IndexAdded(ii);
            }
        }
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
            return Stuff.Tags.Where(z => (!z.IsHidden || Stuff.ShowHidden) && z.ContainsFile(fullName)).ToArray();
        }

        public static bool IsDirty = false;
        public static List<string> RecentPathes = new List<string>();
        public static List<TabInfo> Tabs = new List<TabInfo>();

        public static string WrapInCdata(string str)
        {
            return $"<![CDATA[{str}]]>";
        }
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

            List<DirectoryEntry> direntries = new List<DirectoryEntry>();
            List<FileEntry> fileentries = new List<FileEntry>();
            #region load directory and files entries
            var entries = s.Descendants("entries").First();
            foreach (var item in entries.Descendants("directory"))
            {
                var id = int.Parse(item.Attribute("id").Value);
                var path = item.Value;
                direntries.Add(new DirectoryEntry() { Id = id, Path = path });
            }
            foreach (var item in entries.Descendants("file"))
            {
                var id = int.Parse(item.Attribute("id").Value);
                var dirId = int.Parse(item.Attribute("dirId").Value);
                var name = item.Value;
                fileentries.Add(new FileEntry() { Id = id, Directory = direntries.First(z => z.Id == dirId), Name = name });
            }
            #endregion

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
                    var arr1 = item.Attribute("id").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    foreach (var aitem in arr1)
                    {
                        var ff = fileentries.First(z => z.Id == aitem);
                        tag.AddFile(ff.FullName);
                    }
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
            foreach (var book in s.Descendants("bookmark"))
            {
                var uri = book.Attribute("uri").Value;
                var orig = book.Attribute("original").Value;
                var info = Encoding.UTF8.GetString(Convert.FromBase64String(book.Attribute("info").Value));

                var f = new UrlBookmark() { OriginalUrl = orig, Info = info, Uri = new Uri(uri) };
                Stuff.AddUrlBookmark(f);
            }
        }

        public static void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine($"<settings password=\"{PasswordHash}\">");

            #region directories entries
            sb.AppendLine("<entries>");

            Dictionary<string, DirectoryEntry> dirdic1 = new Dictionary<string, DirectoryEntry>();
            List<DirectoryEntry> diren = new List<DirectoryEntry>();
            int dirind = 0;
            foreach (var item in Stuff.Tags)
            {
                foreach (var fitem in item.Files)
                {
                    var fin = new FileInfo(fitem);
                    if (dirdic1.ContainsKey(fin.DirectoryName)) continue;
                    var dd = new DirectoryEntry() { Id = dirind++, Path = fin.DirectoryName };
                    dirdic1.Add(fin.DirectoryName, dd);
                    diren.Add(dd);
                    sb.AppendLine($"<directory id=\"{dirdic1[fin.DirectoryName].Id}\">");
                    sb.AppendLine(WrapInCdata(fin.DirectoryName));
                    sb.AppendLine("</directory>");
                }
            }
            #endregion

            #region files entries
            Dictionary<string, FileEntry> fldic1 = new Dictionary<string, FileEntry>();
            HashSet<string> filesentrs = new HashSet<string>();
            List<FileEntry> flentrs = new List<FileEntry>();
            int flind = 0;
            foreach (var item in Stuff.Tags)
            {
                foreach (var fitem in item.Files)
                {
                    var fin = new FileInfo(fitem);
                    if (filesentrs.Add(fin.FullName))
                    {
                        var den = dirdic1[fin.DirectoryName];
                        sb.AppendLine($"<file dirId=\"{den.Id}\" id=\"{flind++}\">");
                        var fen = new FileEntry() { Id = flind - 1, Name = fin.Name, Directory = den };
                        flentrs.Add(fen);
                        fldic1.Add(fin.FullName, fen);
                        sb.AppendLine(WrapInCdata(fin.Name));
                        sb.AppendLine("</file>");
                    }
                }
            }
            sb.AppendLine("</entries>");
            #endregion

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
                sb.Append($"<file id=\"");
                foreach (var fitem in item.Files)
                {
                    var fen = fldic1[fitem];
                    sb.Append($"{fen.Id};");
                }
                sb.AppendLine($"\"/>");
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

            sb.AppendLine("<bookmarks>");
            foreach (var item in Stuff.UrlBookmarks)
            {
                sb.AppendLine($"<bookmark uri=\"{item.Uri.ToString()}\" info=\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Info))}\" original=\"{item.OriginalUrl}\"/>");
            }
            sb.AppendLine("</bookmarks>");

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

        public static void IndexFile(IFileInfo selectedFile)
        {
            if (!selectedFile.Extension.ToLower().EndsWith("djvu")) return;

            DjvuDocument doc = new DjvuDocument(selectedFile.FullName);
            var cnt = doc.Pages.Count();
            StringBuilder sbb = new StringBuilder();
            for (int i = 0; i < cnt; i++)
            {
                var txt = doc.Pages[i].GetTextForLocation(new System.Drawing.Rectangle(0, 0, doc.Pages[i].Width, doc.Pages[i].Height));
                sbb.AppendLine(txt.Replace("\r", "").Replace("\t", ""));
            }

            Stuff.AddIndex(new IndexInfo() { Path = selectedFile.FullName, Text = sbb.ToString() });
            Stuff.Info("Indexing compete: " + sbb.ToString().Length + " symbols.");
            /*page
                .BuildPageImage()
                .Save("TestImage1.png", ImageFormat.Png);

            page.IsInverted = true;

            page
                .BuildPageImage()
                .Save("TestImage2.png", ImageFormat.Png);*/
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


    public class DirectoryEntry
    {
        public int Id;
        public string Path;
    }
    public class FileEntry
    {
        public int Id;
        public DirectoryEntry Directory;
        public string Name;
        public string FullName => Path.Combine(Directory.Path, Name);
    }

}