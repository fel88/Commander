using System;
using System.IO;

namespace commander
{
    public class VirtualFileInfo : IFileInfo
    {
        public FileInfo FileInfo;
        

        public VirtualFileInfo(FileInfo f,IDirectoryInfo dd)
        {
            Directory = dd;
            FileInfo = f;
        }

        public string FullName => Path.Combine(Directory.FullName, FileInfo.Name);

        public string Name => FileInfo.Name;

        public string DirectoryName => Directory.Name;

        public string Extension => FileInfo.Extension;

        public DateTime LastWriteTime => FileInfo.LastWriteTime;

        public long Length => FileInfo.Length;

        public IDirectoryInfo Directory { get; set; }

        public FileAttributes Attributes => FileInfo.Attributes;

        public bool Exist => FileInfo.Exists;

        public IFilesystem Filesystem => Directory.Filesystem;
    }
}