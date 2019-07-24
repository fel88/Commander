using System.Drawing;
using System.Windows.Forms;

namespace commander
{
    public class PreviewControl : Control
    {
        private Bitmap bitmap = null;

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; Refresh(); }
        }

        public PreviewControl()
        {            
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw 
                //| ControlStyles.SupportsTransparentBackColor                
                | ControlStyles.Opaque, true);
            //BackColor = Color.Transparent;
        }

        public static int PreviewGap;
        protected override void OnPaint(PaintEventArgs e)
        {
            int shift = PreviewGap;
            Rectangle r = new Rectangle(shift, shift, Width - shift * 2, Height - shift * 2);


            Graphics g = e.Graphics;
            using (Brush brushWhite = new SolidBrush(Color.White))
            {
                g.FillRectangle(brushWhite, r);
            }

            if (bitmap == null)
            {
                using (Font fontText = new Font("Tahoma", 8, FontStyle.Regular))
                using (Brush brushText = new SolidBrush(Color.FromArgb(190, 190, 190)))
                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.TextContrast = 5;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    g.DrawString("Loading...", fontText, brushText, r, sf);
                }
            }
            else
            {
                g.DrawImageUnscaled(bitmap, shift, shift);
            }
        }
    }
}