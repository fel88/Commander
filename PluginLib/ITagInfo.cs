﻿using System.Collections.Generic;

namespace PluginLib
{
    public interface ITagInfo
    {
        string Name { get; }
        IEnumerable<IFileInfo> Files { get; }
        bool ContainsFile(IFileInfo fn);
        bool ContainsFile(string fn);
    }
}
