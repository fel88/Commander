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
    public partial class Lines : Form
    {
        public Lines()
        {
            InitializeComponent();
            Shown += Lines_Shown;
        }

        private void Lines_Shown(object sender, EventArgs e)
        {
            Height = 10 * CellW + SystemInformation.ToolWindowCaptionHeight + 6;
            Width = 10 * CellW + 6;
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            InitGame();
        }

        Random r = new Random();

        Point[] GetFree()
        {
            List<Point> ret = new List<Point>();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == -1)
                    {
                        ret.Add(new Point(i, j));
                    }
                }
            }
            return ret.ToArray();
        }
        void AddRandCircle(Point[] free)
        {
            var p = free[r.Next(free.Length)];
            map[p.X, p.Y] = r.Next(brushes.Length);
        }

        public void InitGame()
        {
            map = new int[10, 10];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = -1;
                }
            }
            var free = GetFree();
            for (int i = 0; i < 3; i++)
            {
                AddRandCircle(free);
            }
        }

        Graphics gr;
        Bitmap bmp;

        int[,] map = new int[10, 10];
        const int CellW = 30;

        int score = 0;
        Brush[] brushes = new Brush[] { Brushes.Red, Brushes.Blue, Brushes.Green };

        bool gameOver = false;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            int sx = 0;
            int sy = 0;

            var fr = GetFree();
            if (!fr.Any()) gameOver = true;
            var ww = CellW;
            gr.Clear(Color.Black);
            //gr.DrawString("Scores: " + score, new Font("Consolas", 12), Brushes.White, 10 , 10);
            Text = "Lines: Scores: " + score;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    var rect = new Rectangle(i * ww + sx, j * ww + sy, ww, ww);
                    var rect2 = new Rectangle(i * ww + sx + 2, j * ww + sy + 2, ww - 4, ww - 4);
                    if (selected != null && selected.Value.X == i && selected.Value.Y == j && map[i, j] != -1)
                    {
                        gr.FillRectangle(Brushes.Orange, rect);
                    }
                    gr.DrawRectangle(Pens.White, rect);
                    if (map[i, j] != -1)
                    {
                        gr.FillEllipse(brushes[map[i, j]], rect2);
                        gr.DrawEllipse(Pens.White, rect2);
                    }
                }
            }
            if (gameOver)
            {

            }
            gr.DrawString("Game under construction.", new Font("Arial", 8), Brushes.White, 2, 2);
            pictureBox1.Image = bmp;
        }

        Point? selected = null;

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var loc = e.Location;
            var xx = loc.X / CellW;
            var yy = loc.Y / CellW;
            if (selected == null)
            {
                if (map[xx, yy] != -1)
                {
                    selected = new Point(xx, yy);
                }
            }
            else
            {
                //bfs to target
                var val = map[selected.Value.X, selected.Value.Y];
                if (map[xx, yy] != -1 || val == -1) return;

                map[selected.Value.X, selected.Value.Y] = -1;
                map[xx, yy] = val;
                selected = null;
                AddRandCircle(GetFree());

            }
        }
    }
}
