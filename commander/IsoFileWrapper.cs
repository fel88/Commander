using System;
using System.IO;
using System.Linq;
using System.Text;

namespace commander
{
    public class IsoFileWrapper : IFileInfo
    {
        public MountInfo MountInfo;
        public IsoFileWrapper(MountInfo minfo,  isoViewer.DirectoryRecord ff)
        {
            MountInfo = minfo;
            Reader = minfo.Reader;
            record = ff;
            isoViewer.DirectoryRecord rec = ff;
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
            fullName = sb.ToString();
            ext = Path.GetExtension(fullName);

        }
        string ext;
        string fullName;
        isoViewer.DirectoryRecord record;


        public isoViewer.IsoReader Reader;

        public string FullName => fullName;

        public string Name => record.Name;

        public string DirectoryName => new FileInfo(fullName).DirectoryName;

        public string Extension => ext;

        public DateTime LastWriteTime => DateTime.Now;

        public long Length => record.DataLength;

        public IDirectoryInfo Directory => new IsoDirectoryInfoWrapper(MountInfo, record.Parent);

        public FileAttributes Attributes => FileAttributes.ReadOnly;
    }
}
