using IsoLib;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace commander
{
    public class IsoFileWrapper : IFileInfo
    {
        public MountInfo MountInfo;
        public IsoFileWrapper(MountInfo minfo, DirectoryRecord ff)
        {
            MountInfo = minfo;
            Reader = minfo.Reader;
            record = ff;
            DirectoryRecord rec = ff;
            StringBuilder sb = new StringBuilder();
            while (rec != null)
            {
                if (rec == Reader.Pvds.Last().RootDir) break;
                if (rec.IsFile)
                {
                    sb.Insert(0, rec.Name);
                }
                else
                {
                    sb.Insert(0, rec.Name + "\\");

                }
                rec = rec.Parent;
            }
            fullName = sb.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First();
            name = Path.GetFileName(fullName);
            ext = Path.GetExtension(fullName);

        }
        string ext;
        string fullName;
        string name;
        DirectoryRecord record;


        public IsoReader Reader;

        public string FullName => fullName;

        public string Name => name;

        public string DirectoryName => new FileInfo(fullName).DirectoryName;

        public string Extension => ext;

        public DateTime LastWriteTime => DateTime.Now;

        public long Length => record.DataLength;

        public IDirectoryInfo Directory { get; set; }

        public FileAttributes Attributes => FileAttributes.ReadOnly;

        public bool Exist => true;

        public IFilesystem Filesystem { get; set; }
    }
}
