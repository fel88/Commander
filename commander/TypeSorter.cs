﻿using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace commander
{
    public class TypeSorter : IComparer
    {
        private SortOrder sorting;

        public TypeSorter(SortOrder sorting)
        {
            this.sorting = sorting;
        }

        public int Compare(object x, object y)
        {
            var lv1 = x as ListViewItem;
            var lv2 = y as ListViewItem;

            if (lv1.SubItems[0].Text == "..")
            {
                return -1;
            }
            if (lv2.SubItems[0].Text == "..")
            {
                return 1;
            }
            string sz1 = "";
            string sz2 = "";
            if (lv1.Tag is FileInfo)
            {
                var fin = lv1.Tag as FileInfo;
                sz1 = fin.Extension;
            }
            if (lv2.Tag is FileInfo)
            {
                var fin = lv2.Tag as FileInfo;
                sz2 = fin.Extension;
            }

            var res = sz1.CompareTo(sz2);
            if (sorting == SortOrder.Descending)
            {
                res = -res;
            }
            return res;

        }


    }
}
