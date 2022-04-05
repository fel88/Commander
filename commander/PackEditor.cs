using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public partial class PackEditor : Form
    {
        public PackEditor()
        {
            InitializeComponent();
        }

        internal void Init(List<IFileInfo> flss)
        {
            listView1.Items.Clear();
            foreach (var item in flss.OrderBy(z => z.FullName.ToLower()))
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.FullName }) { Tag = item });
            }
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {

        }
    }
}
