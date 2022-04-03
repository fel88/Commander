using System.Runtime.InteropServices;

namespace commander.Controls
{
    public static class WebpWrapper
    {
        [DllImport("webp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sum(int a, int b);
        [DllImport("webp.dll")]
        public static extern int test1();
        [DllImport("webp.dll")]
        public static extern int getWidth();

        [DllImport("webp.dll")]
        public static extern int getXoffset();

        [DllImport("webp.dll")]
        public static extern int getYoffset();


        [DllImport("webp.dll")]
        public static extern int getDuration();


        [DllImport("webp.dll")]
        public static extern int getHeight();
        [DllImport("webp.dll")]

        public static extern int test2(int a);

        [DllImport("webp.dll")]
        public static extern int getFramesNum();

        [DllImport("webp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void decodeIteration(int what);


        [DllImport("webp.dll")]
        public static extern int getCurrFrame();

        [DllImport("webp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getData(byte[] data);


        [DllImport("webp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int open(string filePath);
    }
}
