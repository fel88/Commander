using System;
using System.Collections.Generic;
using System.IO;

namespace commander
{
    public class IsoDirectoryInfoWrapper : IDirectoryInfo
    {
        public MountInfo MountInfo;
        public IsoDirectoryInfoWrapper(MountInfo minfo, isoViewer.DirectoryRecord ff)
        {
            MountInfo = minfo;
            Reader = minfo.Reader;
            if (ff == minfo.Reader.WorkPvd.RootDir)
            {
                _name = minfo.IsoPath.Name;
            }
            else
            {
                _name = ff.Name;
            }
            record = ff;
        }
        isoViewer.DirectoryRecord record;

        string _name;
        public isoViewer.IsoReader Reader;

        public string FullName => Path.Combine(MountInfo.Path, _name);

        public string Name => _name;

        public DateTime LastWriteTime => MountInfo.IsoPath.LastWriteTime;

        public IDirectoryInfo Parent
        {
            get
            {
                if (record.Parent == null)
                {
                    return new DirectoryInfoWrapper(MountInfo.Path);
                }
                else
                {
                    return new IsoDirectoryInfoWrapper(MountInfo, record.Parent);
                }
            }
        }

        public bool Exists => true;

        public IDirectoryInfo Root => new DirectoryInfoWrapper(MountInfo.Path);
                

        public IEnumerable<IDirectoryInfo> GetDirectories()
        {
            foreach (var item in record.Records)
            {
                if (item.LBA == record.LBA) continue;
                if (record.Parent != null && item.LBA == record.Parent.LBA) continue;
                if (!item.IsDirectory) continue;

                yield return new IsoDirectoryInfoWrapper(MountInfo, item);
            }
        }

        public IEnumerable<IFileInfo> GetFiles()
        {
            foreach (var item in record.Records)
            {
                if (item.LBA == record.LBA) continue;
                if (!item.IsFile) continue;
                yield return new IsoFileWrapper(MountInfo, item);
            }
        }
    }
}
