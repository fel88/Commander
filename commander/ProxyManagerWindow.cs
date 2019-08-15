using Microsoft.Win32;
using ProxyLib;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class ProxyManagerWindow : Form
    {
        public ProxyManagerWindow()
        {
            InitializeComponent();
            UpdateOfflineList();
            Stuff.SetDoubleBuffered(listView2);
            Stuff.SetDoubleBuffered(listView1);
            Stuff.SetDoubleBuffered(listView3);
            listView3.HideSelection = false;
            listView2.HideSelection = false;

            if (File.Exists("cache.xml"))
            {
                cache.Restore("cache.xml");
            }

            listView3.Items.Clear();
            foreach (var item in cache.Cache)
            {
                listView3.Items.Add(new ListViewItem(new string[] { item.Key}) { Tag = item.Key });
            }
            SimpleHttpProxyServer.Cache = cache;

        }
        SimpleProxyCache cache = new SimpleProxyCache();
        private void UpdateOfflineList()
        {
            listView2.Items.Clear();
            foreach (var item in Stuff.OfflineSites)
            {
                listView2.Items.Add(new ListViewItem(item.Path) { Tag = item });
            }
        }

        Server server;
        public void Init(Server server)
        {
            this.server = server;
            server.NewConnectionInfoDelegate = (x) =>
            {
                listView1.Invoke((Action)(() =>
                {
                    listView1.Items.Add(new ListViewItem(new string[] { x.Log.Any() ? x.Log.First() : x.ToString(), "", "" }) { Tag = x });
                }));
            };
        }



        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var c = listView1.SelectedItems[0].Tag as ConnectionInfo;

            richTextBox1.Text = c.Log.Aggregate("", (x, y) => x + y + Environment.NewLine);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            toolStripLabel2.Text = "cache items: " + cache.Cache.Count;
            toolStripLabel2.ForeColor = Color.Green;
            foreach (var item in listView1.Items)
            {

                var l = item as ListViewItem;
                if (!(l.Tag is ConnectionInfo)) continue;
                var tt = l.Tag as ConnectionInfo;

                if (tt.Log.Any() && string.IsNullOrEmpty(l.SubItems[1].Text))
                {
                    l.SubItems[1].Text = tt.Log.First();
                }
                l.SubItems[2].Text = tt.Client.Available + "";
            }

            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);



            int proxyStatus = (int)registry.GetValue("ProxyEnable");
            toolStripLabel1.ForeColor = Color.Blue;
            if (proxyStatus == 0)
                toolStripLabel1.Text = "proxy: disable ";
            if (proxyStatus == 1)
                toolStripLabel1.Text = "proxy: enable";
        }



        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

            //disable proxy           
            //  registry.SetValue("ProxyServer", 0);
            registry.SetValue
                          ("ProxyServer", "127.0.0.1:8888");

            //remove tik 

            registry.SetValue("ProxyEnable", 1);

            //Proxy Status



        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

            //disable proxy           
            //registry.SetValue("ProxyServer", 1);

            //remove tik 

            registry.SetValue("ProxyEnable", 0);



        }
        public static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                // .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault();
        }



        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(SimpleHttpProxyServer.Run);
            th.IsBackground = true;
            th.Start();



            return;

            if (server == null)
            {
                server = new Server();
                server.Init(8888);
                Init(server);

            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            //start simple offline server. on the specific port and show all local sites to view
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            TcpClient cl = new TcpClient();
            /*WebClient wec = new WebClient();
            wec.Proxy = null;
            var dat = wec.DownloadString("http://codeforces.com");
            richTextBox3.Text = dat;
            return;*/
            var gateway = GetDefaultGateway();

            cl.Connect(gateway, 80);


            var wr = new StreamWriter(cl.GetStream());
            var rdr = new StreamReader(cl.GetStream());
            foreach (var item in richTextBox2.Lines)
            {
                wr.WriteLine(item);
            }


            richTextBox3.Text = rdr.ReadToEnd();

        }

        private void RichTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            var of = listView2.SelectedItems[0].Tag as OfflineSiteInfo;
            Stuff.ExecuteFile(new FileInfoWrapper(of.Path));
        }

        private void ToolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void RestoreCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("cache.xml"))
            {
                cache.Restore("cache.xml");
            }
        }

        private void SaveCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cache.Store("cache.xml");
        }

        private void EnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SimpleHttpProxyServer.UseCache = enableToolStripMenuItem.Checked;
        }

        private void DisableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SimpleHttpProxyServer.AllowStoreInCache = disableToolStripMenuItem.Checked;
        }

        private void ListView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView3.SelectedItems.Count == 0) return;
            var str = (string)listView3.SelectedItems[0].Tag;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = str;
            Process.Start(psi);
        }
    }
}



