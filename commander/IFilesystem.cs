namespace commander
{
    public interface IFilesystem
    {
        byte[] ReadAllBytes(string path);
        string[] ReadAllLines(string fullName);
    }
  
}