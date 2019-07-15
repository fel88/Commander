using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
            Explorer f = new Explorer();
            
            f.MdiParent = this;
            f.Show();
            //f.WindowState = FormWindowState.Maximized;

            MainForm = this;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenWindow(new Explorer());
        }

        public static mdi MainForm;

        public void OpenWindow(Form frm)
        {
            frm.MdiParent = this;
            frm.Show();

        }

        public void OpenWindow(Control cc)
        {
            var frm = new Form();
            frm.Width = cc.Width;
            frm.Height = cc.Height;

            cc.Dock = DockStyle.Fill;
            frm.Controls.Add(cc);
            frm.MdiParent = this;
            frm.Show();

        }
        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            OpenWindow(new Browser());
        }

        
    }
}
