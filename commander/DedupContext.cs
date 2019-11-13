using PluginLib;
using System.Collections.Generic;
using System.Linq;

namespace commander
{
    public class DedupContext
    {
        public IDirectoryInfo[] Dirs;
        public IFileInfo[] Files;
        public bool EnablePartCheck;
        public int SizeCheckPart;


        public DedupContext(IDirectoryInfo[] directoryInfo, IFileInfo[] fileInfo)
        {
            this.Dirs = directoryInfo;
            this.Files = fileInfo;
        }

        public IFileInfo[] GetAllFiles()
        {
            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var d in Dirs)
            {
                Stuff.GetAllFiles(d, files);
            }
            files.AddRange(Files);
            files = files.Where(z => z.Exist).ToList();
            return files.ToArray();
        }
    }
}
