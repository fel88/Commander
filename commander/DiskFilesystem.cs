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

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string[] ReadAllLines(string fullName)
        {
            return File.ReadAllLines(fullName);
        }
    }
  
}