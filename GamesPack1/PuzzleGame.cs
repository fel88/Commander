﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GamesPack1
{
    public partial class PuzzleGame : Form
    {
        public PuzzleGame()
        {
            InitializeComponent();
            Shown += PuzzleGame_Shown;
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            MouseWheel += PuzzleGame_MouseWheel;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            SizeChanged += PuzzleGame_SizeChanged;



        }

        private void PuzzleGame_Shown(object sender, EventArgs e)
        {
            var ps = new PuzzleSelector();
            if (ps.ShowDialog() == DialogResult.OK)
            {
                Images = ps.Images;
                LoadPuzzle(ps.FileName);
                ResetPuzzle();
                loaded = true;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            dragItem = null;
            drag = false;
            drag2 = false;
            drag3 = false;
        }

        bool drag = false;
        bool drag2 = false;
        bool drag3 = false;
        PuzzleItem dragItem = null;
        int startdx;
        int startdy;
        float startposx;
        float startposy;
        float startang;
        PuzzleItem selected = null;
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            var pos = pictureBox1.PointToClient(Cursor.Position);
            if (e.Button == MouseButtons.Right)
            {
                drag2 = true;
                startposx = scrollx;
                startposy = scrolly;
                startdx = pos.X;
                startdy = pos.Y;
            }
            if (e.Button == MouseButtons.Left)
            {
                selected = null;
                if (hovered != null)
                {


                    Matrix mtr = new Matrix();
                    var item = hovered;
                    mtr.Translate(item.X, item.Y);
                    mtr.Translate(-item.Bmp.Width / 2, -item.Bmp.Height / 2);
                    mtr.Translate(item.Bmp.Width / 2, item.Bmp.Height / 2);
                    mtr.Rotate(item.Ang);
                    mtr.Translate(-item.Bmp.Width / 2, -item.Bmp.Height / 2);
                    mtr.Invert();
                    PointF[] pnts = new PointF[] { new PointF(pos.X - scrollx, pos.Y - scrolly) };

                    mtr.TransformPoints(pnts);
                    var pos0 = pnts[0];


                    var d = Math.Sqrt(Math.Pow(pos0.X - item.Bmp.Width / 2, 2) + Math.Pow(pos0.Y - item.Bmp.Height / 2, 2));

                    var temp = hovered;
                    selected = temp;
                    propertyGrid2.SelectedObject = selected;

                    while (temp.Parent != null)
                    {
                        temp = temp.Parent;
                    }
                    foreach (var item0 in Items)
                    {
                        item0.Z++;
                    }


                    dragItem = temp;

                    dragItem.Z = 0;
                    var l = GetAllChilds(dragItem);
                    foreach (var uitem in l)
                    {
                        uitem.Z = 0;
                    }

                    Items = Items.OrderByDescending(z => z.Z).ToList();
                    startdx = pos.X;
                    startdy = pos.Y;
                    startposx = dragItem.X;
                    startposy = dragItem.Y;
                    if (d < item.Bmp.Width / 2)
                    {
                        drag = true;
                    }
                    else
                    {
                        startang = dragItem.Ang;
                        drag3 = true;
                    }

                }
            }
        }

        float zoom = 1;
        private void PuzzleGame_MouseWheel(object sender, MouseEventArgs e)
        {
            return;
            if (e.Delta > 0)
            {
                zoom *= 1.2f;
            }
            if (e.Delta < 0)
            {
                zoom /= 1.2f;
            }
        }
        void LoadPuzzle(string path)
        {
            Text = path;
            Bitmap = Bitmap.FromFile(path) as Bitmap;
            //resize

            float koef = 1;
            if (Bitmap.Width > Bitmap.Height)
            {
                var max = (pictureBox1.Width * 0.95f);
                koef = max / Bitmap.Width;
            }
            else
            {
                var max = (pictureBox1.Height * 0.95f);
                koef = max / Bitmap.Height;
            }
            Bitmap newb = new Bitmap((int)(Bitmap.Width * koef), (int)(Bitmap.Height * koef));
            var gr = Graphics.FromImage(newb);
            gr.ScaleTransform(koef, koef);
            gr.DrawImage(Bitmap, 0, 0);
            var temp = Bitmap;

            temp.Dispose();

            Bitmap = newb;
            LoadPuzzle();
        }
        int ww = 100;
        int crad = 20;

        PuzzleItem[,] array;
        void LoadPuzzle()
        {
            Items.Clear();


            ww = Math.Max(Bitmap.Width, Bitmap.Height) / 6;
            crad = (int)(ww * 0.2f);


            var cellxcnt = Bitmap.Width / ww;
            var cellycnt = Bitmap.Height / ww;
            cellxcnt++;
            cellycnt++;
            array = new PuzzleItem[cellxcnt, cellycnt];
            bool[,] subsmap = new bool[cellxcnt, cellycnt];
            for (int i = 0; i < cellxcnt; i++)
            {
                for (int j = 0; j < cellycnt; j++)
                {
                    subsmap[i, j] = r.Next(2) == 0;
                }
            }


            for (int ci = 0; ci < cellxcnt; ci++)
            {
                for (int cj = 0; cj < cellycnt; cj++)
                {
                    int bshiftx = ci * ww;
                    int bshifty = cj * ww;
                    byte[,] mask = new byte[Bitmap.Width + 500, Bitmap.Height + 500];
                    //add rect
                    for (int i = 0; i < ww; i++)
                    {
                        for (int j = 0; j < ww; j++)
                        {
                            mask[i + bshiftx, j + bshifty] = 1;
                        }
                    }

                    //add circle
                    int shiftx = ww + (int)(crad * 0.75);
                    if (!subsmap[ci, cj])
                    {
                        shiftx = ww - (int)(crad * 0.75);
                    }
                    int shifty = ww / 2;
                    for (int i = -crad; i < crad; i++)
                    {
                        for (int j = -crad; j < crad; j++)
                        {
                            var dist = Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2));
                            if (dist < crad)
                            {
                                mask[i + bshiftx + shiftx, j + bshifty + shifty] = (byte)(subsmap[ci, cj] ? 1 : 0);
                            }
                        }
                    }

                    if (ci > 0)
                    {
                        //add circle
                        shiftx = -(int)(crad * 0.75);
                        if (subsmap[ci - 1, cj])
                        {
                            shiftx = (int)(crad * 0.75);
                        }
                        shifty = ww / 2;
                        for (int i = -crad; i < crad; i++)
                        {
                            for (int j = -crad; j < crad; j++)
                            {
                                var dist = Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2));
                                if (dist < crad)
                                {
                                    mask[i + bshiftx + shiftx, j + bshifty + shifty] = (byte)((!subsmap[ci - 1, cj]) ? 1 : 0);
                                }
                            }
                        }
                    }


                    //add circle
                    shiftx = ww / 2;
                    shifty = ww + (int)(crad * 0.75);
                    if (!subsmap[ci, cj])
                    {
                        shifty = ww - (int)(crad * 0.75);
                    }
                    for (int i = -crad; i < crad; i++)
                    {
                        for (int j = -crad; j < crad; j++)
                        {
                            var dist = Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2));
                            if (dist < crad)
                            {
                                mask[i + bshiftx + shiftx, j + bshifty + shifty] = (byte)(subsmap[ci, cj] ? 1 : 0);
                            }
                        }
                    }
                    if (cj > 0)
                    {
                        //add circle

                        shiftx = ww / 2;
                        shifty = -(int)(crad * 0.75);
                        if (subsmap[ci, cj - 1])
                        {
                            shifty = (int)(crad * 0.75);
                        }
                        for (int i = -crad; i < crad; i++)
                        {
                            for (int j = -crad; j < crad; j++)
                            {
                                var dist = Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2));
                                if (dist < crad)
                                {
                                    mask[i + bshiftx + shiftx, j + bshifty + shifty] = (byte)((!subsmap[ci, cj - 1]) ? 1 : 0);
                                }
                            }
                        }
                    }
                    //get bitmap via mask
                    Bitmap outp = new Bitmap(Bitmap.Width, Bitmap.Height, PixelFormat.Format32bppArgb);
                    int minx = int.MaxValue;
                    int miny = int.MaxValue;
                    int maxx = 0;
                    int maxy = 0;
                    Graphics gr3 = Graphics.FromImage(outp);
                    gr3.Clear(Color.Transparent);
                    gr3.Dispose();
                    var h = Bitmap.Height;
                    for (int i = 0; i < Bitmap.Width; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var mpx = mask[i, j];
                            if (mpx == 1)
                            {
                                var px = Bitmap.GetPixel(i, j);
                                outp.SetPixel(i, j, px);
                                minx = Math.Min(minx, i);
                                miny = Math.Min(miny, j);
                                maxx = Math.Max(maxx, i);
                                maxy = Math.Max(maxy, j);
                            }
                        }
                    }
                    Bitmap outp2 = new Bitmap(maxx - minx + 1, maxy - miny + 1, PixelFormat.Format32bppArgb);
                    Graphics gr2 = Graphics.FromImage(outp2);
                    gr2.TranslateTransform(-minx, -miny);
                    gr2.DrawImage(outp, 0, 0);
                    outp.Dispose();
                    gr2.Dispose();

                    //Items.Add(new PuzzleItem() { Bmp = outp2, Position = new Point(0, 0), X = r.Next(Width), Y = r.Next(Height), Ang = r.Next(360) });
                    Items.Add(new PuzzleItem()
                    {
                        Bmp = outp2,
                        Position = new Point(ci, cj),
                        X = ci * ww + ci * 5,
                        Y = cj * ww + cj * 5,
                        Ang = 0
                    });

                    Items.Last().TopLeftShift = new PointF(minx - ci * ww, miny - cj * ww);
                    Items.Last().CenterShift = new PointF(outp2.Width / 2, outp2.Height / 2);
                    array[ci, cj] = Items.Last();
                }
            }
        }
        private void PuzzleGame_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public List<PuzzleItem> Items = new List<PuzzleItem>();

        public Bitmap Bitmap;
        public Bitmap bmp;
        public Graphics gr;
        public Random r = new Random();
        PuzzleItem hovered = null;

        bool rotateCursor = false;

        float MinRotAng(float ang1, float ang2)
        {
            return (ang1 - ang2 + 540) % 360 - 180;
        }

        bool loaded = false;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            var pos1 = pictureBox1.PointToClient(Cursor.Position);
            var pos = new PointF(pos1.X, pos1.Y);
            /*pos.X -= scrollx;
            pos.Y -= scrolly;*/

            if (drag)
            {
                var dx = pos.X - startdx;
                var dy = pos.Y - startdy;
                dragItem.X = startposx + dx;
                dragItem.Y = startposy + dy;
                RecalcTree();
            }
            if (drag3)
            {
                //get sub of angs 
                //var dx =( pos.X-scrollx) - startdx;
                //var dy = (pos.Y-scrolly) - startdy;

                var tx1 = (startdx - scrollx) - startposx;
                var ty1 = (startdy - scrolly) - startposy;
                var len1 = (float)Math.Sqrt(Math.Pow(tx1, 2) + Math.Pow(ty1, 2));
                tx1 /= len1;
                ty1 /= len1;
                var atan1 = Math.Atan2(ty1, tx1);

                var tx2 = (pos.X - scrollx) - startposx;
                var ty2 = (pos.Y - scrolly) - startposy;
                var len2 = (float)Math.Sqrt(Math.Pow(tx2, 2) + Math.Pow(ty2, 2));
                tx2 /= len2;
                ty2 /= len2;
                var atan2 = Math.Atan2(ty2, tx2);

                var diff = MinRotAng((float)(atan2 * 180f / Math.PI), (float)(atan1 * 180f / Math.PI));


                //var da = dx;
                dragItem.Ang = startang + diff;
                dragItem.Ang %= 360;
                if (dragItem.Ang < 0)
                {
                    dragItem.Ang += 360;
                }
                RecalcTree();

            }
            if (drag2)
            {
                var dx = pos.X - startdx;
                var dy = pos.Y - startdy;
                scrollx = startposx + dx;
                scrolly = startposy + dy;
            }
            hovered = null;
            rotateCursor = false;
            foreach (var item in Items)
            {
                Matrix mtr = new Matrix();
                mtr.Translate(item.X, item.Y);
                mtr.Translate(-item.Bmp.Width / 2, -item.Bmp.Height / 2);
                mtr.Translate(item.Bmp.Width / 2, item.Bmp.Height / 2);
                mtr.Rotate(item.Ang);
                mtr.Translate(-item.Bmp.Width / 2, -item.Bmp.Height / 2);
                mtr.Invert();
                PointF[] pnts = new PointF[] { new PointF(pos.X - scrollx, pos.Y - scrolly) };

                mtr.TransformPoints(pnts);
                var pos0 = pnts[0];


                if (pos0.X > 0 && pos0.Y > 0 && pos0.X < item.Bmp.Width && pos0.Y < item.Bmp.Height)
                {
                    hovered = item;
                    var d = Math.Sqrt(Math.Pow(pos0.X - item.Bmp.Width / 2, 2) + Math.Pow(pos0.Y - item.Bmp.Height / 2, 2));
                    if (d >= item.Bmp.Width / 2)
                    {
                        rotateCursor = true;
                    }
                }
            }

            if (loaded)
            {

                for (int zi = 0; zi < array.GetLength(0); zi++)
                {
                    for (int zj = 0; zj < array.GetLength(1); zj++)
                    {
                        var item = array[zi, zj];
                        for (int i = 0; i <= 1; i++)
                        {
                            for (int j = 0; j <= 1; j++)
                            {
                                var x1 = zi + i;
                                var y1 = zj + j;
                                if ((Math.Abs(i) + Math.Abs(j)) != 1) continue;
                                if (x1 < 0 || y1 < 0 || x1 >= array.GetLength(0) || y1 >= array.GetLength(1)) continue;

                                var dang = MinRotAng(array[x1, y1].Ang, item.Ang);
                                var d = Math.Sqrt(Math.Pow(array[x1, y1].X - item.X, 2) + Math.Pow(array[x1, y1].Y - item.Y, 2));
                                if (Math.Abs(dang) < 10 && d < 1.5 * ww)
                                {
                                    //get connected component tree from any
                                    PuzzleItem top1 = item;
                                    while (top1.Parent != null) { top1 = top1.Parent; }
                                    PuzzleItem top2 = array[x1, y1];
                                    while (top2.Parent != null) { top2 = top2.Parent; }
                                    //check if intersected
                                    if (top1 != top2)
                                    {
                                        top1.Attach(top2);
                                    }

                                }
                            }
                        }
                    }
                }

            }
            gr.Clear(Color.Black);
            gr.TranslateTransform(scrollx, scrolly);
            gr.ScaleTransform(zoom, zoom);

            foreach (var item in Items)
            {


                var tr = gr.Transform;

                gr.TranslateTransform(item.X, item.Y);
                gr.TranslateTransform(-item.Bmp.Width / 2, -item.Bmp.Height / 2);
                gr.TranslateTransform(item.Bmp.Width / 2, item.Bmp.Height / 2);
                gr.RotateTransform(item.Ang);
                gr.TranslateTransform(-item.Bmp.Width / 2, -item.Bmp.Height / 2);

                gr.DrawImage(item.Bmp, 0, 0);
                //gr.DrawRectangle(new Pen(Color.Violet, 2), -item.TopLeftShift.X, -item.TopLeftShift.Y, 5, 5);
                //gr.DrawRectangle(new Pen(Color.Red, 2), item.CenterShift.X - 2, item.CenterShift.Y - 2, 4, 4);
                if (item == hovered)
                {
                    gr.DrawRectangle(new Pen(Color.Yellow, 2), 0, 0, item.Bmp.Width, item.Bmp.Height);
                }
                else
                {
                    if (selected == item)
                    {
                        //gr.DrawRectangle(new Pen(Color.Blue, 2), 0, 0, item.Bmp.Width, item.Bmp.Height);

                    }
                    else
                    {
                        //gr.DrawRectangle(Pens.Red, 0, 0, item.Bmp.Width, item.Bmp.Height);
                    }
                }

                gr.Transform = tr;
            }
            gr.ResetTransform();
            if (drag3 || (rotateCursor && hovered != null))
            {
                gr.DrawString("rotate", new Font("Arial", 8), Brushes.White, pos.X + 10, pos.Y);
                //gr.FillEllipse(Brushes.LightBlue, pos.X, pos.Y, 20, 20);
            }
            gr.ResetTransform();

            gr.DrawString((pos.X - scrollx) + "; " + (pos.Y - scrolly), new Font("Arial", 10), Brushes.White, 5, 5);
            pictureBox1.Image = bmp;

        }

        float scrollx;
        float scrolly;

        void ResetPuzzle()
        {
            foreach (var item in Items)
            {
                item.X = pictureBox1.Width / 2 + r.Next(-pictureBox1.Width / 3, pictureBox1.Width / 3) - scrollx;
                item.Y = pictureBox1.Height / 2 + r.Next(-pictureBox1.Height / 3, pictureBox1.Height / 3) - scrolly;
                item.Ang = r.Next(360);
                item.Connected.Clear();
                item.Parent = null;
            }
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            ResetPuzzle();
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            loaded = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadPuzzle(ofd.FileName);
                ResetPuzzle();
                loaded = true;
            }
        }

        List<PuzzleItem> GetAllChilds(PuzzleItem p, List<PuzzleItem> list = null)
        {
            if (list == null)
            {
                list = new List<PuzzleItem>();
            }
            list.Add(p);
            foreach (var pp in p.Connected)
            {
                GetAllChilds(pp, list);
            }
            return list;
        }
        void ProcessRec(PuzzleItem ccitem)
        {
            foreach (var item in ccitem.Connected)
            {

                item.Ang = ccitem.Ang;
                var relativex = (item.Position.X - ccitem.Position.X) * ww;
                var relativey = (item.Position.Y - ccitem.Position.Y) * ww;
                Matrix mtr = new Matrix();
                mtr.Rotate(ccitem.Ang);
                PointF[] pp = new PointF[] { new PointF(relativex, relativey),
                        new PointF (item.CenterShift.X,item.CenterShift.Y),
                        new PointF (item.TopLeftShift.X,item.TopLeftShift.Y),
                        new PointF (ccitem.CenterShift.X,ccitem.CenterShift.Y),
                        new PointF (ccitem.TopLeftShift.X,ccitem.TopLeftShift.Y),

                    };
                mtr.TransformPoints(pp);
                var tlx1 = ccitem.X - pp[3].X - pp[4].X;
                var tly1 = ccitem.Y - pp[3].Y - pp[4].Y;

                var targetx1 = tlx1 + pp[0].X;
                var targety1 = tly1 + pp[0].Y;


                item.X = targetx1 + pp[1].X + pp[2].X;
                item.Y = targety1 + pp[1].Y + pp[2].Y;
                ProcessRec(item);

            }
        }
        void RecalcTree()
        {
            List<PuzzleItem> processed = new List<PuzzleItem>();

            foreach (var ccitem in Items)
            {
                if (processed.Contains(ccitem)) continue;

                var top = ccitem;
                while (top.Parent != null) { top = top.Parent; }
                var list = GetAllChilds(top);
                processed.AddRange(list);
                ProcessRec(top);


            }
        }
        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            RecalcTree();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            if (!loaded) return;
            foreach (var item in Items)
            {
                item.Parent = null;
                item.Connected.Clear();
            }

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (i == 0 && j == 0) continue;
                    array[0, 0].Attach(array[i, j]);
                }
            }

            array[0, 0].X = 0;
            array[0, 0].Y = 0;
            foreach (var item in Items)
            {
                item.Ang = 0;
            }
            RecalcTree();
        }

        private void ToolStripButton5_Click(object sender, EventArgs e)
        {
            if (Images == null || !Images.Any()) return;

            var c = Images[r.Next(Images.Count())];
            LoadPuzzle(c);
            ResetPuzzle();
            loaded = true;
        }

        public string[] Images;
        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            var ps = new PuzzleSelector();
            if (ps.ShowDialog() == DialogResult.OK)
            {
                Images = ps.Images;
                LoadPuzzle(ps.FileName);
                ResetPuzzle();
                loaded = true;
            }
        }
    }
}


