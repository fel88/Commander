using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace commander
{
    public class DirectoryInfoWrapper : IDirectoryInfo
    {
        public DirectoryInfoWrapper(DirectoryInfo ff)
        {
            DirectoryInfo = ff;
        }

        public DirectoryInfoWrapper(string baseDirectory)
        {
            DirectoryInfo = (new DirectoryInfo(baseDirectory));
        }

        public DirectoryInfo DirectoryInfo;
        

        public string FullName => DirectoryInfo.FullName;

        public string Name => DirectoryInfo.Name;

        public DateTime LastWriteTime => DirectoryInfo.LastWriteTime;

        public IDirectoryInfo Parent => new DirectoryInfoWrapper(DirectoryInfo.Parent);

        public bool Exists => DirectoryInfo.Exists;

        public IDirectoryInfo Root => new DirectoryInfoWrapper( DirectoryInfo.Root);

        public IFilesystem Filesystem => Stuff.DefaultFileSystem;

        public IEnumerable<IDirectoryInfo> GetDirectories()
        {
            return DirectoryInfo.GetDirectories().Select(z => new DirectoryInfoWrapper(z));
        }

        public IEnumerable<IFileInfo> GetFiles()
        {
            return DirectoryInfo.GetFiles().Select(z => new FileInfoWrapper(z));
        }
    }
}
