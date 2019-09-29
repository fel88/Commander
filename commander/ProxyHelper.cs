using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace commander
{
    public static class ProxyHelper
    {
        [DllImport("wininet.dll")]
        internal static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        internal const int InternetOptionSettingsChanged = 39;
        internal const int InternetOptionRefresh = 37;
        private const string regKeyInternetSettings = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";

        public static void DisableProxy()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(regKeyInternetSettings, true);

            //disable proxy           
            //registry.SetValue("ProxyServer", 1);

            //remove tik 

            registry.SetValue("ProxyEnable", 0);
            RefreshProxy();
        }

        public static void EnableProxy()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(regKeyInternetSettings, true);

            //disable proxy           
            //  registry.SetValue("ProxyServer", 0);
            //registry.SetValue("ProxyServer", "127.0.0.1:8888");
            registry.SetValue("ProxyServer", "http=localhost:8888;https=localhost:8888");
            //"http=localhost:8888;https=localhost:8888"

            //remove tik 

            registry.SetValue("ProxyEnable", 1);

            //Proxy Status

            RefreshProxy();
        }

        public static void RefreshProxy()
        {
            //refresh
            InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);

        }
    }
}



