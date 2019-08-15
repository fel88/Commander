using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DjvuNet;

namespace commander
{
    public partial class DjvuPreviewer : UserControl
    {
        public DjvuPreviewer()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        DjvuDocument Document;
        public void Init(string pathdoc)
        {            
            Document = new DjvuDocument(pathdoc);
            Init(Document);
        }
        public void Init(DjvuDocument doc)
        {
            Document = doc;
            currentPage = 0;
            label1.Text = " / "+doc.Pages.Length;
            UpdatePage();
        }

        int currentPage = 0;

        public void UnloadBook()
        {
            Document = null;
        }

        public void UpdatePage()
        {
            try
            {
                textBox1.Text = currentPage+"";
                var page = Document.Pages[currentPage];
                //page.IsInverted = false;                
                //var img = page.Image.Image;//(new DjvuNet.Graphics.Rectangle(0, 0, page.Width, page.Height), 1, 0, null).ToImage();
                var img = page.BuildImage();

                var temp = pictureBox1.Image;
                pictureBox1.Image = img;
                if (temp != null)
                {
                    temp.Dispose();
                }
            }
            catch (Exception ex)
            {
                Stuff.Error(ex.Message);
            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {            
            currentPage++;
            UpdatePage();
        }
                
        
        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                currentPage = int.Parse(textBox1.Text);
                UpdatePage();
            }
        }
    }
}
