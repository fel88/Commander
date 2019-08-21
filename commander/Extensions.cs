using System.Windows.Forms;

namespace commander
{
    public static class Extensions
    {
        public static bool IsItemVisible(this ListView lv, ListViewItem item)
        {
            return item.Bounds.IntersectsWith(lv.ClientRectangle);
        }
    }

}

