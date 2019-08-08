using System;
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
        bool Exist { get; }

        IFilesystem Filesystem { get; }
    }

}
