namespace commander
{
    public interface ILibrary
    {
        string Name { get; }
        void AppendFile(string path, byte[] data);
    }
}
