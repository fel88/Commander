using System.Drawing;
using System.IO;

namespace commander
{
    public class DiskFilesystem : IFilesystem
    {
        public Image BitmapFromFile(string fullName)
        {
            return Bitmap.FromFile(fullName);
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