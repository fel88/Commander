using System.IO;

namespace isoViewer
{
    public static class Extensions
    {
        public static int ReadInt(this FileStream fs)
        {
            byte[] bb = new byte[4];
            fs.Read(bb, 0, 4);
            int ret = 0;
            for (int i = 0; i < 4; i++)
            {
                ret |= bb[i] << (8 * i);
            }

            return ret;

        }
    }
}