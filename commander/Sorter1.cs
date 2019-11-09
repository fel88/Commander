using PluginLib;
using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace commander
{
    public class Sorter1 : IComparer
    {
        private SortOrder sorting;

        public Sorter1(SortOrder sorting)
        {
            this.sorting = sorting;
        }

        public int Compare(object x, object y)
        {
            var lv1 = x as ListViewItem;
            var lv2 = y as ListViewItem;
            DateTime dt1 = DateTime.Now;
            DateTime dt2 = DateTime.Now;
            if (lv1.SubItems[0].Text == "..")
            {
                return -1;
            }
            if (lv2.SubItems[0].Text == "..")
            {
                return 1;
            }
            
            if (lv1.Tag is IFileInfo)
            {
                var fin = lv1.Tag as IFileInfo;
                dt1 = fin.LastWriteTime;
            }
            if (lv1.Tag is IDirectoryInfo)
            {
                var fin = lv1.Tag as IDirectoryInfo;
                dt1 = fin.LastWriteTime;
            }
            if (lv2.Tag is IFileInfo)
            {
                var fin = lv2.Tag as IFileInfo;
                dt2 = fin.LastWriteTime;
            }
            if (lv2.Tag is IDirectoryInfo)
            {
                var fin = lv2.Tag as IDirectoryInfo;
                dt2 = fin.LastWriteTime;
            }
            var res = dt1.CompareTo(dt2);
            if (sorting == SortOrder.Descending)
            {
                res = -res;
            }
            return res;

        }


    }
}
