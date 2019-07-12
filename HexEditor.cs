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
    public partial class HexEditor : Form
    {
        public HexEditor()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            MouseWheel += Form1_MouseWheel;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                start_line += 5;
            }
            else
            {
                start_line -= 5;
            }
            if (start_line < 0)
            {
                start_line = 0;
            }

        }

        Bitmap bmp;
        Graphics gr;
        string lastPath = "";
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                OpenFile(ofd.FileName);
            }
        }

        public void OpenFile(string path)
        {
            lastPath = path;
            Text = path;
            var vf = File.ReadAllBytes(path);
            bytes = vf;
        }

        public byte[] bytes;
        int perline = 64;
        int start_line = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            gr.Clear(Color.White);
            int shift = 80;
            gr.DrawLine(Pens.Black, shift, 0, shift, bmp.Height);
            var font = Font;
            if (bytes != null)
            {
                int line = 0;
                int lineh = 15;
                int colw = 20;
                int max = bmp.Height / lineh;
                label1.Text = ((start_line * perline / (float)bytes.Length) * 100.0f).ToString("0.00") + "%";
                byte crc = 0;
                for (int i = start_line * perline; i < bytes.Length; i += perline)
                {
                    if (line > max)
                    {
                        break;
                    }
                    if (radioButton1.Checked)
                    {
                        gr.DrawString(i.ToString("X2"), font, Brushes.Black, 5, line * lineh);
                    }
                    else
                    {
                        gr.DrawString(i+"", font, Brushes.Black, 5, line * lineh);
                    }
                    crc = 0;
                    for (int j = 0; j < perline; j++)
                    {
                        if ((i + j) < bytes.Length)
                        {
                            crc ^= bytes[j + i];                           
                            gr.DrawString(bytes[j + i].ToString("X2"), font, Brushes.Black, j * colw + shift, line * lineh);
                        }
                    }
                    gr.DrawString(crc.ToString("X2"), font, Brushes.Black, perline * colw + shift, line * lineh);
                    line++;
                    
                }
                gr.DrawLine(Pens.Black, shift + perline * colw, 0, shift + perline * colw, bmp.Height);
                
            }
            pictureBox1.Image = bmp;

        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                perline = int.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var vf = File.ReadAllBytes(lastPath);
            bytes = vf;
        }
    }
}
