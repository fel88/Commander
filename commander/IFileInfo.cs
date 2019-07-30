using System;
using System.Collections.Generic;
using System.IO;

namespace commander
{
    public interface IFileInfo
    {
        string FullName { get; }
        string Name { get; }
        string DirectoryName { get; }
        string Extension { get; }
        DateTime LastWriteTime { get; }
        long Length { get; }
        IDirectoryInfo Directory { get; }
        FileAttributes Attributes { get; }
    }

    public interface IDirectoryInfo
    {
        string FullName { get; }
        string Name { get; }

         IDirectoryInfo Parent { get; }

        IEnumerable<IDirectoryInfo> GetDirectories();

        IEnumerable<IFileInfo> GetFiles();
       
        DateTime LastWriteTime { get; }
        bool Exists { get; }
    }

}
