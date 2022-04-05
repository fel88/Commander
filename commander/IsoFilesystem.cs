using IsoLib;
using isoViewer;
using PluginLib;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace commander
{
    public class IsoFilesystem : IFilesystem
    {
        public IsoFilesystem(MountInfo m)
        {
            mountInfo = m;
        }

        MountInfo mountInfo;
        public bool IsReadOnly => true;

        public IFileInfo IsoFileInfo { get; internal set; }

        public Image BitmapFromFile(IFileInfo file)
        {
            return Bitmap.FromStream(OpenReadOnlyStream(file));
        }
        public void DeleteDirectory(IDirectoryInfo item, bool recursive)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFile(string fullName)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFile(IFileInfo file)
        {
            throw new System.NotImplementedException();
        }

        public bool FileExist(string path)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                return ret.Any(x => x.IsFile && (x.FullPath.ToLower() == path.ToLower() || Path.Combine(mountInfo.MountTarget.FullName, x.FullPath).ToLower() == path.ToLower()));
            }
        }

        public IFileInfo GetFile(string path)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                var aa = ret.First(x => x.IsFile && x.FullPath.ToLower() == path.ToLower());
                return new IsoFileWrapper(new MountInfo() { IsoPath = IsoFileInfo }, aa) { Filesystem = this };
            }
        }

        public bool FileHasTag(IFileInfo file, ITagInfo tag)
        {
            return tag.ContainsFile(file.FullName);
        }

        public Stream OpenReadOnlyStream(IFileInfo file)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                //var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                //var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == file.FullName.ToLower());


                var dat = (file as IsoFileWrapper).record.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);

                return ms;
            }
        }

        public byte[] ReadAllBytes(string path)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);

                var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == path.ToLower());

                var dat = fr.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);
                return ms.ToArray();
            }
        }

        public string[] ReadAllLines(string fullName)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string fullName)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);


                var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == fullName.ToLower());


                var dat = fr.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);
                var rdr = new StreamReader(ms);
                return rdr.ReadToEnd();
            }
        }

        public string ReadAllText(IFileInfo file)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                //var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                //var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == file.FullName.ToLower());

                var dat = (file as IsoFileWrapper).record.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);
                var rdr = new StreamReader(ms);
                return rdr.ReadToEnd();
            }
        }

        public void WriteAllText(IFileInfo fileInfo, string text)
        {
            throw new System.NotImplementedException();
        }

        public void Run(IFileInfo file)
        {
            Directory.CreateDirectory("temp");
            var fn = Path.Combine("temp", file.Name);
            using (var stream = OpenReadOnlyStream(file))
            {
                using (var fs = new FileStream(fn, FileMode.Create))
                {
                    stream.CopyTo(fs);
                }
            }
            Process.Start(fn);
        }

        public string ReadAllText(IFileInfo file, Encoding encoding)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                //var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                //var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == file.FullName.ToLower());

                var dat = (file as IsoFileWrapper).record.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);
                var rdr = new StreamReader(ms, encoding);
                return rdr.ReadToEnd();
            }
        }
    }
}