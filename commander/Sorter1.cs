using System;
using System.Collections;
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
            DateTime.TryParse(lv1.SubItems[2].Text, out dt1);
            DateTime.TryParse(lv2.SubItems[2].Text, out dt2);
            var res = dt1.CompareTo(dt2);
            if (sorting == SortOrder.Descending)
            {
                res = -res;
            }
            return res;

        }


    }
}
