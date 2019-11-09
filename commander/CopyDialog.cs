using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class CopyDialog : Form
    {
        public CopyDialog()
        {
            InitializeComponent();
            Shown += CopyDialog_Shown;
        }

        private void CopyDialog_Shown(object sender, EventArgs e)
        {
            Run();
        }

        IFileInfo[] list;
        IDirectoryInfo[] dirs;
        IDirectoryInfo to;
        IDirectoryInfo prnt;
        public void Init(IFileInfo[] list, IDirectoryInfo[] dirs, IDirectoryInfo to, IDirectoryInfo prnt)
        {
            this.list = list;
            this.dirs = dirs;
            this.to = to;
            this.prnt = prnt;
        }

        public void UpdateProgress(int total, int cnt)
        {
            progressBar1.Invoke((Action)(() =>
            {
                var val = (int)Math.Round(((float)cnt / total) * 100);
                progressBar1.Value = val;
            }));
        }

        public void UpdateLabels(string from, string to, int cnt, int total, int skipped)
        {
            label1.Invoke((Action)(() =>
            {
                label2.Text = from + " -> " + to;
                label1.Text = cnt + " / " + total;
                if (skipped > 0)
                {
                    label1.Text += "; skipped: " + skipped;
                }
            }));
        }

        Thread th;
        public void Run()
        {
            th = new Thread(() =>
            {
                int cnt = 0;
                int total = dirs.Count() + list.Count();
                int ftotal = list.Count();
                int skipped = 0;
                foreach (var item in dirs)
                {
                    UpdateProgress(total, cnt);

                    var relPath = item.FullName.Replace(prnt.FullName, "").Trim(new char[] { '\\' });
                    var target = Path.Combine(to.FullName, relPath);
                    UpdateLabels(item.FullName, target, cnt, total, 0);
                    if (!Directory.Exists(target))
                    {
                        Directory.CreateDirectory(target);
                    }
                    cnt++;
                }

                bool repAll = false;
                bool skipAll = false;

                foreach (var item in list)

                {
                    UpdateProgress(total, cnt);

                    var relPath = item.FullName.Replace(prnt.FullName, "").Trim(new char[] { '\\' });
                    var target = Path.Combine(to.FullName, relPath);
                    UpdateLabels(item.FullName, target, cnt, total, skipped);
                    if (File.Exists(target) && skipAll)
                    {
                        cnt++;
                        skipped++;
                        continue;
                    }
                    if (File.Exists(target) && !repAll)
                    {
                        CopyReplaceDialog crd = new CopyReplaceDialog();                        
                        crd.Init(target);                        
                        crd.ShowDialog(this);
                        switch (crd.Result)
                        {
                            case CopyReplaceDialogResultEnum.ReplaceAll:
                                repAll = true;
                                File.Copy(item.FullName, target, true);
                                break;
                            case CopyReplaceDialogResultEnum.Replace:
                                File.Copy(item.FullName, target, true);
                                break;
                            case CopyReplaceDialogResultEnum.Skip:
                                skipped++;
                                break;
                            case CopyReplaceDialogResultEnum.SkipAll:
                                skipped++;
                                skipAll = true;
                                break;
                        }
                    }
                    else
                    {
                        File.Copy(item.FullName, target, true);
                    }
                    cnt++;
                }
                UpdateProgress(total, total);
                Close();
            });
            th.IsBackground = true;
            th.Start();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Stuff.Question("Are your sure to abort coping?") == DialogResult.Yes)
            {
                th.Abort();
                Close();
            }
        }
    }
}
