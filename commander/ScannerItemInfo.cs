using PluginLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace commander
{
    public class ScannerItemInfo
    {
        public float R1;
        public float R2;
        public float StartAng;
        public float EndAng;
        public long Size;
        public long HiddenItemsSize;
        public List<ScannerItemInfo> Items = new List<ScannerItemInfo>();

        public virtual string Name { get; set; }

        public void CalcSize()
        {
            if (Items.Count == 0) return;
            Size = HiddenItemsSize;
            foreach (var item in Items)
            {
                item.CalcSize();
                Size += item.Size;
            }
        }
    }
    public class ScannerDirInfo : ScannerItemInfo
    {

        public ScannerDirInfo(ScannerDirInfo prnt, IDirectoryInfo d)
        {
            Parent = prnt;
            Dir = d;
            Name = d.Name;
        }
        public ScannerDirInfo Parent;

        public IDirectoryInfo Dir;
        public override string Name { get; set; }

        public string GetDirFullName()
        {
            ScannerDirInfo d = this;
            StringBuilder sb = new StringBuilder();

            while (d != null)
            {
                if (d.Parent == null)
                {
                    sb.Insert(0, d.Dir.FullName + "\\");
                }
                else
                {
                    sb.Insert(0, d.Name + "\\");
                }
                d = d.Parent;
            }


            return sb.ToString();

        }        
    }

    public class ScannerFileInfo : ScannerItemInfo
    {
        public ScannerFileInfo(ScannerDirInfo prnt,IFileInfo f)
        {
            Parent = prnt;
            Name = f.Name;
            Size = f.Length;
        }
        public ScannerDirInfo Parent;
        public override string Name { get; set; }
        
    }
}
