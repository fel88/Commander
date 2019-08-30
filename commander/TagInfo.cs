﻿using System;
using System.Collections.Generic;

namespace commander
{
    public class TagInfo
    {
        public string Name;
        public bool IsHidden;
        private List<IFileInfo> files = new List<IFileInfo>();
        private HashSet<string> hash = new HashSet<string>();
        public IEnumerable<IFileInfo> Files
        {
            get
            {
                return files.ToArray();
            }
        }
        public bool ContainsFile(string fn)
        {
            return hash.Contains(fn.ToLower());
        }
        public bool ContainsFile(IFileInfo fn)
        {
            return fn.Filesystem.FileHasTag(fn, this);
        }
        public void AddFile(IFileInfo fn)
        {
            if (!ContainsFile(fn))
            {
                files.Add(fn);
                hash.Add(fn.FullName.ToLower());
                if (!(fn is IsoFileWrapper))
                {
                    Stuff.IsDirty = true;
                }
            }
        }

        public void DeleteFile(string fullName)
        {
            var fl = fullName.ToLower();
            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].FullName.ToLower() == fl)
                {
                    files.RemoveAt(i);
                    hash.Remove(fl);
                    break;
                }
            }
            Stuff.IsDirty = true;
        }


    }
}