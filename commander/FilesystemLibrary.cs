using PluginLib;
using System.IO;
using System.Linq;

namespace commander
{
    public class FilesystemLibrary : ILibrary
    {
        public string BaseDirectory;

        public string Name { get; set; }

        public void AppendFile(string path, byte[] data)
        {
            File.WriteAllBytes(Path.Combine(BaseDirectory, path), data);
        }

        public string[] EnumerateFiles()
        {
            var dir = new DirectoryInfo(BaseDirectory);
            return Stuff.GetAllFiles(dir).Select(z => z.FullName).ToArray();
        }

        public byte[] GetFile(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
