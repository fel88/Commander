using PluginLib;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace commander
{
    public class DiskFilesystem : IFilesystem
    {
        public bool IsReadOnly { get; set; }

        public Image BitmapFromFile(IFileInfo file)
        {
            return Bitmap.FromFile(file.FullName);
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

        public bool FileExist(string path)
        {
            return File.Exists(path);
        }

        public bool FileHasTag(IFileInfo file, ITagInfo tag)
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

        public string ReadAllText(IFileInfo file, System.Text.Encoding encoding)
        {            
            return File.ReadAllText(file.FullName, encoding);
        }

        public string ReadAllText(IFileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        public void Run(IFileInfo file)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WorkingDirectory = file.DirectoryName;
            psi.FileName = file.FullName;
            Process.Start(psi);
        }

        public void WriteAllText(IFileInfo fileInfo, string text)
        {
            File.WriteAllText(fileInfo.FullName, text);
        }
    }

}