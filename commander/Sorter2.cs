using System.Collections;
using System.Windows.Forms;

namespace commander
{
    public class Sorter2 : IComparer
    {
        private SortOrder sorting;

        public Sorter2(SortOrder sorting)
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
            var r1 = lv1.SubItems[0].Text;
            var r2 = lv2.SubItems[0].Text;
            var res = r1.CompareTo(r2);
            if (sorting == SortOrder.Descending)
            {
                res = -res;
            }
            return res;

        }


    }
}
