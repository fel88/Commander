using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class ProgressBarOperationDialog : Form
    {
        public ProgressBarOperationDialog()
        {
            InitializeComponent();
            Shown += ProgressBarOperationDialog_Shown;
        }

        private void ProgressBarOperationDialog_Shown(object sender, EventArgs e)
        {

            th.Start();
        }

        public void SetProgress(string title, int p, int max)
        {
            progressBar1.Invoke((Action)(() =>
            {
                Text = title;
                progressBar1.Maximum = max;
                progressBar1.Value = p;
            }));
           
        }
        Thread th;
        internal void Init(Action p)
        {
            DialogResult = DialogResult.OK;
            th = new Thread(() =>
            {
                p();
            });
            th.IsBackground = true;
        }




        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            th.Abort();
            Close();
        }

        internal void Complete()
        {
            progressBar1.Invoke((Action)(() =>
            {
                progressBar1.Value = progressBar1.Maximum;
            }));
            Close();
        }
    }
}
