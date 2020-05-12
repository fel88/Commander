using BrightIdeasSoftware;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace commander.Controls.MetaInfo
{
    public partial class SubtitlesMetaInfoControl : UserControl, IMetaInfoEditorControl
    {
        public SubtitlesMetaInfoControl()
        {
            InitializeComponent();
            (objectListView1.Columns[1] as OLVColumn).AspectGetter = (x) =>
            {
                return (x as SubtitleItem).Start.ToString();
            };
            (objectListView1.Columns[1] as OLVColumn).AspectPutter = (x, y) =>
             {
                 (x as SubtitleItem).Start = TimeSpan.Parse((string)y);
             };
            (objectListView1.Columns[2] as OLVColumn).AspectGetter = (x) =>
            {
                return (x as SubtitleItem).End.ToString();
            };
            (objectListView1.Columns[2] as OLVColumn).AspectPutter = (x, y) =>
            {
                (x as SubtitleItem).End = TimeSpan.Parse((string)y);
            };
        }

        SubtitlesMetaInfo Info;
        public void Init(SubtitlesMetaInfo k)
        {
            Info = k;
            axWindowsMediaPlayer1.URL = Info.Parent.File.FullName;
            objectListView1.SetObjects(Info.Items);
        }

        public event Action ValueChanged;

        private void timer1_Tick(object sender, EventArgs e)
        {
            var pos = axWindowsMediaPlayer1.Ctlcontrols.currentPosition / 60.0;
            label1.Text = TimeSpan.FromSeconds(axWindowsMediaPlayer1.Ctlcontrols.currentPosition) + string.Empty;
            var fr = Info.Items.FirstOrDefault(z => pos >= z.Start.TotalMinutes && pos <= z.End.TotalMinutes);
            if (fr != null)
            {
                textBox1.Text = fr.Text;
            }
            else
            {
                textBox1.Text = string.Empty;
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int nextId = 1;
            if (Info.Items.Any())
            {
                nextId = Info.Items.Max(z => z.Id) + 1;
            }
            Info.Items.Add(new SubtitleItem() { Id = nextId, Text = "new text" });
            objectListView1.SetObjects(Info.Items);

        }

        public void Stop()
        {
            axWindowsMediaPlayer1.URL = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var t1 = TimeSpan.Parse(textBox2.Text);
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = t1.TotalSeconds;
                textBox2.BackColor = Color.White;
                textBox2.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
                textBox2.ForeColor = Color.White;
            }

        }

        private void goHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject == null) return;
            var sub = objectListView1.SelectedObject as SubtitleItem;
            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = sub.Start.TotalSeconds;
        }

        private void setCurrentEndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject == null) return;
            var sub = objectListView1.SelectedObject as SubtitleItem;
            sub.End = TimeSpan.FromSeconds(axWindowsMediaPlayer1.Ctlcontrols.currentPosition);
        }

        private void setCurrentStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject == null) return;
            var sub = objectListView1.SelectedObject as SubtitleItem;
            sub.Start = TimeSpan.FromSeconds(axWindowsMediaPlayer1.Ctlcontrols.currentPosition);
        }

        private void copyPreviousEndStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
