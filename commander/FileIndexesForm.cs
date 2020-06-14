using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace commander
{
    public partial class FileIndexesForm : Form
    {
        public FileIndexesForm()
        {
            InitializeComponent();
            foreach (var item in Stuff.FileIndexes)
            {
                listView1.Items.Add(new ListViewItem(new string[] { Path.Combine(item.RootPath, item.FileName) }) { Tag = item });
            }
        }

    }
}
