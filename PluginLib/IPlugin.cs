
namespace PluginLib
{
    public interface IPlugin
    {
        void Activate(PluginContext ctx);
        string Name { get; }
    }
}
