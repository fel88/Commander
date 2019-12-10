using PluginLib;

namespace commander
{
    public class DedupContext : FilesAndDirectoriesContext
    {
        public DedupContext(IDirectoryInfo[] directoryInfo, IFileInfo[] fileInfo) : base(directoryInfo, fileInfo)
        {
        }
        public bool EnablePartCheck;
        public int SizeCheckPart;
       
    }
}
