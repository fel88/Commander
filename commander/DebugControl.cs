using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace commander
{
    public partial class DebugControl : Form
    {
        public static List<DebugMessageInfo> Messages = new List<DebugMessageInfo>();
        public DebugControl()
        {
            InitializeComponent();
            Shown += DebugControl_Shown;
        }

        void updateList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var item in Messages)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Timestamp.ToString(), item.Type.ToString(), item.Text }) { Tag = item });
            }
            listView1.EndUpdate();
        }
        private void DebugControl_Shown(object sender, EventArgs e)
        {
            updateList();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Messages.Clear();
            updateList();
        }

        internal void UpdateList()
        {
            updateList();
        }
    }


    public class DebugMessageInfo
    {
        public DateTime Timestamp;
        public string Text;
        public DebugMessageInfoTypeEnum Type;
    }

    public enum DebugMessageInfoTypeEnum
    {
        Warning, Error, Info
    }
}
