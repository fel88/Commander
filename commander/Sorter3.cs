using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace commander
{
    public class Sorter3 : IComparer
    {
        private SortOrder sorting;

        public Sorter3(SortOrder sorting)
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
            long sz1 = 0;
            long sz2 = 0;
            if (lv1.Tag is IFileInfo)
            {
                var fin = lv1.Tag as IFileInfo;
                sz1 = fin.Length;
            }
            if (lv2.Tag is IFileInfo)
            {
                var fin = lv2.Tag as IFileInfo;
                sz2 = fin.Length;
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
