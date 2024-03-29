﻿using DjvuNet;
using IsoLib;
using IsoLib.DiscUtils;
using PdfiumViewer;
using PluginLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Trinet.Core.IO.Ntfs;

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
                    //var ico = Stuff.ExtractAssociatedIcon(fn);
                    var ico = ShellIcon.GetSmallIcon(fn);

                    if (ico != null)
                    {
                        try
                        {
                            var bb = Bitmap.FromHicon(ico.Handle);
                            bb.MakeTransparent();
                            //var bb = ico;
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
                var ico = ShellIcon.GetSmallIconFromExtension(d);
                //var ico = Stuff.ExtractAssociatedIcon(fn);

                if (ico != null)
                {
                    try
                    {
                        var bb = Bitmap.FromHicon(ico.Handle);
                        bb.MakeTransparent();
                        //var bb = ico;
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

        public static FileMetaInfo GetMetaInfoOfFile(IFileInfo fileInfo)
        {

            foreach (var item in Stuff.MetaInfos)
            {
                if (item.File.FullName.ToLower() != fileInfo.FullName.ToLower()) continue;
                return item;
            }
            return null;
        }

        public static void UnmountAll()
        {
            var arr = Stuff.MountInfos.ToArray();
            foreach (var item in arr)
            {
                Unmount(item);
            }
        }

        public static void Unmount(MountInfo m)
        {
            MountInfos.Remove(m);

            //unmount all tags
            var fls = m.MountTarget.GetFiles();
            var temp = Stuff.IsDirty;
            foreach (var item in fls)
            {
                var tags = Stuff.GetTagsOfFile(item.FullName);
                foreach (var titem in tags)
                {
                    titem.DeleteFile(item.FullName);
                }
            }
            Stuff.IsDirty = temp;
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
        internal static void ReadMeta(IFileInfo fileInfo)
        {
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileInfo.FullName));
            if (!File.Exists("settings.zip"))
            {
                return;
            }
            using (var zip = System.IO.Compression.ZipFile.OpenRead("settings.zip"))
            {
                foreach (var item in zip.Entries)
                {
                    if (item.FullName.StartsWith("meta"))
                    {
                        if (item.FullName.EndsWith(b64))
                        {
                            using (var stream = item.Open())
                            {
                                var r = new StreamReader(stream);
                                var mi = Stuff.GetMetaInfoOfFile(fileInfo);
                                if (mi == null)
                                {
                                    Stuff.MetaInfos.Add(new FileMetaInfo() { File = fileInfo });
                                }
                                mi = Stuff.GetMetaInfoOfFile(fileInfo);
                                if (mi.Infos.Any(z => z is SubtitlesMetaInfo))
                                {
                                    mi.Infos.RemoveAll(z => z is SubtitlesMetaInfo);
                                }
                                var sub = SubtitlesMetaInfo.FromSRT(r.ReadToEnd());
                                sub.Parent = mi;
                                mi.Infos.Add(sub);
                            }
                        }
                    }
                }
            }
        }

        internal static void SaveMeta(IFileInfo fileInfo)
        {
            var mi = Stuff.GetMetaInfoOfFile(fileInfo);
            if (mi == null) return;
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileInfo.FullName));
            if (!File.Exists("settings.zip"))
            {
                using (var cr = ZipFile.Open("settings.zip", ZipArchiveMode.Create))
                {


                }
            }
            using (var zip = System.IO.Compression.ZipFile.Open("settings.zip", ZipArchiveMode.Update))
            {
                var fr = (zip.Entries.FirstOrDefault(z => z.FullName.EndsWith(b64)));
                if (fr != null)
                {
                    fr.Delete();
                }

                var fr1 = mi.Infos.FirstOrDefault(z => z is SubtitlesMetaInfo);
                if (fr1 != null)
                {
                    var ent = zip.CreateEntry("meta\\" + b64);

                    using (var stream = ent.Open())
                    {
                        using (var r = new StreamWriter(stream))
                        {
                            var sub = (fr1 as SubtitlesMetaInfo);
                            r.Write(sub.GetSRT());
                            r.Flush();
                        }
                    }
                }
            }
        }

        internal static void DeleteBookmark(UrlBookmark bm)
        {
            UrlBookmarks.Remove(bm);
            Stuff.IsDirty = true;
        }

        public static List<UrlBookmark> UrlBookmarks = new List<UrlBookmark>();
        internal static bool AddUrlBookmark(UrlBookmark b)
        {
            if (UrlBookmarks.Any(z => z.OriginalUrl == b.OriginalUrl))
            {
                return false;
            }
            UrlBookmarks.Add(b);
            IsDirty = true;
            return true;
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

        public static event Action<HelperEnum> HelperVisibleChanged;

        public static void OnHelperVisibleChanged(HelperEnum h)
        {
            HelperVisibleChanged(h);
        }


        public static bool TagsHelperVisible = true;
        public static bool FiltersHelperVisible = true;
        public static bool AllowNTFSStreamsSync = true;

        public static void SyncMetaInfo(IFileInfo selectedFile)
        {
            foreach (var item in new FileInfo(selectedFile.FullName).ListAlternateDataStreams())
            {
                if (item.Name == "metainfo")
                {
                    try
                    {
                        using (var s = item.OpenText())
                        {
                            var txt = s.ReadToEnd();

                            var doc = XDocument.Parse(txt);
                            var tags = doc.Element("root").Element("tags");
                            foreach (var titem in tags.Elements("tag"))
                            {
                                var nm = titem.Attribute("name").Value;
                                var tg = Stuff.Tags.FirstOrDefault(z => z.Name.ToLower() == nm.ToLower());
                                if (tg == null)
                                {
                                    tg = Stuff.AddTag(new TagInfo() { Name = nm }, false);
                                }
                                if (!tg.ContainsFile(selectedFile))
                                    tg.AddFile(selectedFile);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Stuff.Error("Wrong metainfo detected");
                        item.Delete();
                    }
                }
            }
        }

        public static void UpdateFileMetaInfo(IFileInfo item)
        {
            if (!item.Exist)
                return;

            if (item.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                if (Stuff.Question("File is read-only, do you want to update tags anyway?") != DialogResult.Yes)
                    return;

                File.SetAttributes(item.FullName, item.Attributes & ~FileAttributes.ReadOnly);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<tags>");
            var tags = Stuff.Tags.Where(z => z.ContainsFile(item));
            foreach (var titem in tags)
            {
                sb.AppendLine($"<tag name=\"{titem.Name}\" />");
            }

            sb.AppendLine("</tags>");
            sb.AppendLine("</root>");

            var altstream = new FileInfo(item.FullName).GetAlternateDataStream("metainfo", FileMode.Create);
            altstream.Delete();
            using (var fs = altstream.OpenWrite())
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sb.ToString());
                }
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

        internal static long GetDirectorySize(IDirectoryInfo d)
        {
            long ret = 0;
            foreach (var item in d.GetDirectories())
            {
                ret += GetDirectorySize(item);
            }
            foreach (var item in d.GetFiles())
            {
                ret += item.Length;
            }
            return ret;
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

        internal static void MountIso(MountInfo mountInfo)
        {
            Stuff.MountInfos.Add(mountInfo);
            //extract .indx/meta.xml
            var f = mountInfo;
            if (f.Reader == null)
            {
                IsoReader reader = new IsoReader();
                reader.Parse(f.IsoPath.FullName);
                f.Reader = reader;
            }
            var r = new IsoDirectoryInfoWrapper(f, f.Reader.WorkPvd.RootDir);
            //r.Parent = null;
            r.Filesystem = new IsoFilesystem(f) { IsoFileInfo = f.IsoPath };
            MountIndex(".indx\\meta.xml", r, mountInfo.MountTarget, mountInfo.FullPath);
        }

        public static void MountIndex(string indexPath, IDirectoryInfo r, IDirectoryInfo mountTarget, string mountFullPath)
        {
            if (r.Filesystem.FileExist(indexPath))
            {
                var txt = r.Filesystem.ReadAllText(indexPath);
                var doc = XDocument.Parse(txt);
                List<FileEntry> fEntries = new List<FileEntry>();
                List<DirectoryEntry> dEntries = new List<DirectoryEntry>();
                var entries = doc.Descendants("entries").First();
                foreach (var item in entries.Elements("directory"))
                {
                    var id = int.Parse(item.Attribute("id").Value);
                    var path = item.Value;

                    //dEntries.Add(new DirectoryEntry() { Id = id, Path = Path.Combine(root.FullName, path) });
                    dEntries.Add(new DirectoryEntry() { Id = id, Path = Path.Combine(mountFullPath, path) });
                }
                foreach (var item in entries.Descendants("file"))
                {
                    var id = int.Parse(item.Attribute("id").Value);
                    var dirId = int.Parse(item.Attribute("dirId").Value);
                    var name = item.Value;

                    var dir = dEntries.First(z => z.Id == dirId);

                    fEntries.Add(new FileEntry() { Id = id, Directory = dir, Name = Path.Combine(mountTarget.FullName, name) });
                }
                var fls = Stuff.GetAllFiles(mountTarget);
                foreach (var item in doc.Descendants("tag"))
                {
                    var nm = item.Attribute("name").Value;
                    var tagg = Stuff.AddTag(new TagInfo() { Name = nm });
                    foreach (var fitem in item.Descendants("file"))
                    {
                        var arr1 = fitem.Attribute("id").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                        foreach (var aitem in arr1)
                        {
                            var ff = fEntries.First(z => z.Id == aitem);
                            var fr = fls.First(z => z.FullName.ToLower() == ff.FullName.ToLower());
                            tagg.AddFile(fr);
                            //tagg.AddFile(new FileInfoWrapper(ff.FullName), false);
                        }
                        /*var pt = fitem.Value;
                        var path = Path.Combine(mountInfo.MountTarget.FullName, pt);
                       
                        var fr = fls.First(z => z.FullName.ToLower() == path.ToLower());
                        tagg.AddFile(fr);*/

                        //tagg.AddFile((r.Filesystem as IsoFilesystem).GetFile(path));
                    }
                }
            }
        }

        public static string CalcMD5(IFileInfo file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = file.Filesystem.OpenReadOnlyStream(file))
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

        public static string CalcPartMD5(IFileInfo file, int bytes)
        {

            if (file.Length < bytes) return CalcMD5(file);
            using (var md5 = MD5.Create())
            {
                using (var stream = file.Filesystem.OpenReadOnlyStream(file))
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

            if (!AllowNTFSStreamsSync)
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
            return Stuff.Tags.Where(z => /*(!z.IsHidden || Stuff.ShowHidden) &&*/ z.ContainsFile(fullName)).ToArray();
        }
        public static TagInfo[] GetAllTagsOfFile(string fullName)
        {
            return Stuff.Tags.Where(z => z.ContainsFile(fullName)).ToArray();
        }
        public static bool IsDirty { get; set; } = false;
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

        public static void GetDefaultPathes()
        {
            var p1 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var p2 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var dirs = new DirectoryInfo(p1).GetDirectories().Union(new DirectoryInfo(p2).GetDirectories());
            var lod = dirs.FirstOrDefault(z => z.Name.ToLower() == "libreoffice");
            if (lod == null) return;
            var path = Path.Combine(lod.FullName, "program", "soffice.com");
            if (!File.Exists(path)) return;
            Stuff.LibreOfficePath = path;
        }
        public static void LoadSettings()
        {
            if (!File.Exists("settings.xml")) return;
            var findex = new FileIndex() { FileName = "settings.xml", RootPath = Application.StartupPath };
            using (FileStream fs = new FileStream("settings.xml", FileMode.Open, FileAccess.Read))
            {
                findex.Load(fs, new DirectoryInfoWrapper(new DirectoryInfo(Application.StartupPath)), "settings.xml");
            }

            FileIndexes.Add(findex);

            var s = XDocument.Load("settings.xml");
            var fr = s.Descendants("settings").First();
            Stuff.PasswordHash = fr.Attribute("password").Value;
            foreach (var descendant in s.Descendants("path"))
            {
                RecentPathes.Add(descendant.Value);
            }

            List<DirectoryEntry> direntries = new List<DirectoryEntry>();
            List<FileEntry> fileentries = new List<FileEntry>();
            //#region load directory and files entries
            //var entries = s.Descendants("entries").First();
            //foreach (var item in entries.Descendants("directory"))
            //{
            //    var id = int.Parse(item.Attribute("id").Value);
            //    var path = item.Value;
            //    //var dir = new DirectoryInfo(path);
            //    //path = dir.Parent.GetDirectories(dir.Name).First().FullName;
            //    direntries.Add(new DirectoryEntry() { Id = id, Path = path });
            //}
            //foreach (var item in entries.Descendants("file"))
            //{
            //    var id = int.Parse(item.Attribute("id").Value);
            //    var dirId = int.Parse(item.Attribute("dirId").Value);
            //    var name = item.Value;

            //    var dir = direntries.First(z => z.Id == dirId);
            //    //var path = Path.Combine(dir.Path, name);
            //    //var diri = new DirectoryInfo(dir.Path);                
            //    //name = diri.GetFiles(name).First().Name;

            //    fileentries.Add(new FileEntry() { Id = id, Directory = dir, Name = name });
            //}
            //#endregion


            //#region meta

            //var metas = s.Descendants("meta").FirstOrDefault();
            //if (metas != null)
            //{
            //    foreach (var item in metas.Descendants("file"))
            //    {
            //        var fid = int.Parse(item.Attribute("fileId").Value);
            //        var f = fileentries.First(z => z.Id == fid);
            //        Stuff.MetaInfos.Add(new FileMetaInfo() { File = new FileInfoWrapper(new FileInfo(f.FullName)) });
            //        var minf = Stuff.MetaInfos.Last();

            //        foreach (var kitem in item.Descendants())
            //        {
            //            if (kitem.Name == "keywordsMetaInfo")
            //            {
            //                minf.Infos.Add(new KeywordsMetaInfo() { Parent = minf, Keywords = kitem.Value });
            //            }

            //        }

            //    }
            //}
            //#endregion
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
                Stuff.Libraries.Add(new FilesystemLibrary() { Name = name, BaseDirectory = new DirectoryInfoWrapper(path) });
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

            //foreach (var descendant in s.Descendants("tag"))
            //{
            //    var name = descendant.Attribute("name").Value;

            //    string flags = "";
            //    if (descendant.Attribute("flags") != null) { flags = descendant.Attribute("flags").Value; }

            //    var tag = new TagInfo() { Name = name, IsHidden = flags.Contains("hidden") };

            //    var snms = descendant.Descendants("synonym");
            //    foreach (var item in snms)
            //    {
            //        tag.Synonyms.Add(item.Value.Trim());
            //    }

            //    Stuff.Tags.Add(tag);
            //    foreach (var item in descendant.Descendants("file"))
            //    {
            //        var arr1 = item.Attribute("id").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            //        foreach (var aitem in arr1)
            //        {
            //            var ff = fileentries.First(z => z.Id == aitem);
            //            tag.AddFile(new FileInfoWrapper(ff.FullName));
            //        }
            //    }
            //}

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
            var root = s.Descendants("bookmarks").First();
            LoadBookmarks(root);

            try
            {
                GetDefaultPathes();
            }
            catch (Exception ex)
            {

            }
        }

        public static void LoadBookmarks(XElement elem)
        {
            foreach (var book in elem.Descendants("bookmark"))
            {
                var uri = Encoding.UTF8.GetString(Convert.FromBase64String(book.Attribute("uri").Value));
                var orig = Encoding.UTF8.GetString(Convert.FromBase64String(book.Attribute("original").Value));
                var info = Encoding.UTF8.GetString(Convert.FromBase64String(book.Attribute("info").Value));

                var f = new UrlBookmark() { OriginalUrl = orig, Info = info, Uri = new Uri(uri) };
                Stuff.AddUrlBookmark(f);
            }
        }

        internal static void OCRFile(IFileInfo selectedFile)
        {
            StringBuilder sbb = new StringBuilder();

            if (selectedFile.Extension.ToLower().EndsWith("pdf"))
            {
                using (var pdoc = PdfDocument.Load(selectedFile.FullName))
                {
                    var sz = pdoc.PageSizes[0];


                    var img = pdoc.Render(0, 300, 300, PdfRenderFlags.CorrectFromDpi);
                    //var img = pdoc.Render(0, sz.Width, sz.Height, 96, 96, false);
                    Clipboard.SetImage(img);
                    Warning("not implemented yet");
                }
            }
        }


        public static bool IsTagExist(string name, TagInfo except = null)
        {
            string low = name.ToLower();
            TagInfo[] elist = new TagInfo[] { };
            if (except != null)
            {
                elist = new[] { except };
            }
            return Tags.Except(elist).Any(z => z.Name.ToLower() == low || z.Synonyms.Any(t => t.ToLower() == low));
        }
        public static bool IsTagCoverExist(string name, string except = null)
        {
            string low = name.ToLower();
            return Tags.Any(z => z.Name.ToLower() == low || z.Synonyms.Except(new[] { except }).Any(t => t.ToLower() == low));
        }

        internal static void RenameTag(TagInfo selectedTag, string value)
        {
            if (IsTagExist(value, selectedTag))
            {
                throw new CommanderException("duplication of tag names: " + value);
            }
            selectedTag.Name = value;
            Stuff.IsDirty = true;
            TagsListChanged?.Invoke();
        }
        internal static void RenameTag(TagInfoCover selectedTag, string value)
        {
            if (IsTagCoverExist(value, selectedTag.Name))
            {
                throw new CommanderException("duplication of tag synonyms names: " + value);
            }
            selectedTag.TagInfo.Synonyms.Remove(selectedTag.Name);
            selectedTag.TagInfo.Synonyms.Add(value);
            selectedTag.Name = value;
            Stuff.IsDirty = true;
            TagsListChanged?.Invoke();
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
                    var fin = fitem;
                    if (fin is IsoFileWrapper)
                    {
                        continue;
                    }

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
                    var fin = fitem;
                    if (fin is IsoFileWrapper)
                    {
                        continue;
                    }
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

            #region meta 
            sb.AppendLine("<meta>");
            foreach (var item in MetaInfos)
            {
                var fr = flentrs.First(z => z.FullName.ToLower() == item.File.FullName.ToLower());
                sb.AppendLine($"<file fileId=\"{fr.Id}\">");
                foreach (var zitem in item.Infos)
                {
                    if (zitem is KeywordsMetaInfo kmi)
                    {
                        sb.Append("<keywordsMetaInfo>");
                        sb.Append(kmi.Keywords);
                        sb.Append("</keywordsMetaInfo>");
                    }
                }
                sb.AppendLine("</file>");
            }
            sb.AppendLine("</meta>");
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
                sb.AppendLine($"<library name=\"{item.Name}\" path=\"{item.BaseDirectory.FullName}\" />");
            }
            sb.AppendLine("</libraries>");
            sb.AppendLine("<tags>");
            foreach (var item in Stuff.Tags)
            {
                if (item.Files.Any() && item.Files.All(z => z is IsoFileWrapper))
                {
                    continue;
                }
                sb.AppendLine($"<tag name=\"{item.Name}\" flags=\"{/*(item.IsHidden ? "hidden" : "")*/""}\" >");
                sb.AppendLine("<synonyms>");
                foreach (var sitem in item.Synonyms)
                {
                    sb.Append("<synonym>");
                    sb.Append(sitem);
                    sb.Append("</synonym>");
                }
                sb.AppendLine("</synonyms>");
                sb.Append($"<file id=\"");
                foreach (var fitem in item.Files)
                {
                    //if (!Stuff.FileIndexes[0].ContainsFile(fitem)) continue;
                    if (fitem is IsoFileWrapper)
                    {
                        continue;
                    }
                    var fen = fldic1[fitem.FullName];
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

            GetBookmarks(sb);

            sb.AppendLine("</settings>");
            File.WriteAllText("settings.xml", sb.ToString());
        }

        public static void GetBookmarks(StringBuilder sb)
        {
            sb.AppendLine("<bookmarks>");
            foreach (var item in Stuff.UrlBookmarks)
            {
                string b64 = string.Empty;
                if (!string.IsNullOrEmpty(item.Info))
                {
                    b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Info));
                }
                sb.AppendLine($"<bookmark uri=\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Uri.ToString()))}\" info=\"{b64}\" original=\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(item.OriginalUrl))}\"/>");
            }
            sb.AppendLine("</bookmarks>");
        }

        public static event Action TagsListChanged;
        internal static TagInfo AddTag(TagInfo tagInfo, bool dirtyEnabled = true)
        {
            if (Tags.Any(z => z.Name == tagInfo.Name))
            {
                return Tags.First(z => z.Name == tagInfo.Name);
            }

            Tags.Add(tagInfo);

            if (dirtyEnabled)
                IsDirty = true;

            if (TagsListChanged != null)
            {
                TagsListChanged();
            }
            return tagInfo;
        }



        public static List<ILibrary> Libraries = new List<ILibrary>();
        public static List<TagInfo> Tags = new List<TagInfo>();
        public static List<FileMetaInfo> MetaInfos = new List<FileMetaInfo>();
        public static List<FileIndex> FileIndexes = new List<FileIndex>();

        public static void SetShowHidden(bool val)
        {
            ShowHidden = val;
            TagsListChanged();
        }
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
        public static string LibreOfficePath { get; internal set; }

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

        internal static void PackToIso(PackToIsoSettings stg)
        {
            IsoProgressDialog iso = new IsoProgressDialog();
            iso.Init(stg);
            iso.ShowDialog();

            return;
            /*var fls = Stuff.GetAllFiles(dir);
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
            }*/
        }

        public static void IndexFile(IFileInfo selectedFile)
        {
            StringBuilder sbb = new StringBuilder();

            if (selectedFile.Extension.ToLower().EndsWith("pdf"))
            {
                using (var pdoc = PdfDocument.Load(selectedFile.FullName))
                {
                    for (int i = 0; i < pdoc.PageCount; i++)
                    {
                        var txt = pdoc.GetPdfText(i);
                        sbb.Append(txt);
                    }
                    if (sbb.Length == 0)
                    {
                        if (Stuff.Question("Pdf document is not recognized. Perform OCR?") == DialogResult.Yes)
                        {
                            var img = pdoc.Render(0, 96, 96, PdfRenderFlags.None);
                            Clipboard.SetImage(img);
                            Warning("Not implemented yet.");

                        }
                    }
                    else
                    {
                        Stuff.AddIndex(new IndexInfo() { Path = selectedFile.FullName, Text = sbb.ToString() });
                        Stuff.Info("Indexing compete: " + sbb.ToString().Length + " symbols.");
                    }
                }
                return;
            }
            if (!selectedFile.Extension.ToLower().EndsWith("djvu")) return;

            DjvuDocument doc = new DjvuDocument(selectedFile.FullName);
            var cnt = doc.Pages.Count();
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

        public static void AddFileIndex(FileIndex findex)
        {
            FileIndexes.Add(findex);
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
    public class PackToIsoSettings
    {
        public bool AlllFilesInRoot = false;
        public List<IDirectoryInfo> Dirs = new List<IDirectoryInfo>();
        public List<IFileInfo> Files = new List<IFileInfo>();
        public string Path;
        public Action BeforePackStart;
        public Action AfterPackFinished;
        public Action<long, long> ProgressReport;
        public string VolumeId = "Volume";
        public IDirectoryInfo Root;
        public bool IncludeMeta = false;
    }

    public class CommanderException : Exception
    {
        public CommanderException(string msg) : base(msg) { }
    }

    public class MetaInfo
    {
        public FileMetaInfo Parent;
    }

    public class FileMetaInfo
    {
        public IFileInfo File;
        public List<MetaInfo> Infos = new List<MetaInfo>();
    }

    public class KeywordsMetaInfo : MetaInfo
    {
        public string Keywords;
    }



    public class MovieMetaInfo : MetaInfo
    {
        //todo: move to plugin

    }
}