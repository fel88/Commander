using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace commander
{
    public static class DefaultIcons
    {
        private static readonly Lazy<Icon> _lazyFolderIcon = new Lazy<Icon>(FetchIcon, true);
        private static readonly Lazy<Icon> _lazyFolderIconSelected = new Lazy<Icon>(FetchIconSelected, true);

        public static Icon FolderLarge
        {
            get { return _lazyFolderIcon.Value; }
        }
        public static Icon FolderLargeSelected
        {
            get { return _lazyFolderIconSelected.Value; }
        }

        private static Icon FetchIcon()
        {
            var tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var icon = ExtractFromPath(tmpDir);
            Directory.Delete(tmpDir);
            return icon;
        }
        private static Icon FetchIconSelected()
        {
            var tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var icon = ExtractFromPath2(tmpDir);
            Directory.Delete(tmpDir);
            return icon;
        }

        private static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }
        private static Icon ExtractFromPath2(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON | SHGFI_SELECTED);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }
        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SELECTED = 0x000010000;
        private const uint SHGFI_SMALLICON = 0x000000001;
    }
}