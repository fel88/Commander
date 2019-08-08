using System.IO;

namespace commander
{
    public class DiskFilesystem : IFilesystem
    {
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