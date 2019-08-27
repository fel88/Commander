using System.Drawing;
using System.IO;

namespace commander
{
    public class DiskFilesystem : IFilesystem
    {
        public bool IsReadOnly { get; set; } 

        public Image BitmapFromFile(string fullName)
        {
            return Bitmap.FromFile(fullName);
        }

        public void DeleteDirectory(IDirectoryInfo item, bool recursive)
        {
            Directory.Delete(item.FullName, recursive);
        }

        public void DeleteFile(string fullName)
        {
            File.Delete(fullName);
        }

        public void DeleteFile(IFileInfo file)
        {
            File.Delete(file.FullName);
        }

        public bool FileHasTag(IFileInfo file, TagInfo tag)
        {
            return tag.ContainsFile(file.FullName);
        }

        public Stream OpenReadOnlyStream(IFileInfo file)
        {
            return new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string[] ReadAllLines(string fullName)
        {
            return File.ReadAllLines(fullName);
        }

        public string ReadAllText(string fullName)
        {
            return File.ReadAllText(fullName);
        }

        public string ReadAllText(IFileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        public void WriteAllText(IFileInfo fileInfo, string text)
        {
            File.WriteAllText(fileInfo.FullName, text);
        }
    }

}