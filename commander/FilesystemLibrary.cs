using PluginLib;
using System.IO;
using System.Linq;

namespace commander
{
    public class FilesystemLibrary : ILibrary
    {
        public IDirectoryInfo BaseDirectory;

        public string Name { get; set; }

        public void AppendFile(string path, byte[] data)
        {
            File.WriteAllBytes(Path.Combine(BaseDirectory.FullName, path), data);
        }

        public string[] EnumerateFiles()
        {
            var dir = new DirectoryInfoWrapper(BaseDirectory.FullName);
            return Stuff.GetAllFiles(dir).Select(z => z.FullName).ToArray();
        }

        public byte[] GetFile(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
