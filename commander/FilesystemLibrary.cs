using System.IO;

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
    }
}
