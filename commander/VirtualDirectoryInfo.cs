using System;
using System.Collections.Generic;

namespace commander
{
    public class VirtualDirectoryInfo : IDirectoryInfo
    {
        public string FullName { get; set; }

        public string Name {get;set;}

        public IDirectoryInfo Root => this;

        public IDirectoryInfo Parent { get; set; }

        public DateTime LastWriteTime { get; set; }

        public bool Exists { get; set; } = true;

        public List<IDirectoryInfo> ChildsDirs = new List<IDirectoryInfo>();
        public List<IFileInfo> ChildsFiles = new List<IFileInfo>();
        public IEnumerable<IDirectoryInfo> GetDirectories()
        {
            return ChildsDirs;
        }

        public IEnumerable<IFileInfo> GetFiles()
        {
            return ChildsFiles;
        }
    }
}
