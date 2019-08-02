using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace commander
{
    public partial class ProxyManagerWindow : Form
    {
        public ProxyManagerWindow()
        {
            InitializeComponent();
            UpdateOfflineList();
        }

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
            foreach (var item in listView1.Items)
            {
                var l = item as ListViewItem;
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

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
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
            cl.Connect("192.168.1.1", 80);

            var wr = new StreamWriter(cl.GetStream());
            var rdr = new StreamReader(cl.GetStream());

            wr.WriteLine("");
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
    }
}
