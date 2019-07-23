using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GamesPack1
{
    public partial class PuzzleSelector : Form
    {
        public PuzzleSelector()
        {
            InitializeComponent();
            if (GamePack1Plugin.Container == null)
            {
                button1.Enabled = false;
            }
            else
            {
                if (GamePack1Plugin.Container.Libraries.Count() == 0)
                {
                    button1.Enabled = false;
                }
            }
        }

        public string FileName;

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
        Random r = new Random();
        private void Button1_Click(object sender, EventArgs e)
        {

            foreach (var item in GamePack1Plugin.Container.Libraries)
            {
                var cands = item.EnumerateFiles().Where(z => z.EndsWith(".jpg") || z.EndsWith(".png")).ToArray();
                var c = cands[r.Next(cands.Count())];
                FileName = c;
                DialogResult = DialogResult.OK;
                Close();
                //using (var ms = new MemoryStream(item.GetFile(c)))
                //{
                //    var bmp = Bitmap.FromStream(ms) as Bitmap;
                //    Bitmap = bmp;
                //    break;
                //}

            }
        }
    }
}
