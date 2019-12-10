using PluginLib;
using System.Collections.Generic;
using System.Linq;

namespace commander
{
    public class FilesAndDirectoriesContext
    {
        public IDirectoryInfo[] Dirs;
        public IFileInfo[] Files;

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

        public FilesAndDirectoriesContext(IDirectoryInfo[] directoryInfo, IFileInfo[] fileInfo)
        {
            this.Dirs = directoryInfo;
            this.Files = fileInfo;
        }

    }
}
