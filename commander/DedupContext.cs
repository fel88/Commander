using PluginLib;

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
    }
}
