using System;
using System.Drawing;
using System.IO;

namespace commander
{
    public interface IFilesystem
    {
        bool IsReadOnly { get; }
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string fullName);
        string ReadAllText(string fullName);
        string ReadAllText(IFileInfo file);
        Image BitmapFromFile(string fullName);

        bool FileHasTag(IFileInfo file, TagInfo tag);

        void DeleteFile(string fullName);
        void DeleteFile(IFileInfo file);
        void DeleteDirectory(IDirectoryInfo item, bool recursive);
        Stream OpenReadOnlyStream(IFileInfo file);
        void WriteAllText(IFileInfo fileInfo, string text);
    }

}