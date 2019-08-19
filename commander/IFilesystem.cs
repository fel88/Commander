using System.Drawing;

namespace commander
{
    public interface IFilesystem
    {
        bool IsReadOnly { get; }
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string fullName);
        Image BitmapFromFile(string fullName);

        void DeleteFile(string fullName);
        void DeleteFile(IFileInfo file);
        void DeleteDirectory(IDirectoryInfo item, bool recursive);
    }

}