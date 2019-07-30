using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace isoViewer
{
    public class IsoReader
    {
        public PVD[] Pvds;
        public string Path;

        public PVD WorkPvd
        {
            get
            {
                return Pvds.Last();
            }
        }

        public void Parse(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Path = path;
                Parse(fs);
            }
        }

        public void Parse(FileStream fs)
        {
            fs.Seek(0x8000, SeekOrigin.Begin);
            List<PVD> list = new List<PVD>();
            PVD p;
            while (true)
            {
                p = new PVD();

                p.Parse(fs);
                if (p.Type == 0xff) break;
                list.Add(p);
            }
            DirectoryRecord.ReadRecursive(fs, list.Last().RootDir, list.Last());
            Pvds = list.ToArray();
        }
    }
}