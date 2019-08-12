using System;

namespace commander
{
    public interface IFileListControl 
    {
        event Action<IFileInfo> SelectedFileChanged;
        IFileInfo SelectedFile { get; }
        void ParentClosing();
    }
}
