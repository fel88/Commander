namespace PluginLib
{
    public interface ILibrary
    {
        string Name { get; }
        void AppendFile(string path, byte[] data);
        string[] EnumerateFiles();
        byte[] GetFile(string path);
    }
}
