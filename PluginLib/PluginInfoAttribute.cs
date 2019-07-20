using System;

namespace PluginLib
{
    public class PluginInfoAttribute : Attribute
    {
        public string Name { get; set; }
        public bool AllowMultipleInstances { get; set; }
    }
}
