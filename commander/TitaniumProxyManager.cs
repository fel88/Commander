using BrightIdeasSoftware;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Titanium.Web.Proxy.Network.Tcp;

namespace commander
{
    public partial class TitaniumProxyManager : Form
    {
        public TitaniumProxyManager()
        {
            InitializeComponent();

            ProxyTestController.Message = (x) =>
            {

            };
        }
        private static readonly ProxyTestController controller = new ProxyTestController();

        private void Button1_Click(object sender, EventArgs e)
        {
            controller.StartProxy();
        }

        private void EnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProxyHelper.EnableProxy();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            controller.Stop();
        }

        private void DisableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProxyHelper.DisableProxy();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            StreamWrapper.Caches.Clear();
            BuildIndex();
            GC.Collect();
        }
        public void Restore(string fn)
        {
            var doc = XDocument.Load(fn);
            //ProxyCacheClientStream.Caches.Clear();
            foreach (var item in doc.Descendants("item"))
            {
                var key = item.Attribute("key").Value;
                var host = item.Attribute("host").Value;
                var port = int.Parse(item.Attribute("port").Value);
                var data = item.Attribute("data").Value;
                var mem = item.Attribute("mem").Value;
                var dat1 = Convert.FromBase64String(data);
                var mem1 = Convert.FromBase64String(mem);
                var bb = Encoding.UTF8.GetString(dat1);
                var bb2 = Encoding.UTF32.GetString(dat1);

                var bb1 = Encoding.UTF8.GetString(Convert.FromBase64String(key));
                if (StreamWrapper.ActiveCache == null)
                {
                    StreamWrapper.Caches.Add(new WebCache());
                    StreamWrapper.ActiveCache = StreamWrapper.Caches.Last();
                }
                StreamWrapper.ActiveCache.Table.Add(new FakeRequest()
                {
                    Host = host,
                    Port = port,
                    Response = new StringBuilder(bb),
                    Text = bb1,
                    Mem = new MemoryStream(mem1)
                });
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < ofd.FileNames.Count(); i++)
            {
                Restore(ofd.FileNames[i]);
            }

            BuildIndex();
            GC.Collect();
        }
        public class WebIndexItem
        {
            public WebIndexItem Parent;
            public string Name { get; set; }
            public long Size { get; set; }

            public void CaclSize()
            {
                if (Request != null)
                {
                    Size += Request.Mem.Length;
                }
                foreach (var item in Childs)
                {
                    item.CaclSize();
                    Size += item.Size;
                }
            }
            public FakeRequest Request;

            public List<WebIndexItem> Childs = new List<WebIndexItem>();
        }
        public WebIndexItem GetByPath(WebIndexItem item, string[] path)
        {
            if (path.Count() == 0) return item;
            var fr = item.Childs.First(z => z.Name == path[0]);
            return GetByPath(fr, path.Skip(1).ToArray());
        }

        public void CreatePath(WebIndexItem item, string[] path)
        {
            if (path.Count() == 0) return;
            if (!item.Childs.Any(z => z.Name == path[0]))
            {
                item.Childs.Add(new WebIndexItem() { Parent = item, Name = path[0] });
            }
            var fr = item.Childs.First(z => z.Name == path[0]);
            CreatePath(fr, path.Skip(1).ToArray());
        }


        public void BuildIndex()
        {
            var grp = StreamWrapper.StaticRequests.GroupBy(z => z.Host).ToArray();

            List<WebIndexItem> Roots = new List<WebIndexItem>();
            foreach (var item in grp)
            {
                Roots.Add(new WebIndexItem() { Name = item.Key });
                var zz = item.ToArray().Select(z => z.Text).ToArray();
                List<string> sstrs = new List<string>();
                foreach (var zitem in zz)
                {
                    var u = zitem.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    sstrs.Add(u);
                }
                sstrs = sstrs.OrderBy(z => z).ToList();
                foreach (var sitem in sstrs)
                {
                    var str2 = sitem.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    CreatePath(Roots.Last(), str2);
                }

                foreach (var aitem in item.ToArray())
                {
                    var u = aitem.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    var str2 = u.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    GetByPath(Roots.Last(), str2).Request = aitem;
                }
            }

            foreach (var item in Roots)
            {
                item.CaclSize();
            }

            treeListView1.GridLines = true;
            treeListView1.CanExpandGetter = (z) =>
            {

                if (z is WebIndexItem ww)
                {
                    return ww.Childs.Any();
                }
                return false;
            };

            treeListView1.ChildrenGetter = (z) =>
            {
                if (z is WebIndexItem ww)
                {
                    return ww.Childs;
                }
                return null;
            };

            
            (treeListView1.Columns[1] as OLVColumn).TextAlign = HorizontalAlignment.Right;
            (treeListView1.Columns[1] as OLVColumn).AspectGetter = (x) =>
            {
                return Stuff.GetUserFriendlyFileSize((x as WebIndexItem).Size);
            };
            treeListView1.SetObjects(Roots.OrderBy(z => z.Name));
        }

        private void TreeListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeListView1.SelectedObject == null) return;
            if (treeListView1.SelectedObject is WebIndexItem wi)
            {
                if (wi.Request == null) return;
                string req = "http://" + wi.Request.Host;

                var s = wi.Request.Text.Split(new char[] { ' ' }).ToArray()[1];
                req += s;
                Process.Start(req);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            toolStripLabel2.Text = "cache items: " + StreamWrapper.StaticRequests.Length;
            toolStripLabel2.ForeColor = Color.Green;


            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);



            int proxyStatus = (int)registry.GetValue("ProxyEnable");
            toolStripLabel1.ForeColor = Color.Blue;
            if (proxyStatus == 0)
                toolStripLabel1.Text = "proxy: disable ";
            if (proxyStatus == 1)
                toolStripLabel1.Text = "proxy: enable";
        }
        public void SaveCache(string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            foreach (var item in StreamWrapper.StaticRequests)
            {
                var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Response.ToString()));
                var b642 = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Text.ToString()));
                var b3 = Convert.ToBase64String(item.Mem.ToArray());
                sb.AppendLine($"<item host=\"{item.Host}\" key=\"{b642}\" port=\"{item.Port}\" data=\"{b64}\" mem=\"{b3}\"/>");
                //sb.AppendLine($"<item host=\"{item.Host}\" port=\"{item.Port}\" data=\"{b64}\" mem=\"{b3}\"/>");
            }
            sb.AppendLine("</root>");
            File.WriteAllText(str, sb.ToString());
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            SaveCache(ofd.FileName);
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            BuildIndex();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            FakeHttpStream.UseFake = checkBox1.Checked;
        }
    }
}
