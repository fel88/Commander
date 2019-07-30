namespace commander
{
    public class MountInfo
    {
        public string Path;
        public string FullPath
        {
            get
            {
                return System.IO.Path.Combine(Path, IsoPath.Name);
            }
        }
        public IFileInfo IsoPath;
        public isoViewer.IsoReader Reader;
    }
}
