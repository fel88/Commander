using System;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using PluginLib;

namespace commander
{
    public partial class ImgViewerPanel : UserControl, IFileListConsumer
    {
        public ImgViewerPanel()
        {
            InitializeComponent();
        }
        public ImgViewerClickActionMode ClickMode = ImgViewerClickActionMode.NextImage;

        public enum ImgViewerClickActionMode
        {
            RandomImage, NextImage, Open, SizeModeChange
        }

        public Action PictureClick;

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            switch (ClickMode)
            {
                case ImgViewerClickActionMode.RandomImage:
                    SetRandomImage();
                    break;
                case ImgViewerClickActionMode.NextImage:
                    SetNextImage();
                    break;
                case ImgViewerClickActionMode.Open:
                    CurrentFile.Filesystem.Run(CurrentFile);                    
                    break;
                case ImgViewerClickActionMode.SizeModeChange:
                    pictureBox1.SizeMode = (PictureBoxSizeMode)((((int)pictureBox1.SizeMode) + 1) % Enum.GetValues(typeof(PictureBoxSizeMode)).Length);
                    break;
                default:
                    break;
            }            
            PictureClick?.Invoke();
        }
        public void ResetImage()
        {
            Image temp = null;
            if (pictureBox1.Image != null)
            {
                temp = pictureBox1.Image;
            }
            pictureBox1.Image = null;
            CurrentFile = null;
            if (temp != null)
            {
                temp.Dispose();
            }
        }
        internal void SetImage(Image image)
        {
            pictureBox1.Image = image;
            CurrentFile = null;
        }
        public IFileInfo CurrentFile;


        internal void SetImage(IFileInfo fl)
        {
            Image temp = null;
            if (pictureBox1.Image != null)
            {
                temp = pictureBox1.Image;
            }
            CurrentFile = fl;
            pictureBox1.Image = fl.Filesystem.BitmapFromFile(fl);
            if (temp != null)
            {
                temp.Dispose();
            }
        }
        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            mdi.MainForm.OpenWindow(this);
        }
        FileListControl fileListControl;
        public void SetFileList(FileListControl flc)
        {
            fileListControl = flc;
        }

        static Random rand = new Random();
        public void SetRandomImage()
        {
            if (fileListControl == null) return;
            ImgPreviewExtension aa = new ImgPreviewExtension();
            int nn = 0;
            IFileInfo file = null;
            while (true)
            {
                var ar = fileListControl.CurrentDirectory.GetFiles().Where(z => aa.Extensions.Contains(z.Extension.ToLower())).ToArray();
                nn = rand.Next(ar.Length);
                file = ar[nn];
                if (ar[nn].FullName != CurrentFile.FullName) break;
            }
            SetImage(file);
        }

        public void SetNextImage()
        {
            if (fileListControl == null) return;
            ImgPreviewExtension aa = new ImgPreviewExtension();
            int nn = 0;
            IFileInfo file = null;

            var ar = fileListControl.CurrentDirectory.GetFiles().Where(z => aa.Extensions.Contains(z.Extension.ToLower())).ToArray();
            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i].FullName == CurrentFile.FullName)
                {
                    nn = (i + 1) % ar.Length;
                    break;
                }
            }

            file = ar[nn];
            SetImage(file);
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickMode = ImgViewerClickActionMode.NextImage;
        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickMode = ImgViewerClickActionMode.RandomImage;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickMode = ImgViewerClickActionMode.Open;
        }

        private void sizeModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClickMode = ImgViewerClickActionMode.SizeModeChange;
        }
    }

    public interface IFileListConsumer
    {
        void SetFileList(FileListControl flc);
    }
}