using PluginLib;
using System;
using System.IO;

namespace commander
{
    public class FileInfoWrapper : IFileInfo
    {
        public FileInfoWrapper(FileInfo ff)
        {
            FileInfo = ff;
            dwrapper = new DirectoryInfoWrapper(ff.Directory);
        }

        public FileInfoWrapper(string finfo)
        {
            FileInfo = new FileInfo(finfo);
            dwrapper = new DirectoryInfoWrapper(FileInfo.Directory);
        }

        public FileInfo FileInfo;

        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;

        public string Extension => FileInfo.Extension;

        public DateTime LastWriteTime => FileInfo.LastWriteTime;

        public long Length => FileInfo.Length;

        public string DirectoryName => FileInfo.DirectoryName;

        IDirectoryInfo dwrapper;
        private string finfo;

        public IDirectoryInfo Directory => dwrapper;

        public FileAttributes Attributes => File.GetAttributes(FullName);

        public bool Exist => FileInfo.Exists;

        public IFilesystem Filesystem => Directory.Filesystem;
    }
}
