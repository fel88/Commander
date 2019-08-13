using System;
using System.Collections.Generic;

namespace commander
{
    public class TagInfo
    {
        public string Name;
        public bool IsHidden;
        public List<string> Files = new List<string>();

        public bool ContainsFile(string fn)
        {
            return Files.Contains(fn.ToLower());
        }
        public void AddFile(string fn)
        {
            if (!ContainsFile(fn))
            {
                Stuff.IsDirty = true;
                Files.Add(fn.ToLower());
            }
        }

        public void DeleteFile(string fullName)
        {
            Files.Remove(fullName);
            Stuff.IsDirty = true;
        }

        
    }
}