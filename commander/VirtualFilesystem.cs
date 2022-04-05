using PluginLib;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace commander
{
    public class VirtualFilesystem : IFilesystem
    {
        public List<VirtualFileInfo> Files = new List<VirtualFileInfo>();
        public byte[] ReadAllBytes(string path)
        {
            var fr = Files.First(z => z.FullName == path);
            return File.ReadAllBytes(fr.FileInfo.FullName);
        }

        public bool UseIndexes = false;

        public bool IsReadOnly { get; set; }

        public string[] ReadAllLines(string path)
        {
            var fr = Files.First(z => z.FullName == path);
            if (UseIndexes)
            {
                var frr = Stuff.Indexes.FirstOrDefault(z => z.Path == fr.FileInfo.FullName);
                if (frr != null)
                {
                    return frr.Text.Split('\n');
                }
            }
            return File.ReadAllLines(fr.FileInfo.FullName);
        }

        public Image BitmapFromFile(IFileInfo file)
        {
            var fr = Files.First(z => z.FullName == file.FullName.ToLower());
            return Bitmap.FromFile(fr.FileInfo.FullName);
        }

        public void DeleteFile(string fullName)
        {
            var fr = Files.First(z => z.FullName == fullName);
            File.Delete(fr.FileInfo.FullName);
        }

        public void DeleteFile(IFileInfo file)
        {
            var fr = Files.First(z => z.FullName == file.FullName);
            File.Delete(fr.FileInfo.FullName);
        }

        public void DeleteDirectory(IDirectoryInfo item, bool v)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            var fr = Files.First(z => z.FullName == path);
            if (UseIndexes)
            {
                var frr = Stuff.Indexes.FirstOrDefault(z => z.Path == fr.FileInfo.FullName);
                if (frr != null)
                {
                    return frr.Text;
                }
            }
            return File.ReadAllText(fr.FileInfo.FullName, encoding);
        }

        public string ReadAllText(string path)
        {

            var fr = Files.First(z => z.FullName == path);
            if (UseIndexes)
            {
                var frr = Stuff.Indexes.FirstOrDefault(z => z.Path == fr.FileInfo.FullName);
                if (frr != null)
                {
                    return frr.Text;
                }
            }
            return File.ReadAllText(fr.FileInfo.FullName);
        }

        public bool FileHasTag(IFileInfo file, ITagInfo tag)
        {
            var vf = file as VirtualFileInfo;
            return tag.ContainsFile(vf.FileInfo.FullName);
        }

        public Stream OpenReadOnlyStream(IFileInfo file)
        {
            return new FileStream((file as VirtualFileInfo).FileInfo.FullName, FileMode.Open, FileAccess.Read);
        }

        public VirtualFileInfo ToVirtualFileInfo(IFileInfo f)
        {
            return f as VirtualFileInfo;
        }

        public string ReadAllText(IFileInfo file)
        {
            return File.ReadAllText(ToVirtualFileInfo(file).FileInfo.FullName);
        }

        public void WriteAllText(IFileInfo fileInfo, string text)
        {
            File.WriteAllText(ToVirtualFileInfo(fileInfo).FileInfo.FullName, text);
        }

        public bool FileExist(string path)
        {
            return Files.Any(z => z.FullName.ToLower() == path.ToLower());
        }

        public void Run(IFileInfo file)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(IFileInfo file, Encoding encoding)
        {
            return File.ReadAllText(ToVirtualFileInfo(file).FileInfo.FullName, encoding);

        }
    }
}
