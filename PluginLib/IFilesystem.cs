using System;
using System.Drawing;
using System.IO;

namespace PluginLib
{
    public interface IFilesystem
    {
        bool IsReadOnly { get; }
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string fullName);
        string ReadAllText(string fullName);
        string ReadAllText(IFileInfo file);
        Image BitmapFromFile(IFileInfo fullName);

        bool FileHasTag(IFileInfo file, ITagInfo tag);

        void DeleteFile(string fullName);
        void DeleteFile(IFileInfo file);
        void DeleteDirectory(IDirectoryInfo item, bool recursive);
        Stream OpenReadOnlyStream(IFileInfo file);
        void WriteAllText(IFileInfo fileInfo, string text);

        bool FileExist(string path );
    }

}