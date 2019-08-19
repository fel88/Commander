using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace commander
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public GCHandle BitsHandle { get; private set; }

        public DirectBitmap(Bitmap bb)
        {
            var width = bb.Width;
            var height = bb.Height;
            var pf = PixelFormat.Format32bppPArgb;
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            var sz = Image.GetPixelFormatSize(pf) / 8;
            Bitmap = new Bitmap(width, height, width * sz,
                //PixelFormat.Format32bppPArgb, 
                pf,
                BitsHandle.AddrOfPinnedObject());

            var gr = Graphics.FromImage(Bitmap);
            gr.DrawImage(bb, 0, 0);
            gr.Dispose();
        }

        public DirectBitmap(int width, int height, PixelFormat pf = PixelFormat.Format32bppPArgb)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            var sz = Image.GetPixelFormatSize(pf) / 8;
            Bitmap = new Bitmap(width, height, width * sz,
                //PixelFormat.Format32bppPArgb, 
                pf,
                BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }


        private Color[,] cache;

        public void MakeCache()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    //get
                }
            }
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
