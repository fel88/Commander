using System;
using System.Collections.Generic;

namespace commander
{
    public interface IDirectoryInfo
    {
        string FullName { get; }
        string Name { get; }

        IDirectoryInfo Root { get; }

        
        IDirectoryInfo Parent { get; }

        IEnumerable<IDirectoryInfo> GetDirectories();

        IEnumerable<IFileInfo> GetFiles();

        DateTime LastWriteTime { get; }
        bool Exists { get; }
    }

}
