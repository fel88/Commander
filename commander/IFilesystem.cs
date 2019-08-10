using System.Drawing;

namespace commander
{
    public interface IFilesystem
    {
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string fullName);
        Image BitmapFromFile(string fullName);
    }
  
}