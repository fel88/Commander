using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace commander
{
    public partial class Desktop : Form
    {
        public Desktop()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            MinimumSize = new Size(100, SystemInformation.CaptionHeight + 20);

            SizeChanged += Desktop_SizeChanged;

            InitGui();

            pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!Elements.Any(z => z.Hovered)) return;
            var fr = Elements.First(z => z.Hovered);
            fr.DoubleClick();
        }

        public void InitGui()
        {
            foreach (var item in Stuff.Shortcuts)
            {
                DesktopIcon d = new DesktopIcon();
                d.Caption = item.Name;
                
                if (item is AppShortcutInfo)
                {
                    var apps = item as AppShortcutInfo;
                    d.ProgramPath = apps.Path;
                    var fn = new FileInfo(apps.Path);
                    if (fn.Exists)
                    {
                        var bb = Bitmap.FromHicon(Icon.ExtractAssociatedIcon(fn.FullName).Handle);
                        bb.MakeTransparent();                      
                        d.Image = bb;

                    }
                }
                Elements.Add(d);
            }
        }

        private void Desktop_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
        }

        Bitmap bmp;
        Graphics gr;

        public List<GuiElement> Elements = new List<GuiElement>();
        private void timer1_Tick(object sender, EventArgs e)
        {
            gr.Clear(Color.White);
            int xx = 20;
            int yy = 10;
            var ctx = new DesktopDrawingContext(gr, pictureBox1);

            #region arrange icons
            foreach (var item in Elements.OfType<DesktopIcon>())
            {
                //var ms = gr.MeasureString(item.Caption, font1);
                //gr.DrawString(item.Caption, font1, Brushes.Black, xx + xsz / 2 - ms.Width / 2, yy + yshift);
                item.Position = new PointF(xx, yy);
                yy += 80;
                if (yy > (Height - 150))
                {
                    yy = 10;
                    xx += 90;
                }
            }
            #endregion




            foreach (var item in Elements.OrderBy(z => z.ZOrder))
            {
                item.Draw(ctx);
            }
            pictureBox1.Image = bmp;
        }
    }

    public class DesktopDrawingContext
    {
        public DesktopDrawingContext(Graphics gr, Control c)
        {
            Graphics = gr;
            Control = c;

        }
        public Graphics Graphics;
        public Control Control;

        public PointF CursorPos
        {
            get
            {
                return Control.PointToClient(Cursor.Position);
            }
        }
    }

    public class GuiElement
    {
        public bool Hovered;
        public virtual void DoubleClick() { }
        public virtual void Draw(DesktopDrawingContext ctx) { }
        public PointF Position;
        public int ZOrder;
        public virtual RectangleF BoundingBox { get; }
    }

    public class DesktopIcon : GuiElement
    {

        public override void DoubleClick()
        {
            Process.Start(ProgramPath);
            base.DoubleClick();
        }
        public override RectangleF BoundingBox => new RectangleF(Position.X, Position.Y, 60, 60);
        public Font Font = DefaultFont;
        public static Font DefaultFont = new Font("Arial", 10);
        public override void Draw(DesktopDrawingContext ctx)
        {
            var pos = ctx.CursorPos;
            Hovered = BoundingBox.IntersectsWith(new RectangleF(pos.X, pos.Y, 1, 1));
            var gr = ctx.Graphics;
            var ms = gr.MeasureString(Caption, Font);
            var yy = Position.Y;
            var xx = Position.X;
            var sx = xx + Image.Width / 2 - ms.Width / 2;
            var sy = yy + Image.Height;
            if (Hovered)
            {
                gr.FillRectangle(Brushes.LightBlue, sx, sy, ms.Width, ms.Height);
            }


            gr.DrawString(Caption, Font, Brushes.Black, sx, sy);
            gr.DrawImage(Image, xx, yy);

            base.Draw(ctx);
        }

        public Image Image;
        public string Caption;
        public string ProgramPath;
    }
}
