using System;

namespace commander
{
    public interface IFileListControl
    {
        event Action<IFileInfo> SelectedFileChanged;
    }
}
