﻿using System;
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
            
            if (lv1.Tag is FileInfo)
            {
                var fin = lv1.Tag as FileInfo;
                dt1 = fin.LastWriteTime;
            }
            if (lv1.Tag is DirectoryInfo)
            {
                var fin = lv1.Tag as DirectoryInfo;
                dt1 = fin.LastWriteTime;
            }
            if (lv2.Tag is FileInfo)
            {
                var fin = lv2.Tag as FileInfo;
                dt2 = fin.LastWriteTime;
            }
            if (lv2.Tag is DirectoryInfo)
            {
                var fin = lv2.Tag as DirectoryInfo;
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
