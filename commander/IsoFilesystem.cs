using IsoLib;
using isoViewer;
using System.Drawing;
using System.IO;
using System.Linq;

namespace commander
{
    public class IsoFilesystem : IFilesystem
    {
        public bool IsReadOnly => true;

        public IFileInfo IsoFileInfo { get; internal set; }

        public Image BitmapFromFile(string fullName)
        {

            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == fullName.ToLower());


                var dat = fr.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);

                return Bitmap.FromStream(ms);
            }
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

        public bool FileHasTag(IFileInfo file, TagInfo tag)
        {
            throw new System.NotImplementedException();
        }

        public Stream OpenReadOnlyStream(IFileInfo file)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == file.FullName.ToLower());


                var dat = fr.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);

                return ms;
            }
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] ReadAllLines(string fullName)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string fullName)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(IFileInfo file)
        {
            using (var fs = new FileStream(IsoFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IsoReader reader = new IsoReader();
                reader.Parse(fs);
                var ret = DirectoryRecord.GetAllRecords(reader.WorkPvd.RootDir);
                var fr = ret.First(x => x.IsFile && x.FullPath.ToLower() == file.FullName.ToLower());


                var dat = fr.GetFileData(fs, reader.WorkPvd);
                MemoryStream ms = new MemoryStream(dat);
                var rdr = new StreamReader(ms);
                return rdr.ReadToEnd();
            }
        }

        public void WriteAllText(IFileInfo fileInfo, string text)
        {
            throw new System.NotImplementedException();
        }
    }

}