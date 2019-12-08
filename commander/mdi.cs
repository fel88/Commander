using PluginLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using YoutubeExtractor;

namespace commander
{
    public partial class mdi : Form, IAppContainer
    {
        public mdi()
        {
            InitializeComponent();
            MainForm = this;
            LoadPlugins();            

            MdiChildActivate += Mdi_MdiChildActivate;
            Stuff.LoadSettings();
            Stuff.IsDirty = false;

            AllowDrop = true;
            toolStrip1.AllowDrop = true;
            toolStrip1.DragEnter += Tsb1_DragEnter;

            OpenWindow(new Explorer());

            foreach (var item in Stuff.Shortcuts)
            {
                AppendShortCutPanelButton(item);
            }
        }

        private void Mdi_MdiChildActivate(object sender, EventArgs e)
        {
            var fr = MdiChildren.FirstOrDefault(z => z is QuickTagsWindow);
            if (fr == null) return;
            fr.BringToFront();

        }

        public void AppendShortCutPanelButton(ShortcutInfo item)
        {
            toolStrip1.Visible = true;

            MyToolStripButton tsb1 = new MyToolStripButton() { TextImageRelation = TextImageRelation.ImageAboveText };
            tsb1.ContextMenuStrip = contextMenuStrip1;
            tsb1.AutoSize = true;
            tsb1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsb1.Text = item.Name;
            tsb1.Tag = item;
            toolStrip1.Items.Add(tsb1);
            tsb1.Click += Tsb1_Click;
            tsb1.AllowDrop = true;
            tsb1.DragDrop += Tsb1_DragDrop;
            tsb1.DragEnter += Tsb1_DragEnter;
            //toolStrip1.ContextMenuStrip = contextMenuStrip1;

            if (item is AppShortcutInfo)
            {
                var apps = item as AppShortcutInfo;
                var fn = new FileInfo(apps.Path);

                if (fn.Exists)
                {
                    var bb = Bitmap.FromHicon(Icon.ExtractAssociatedIcon(fn.FullName).Handle);
                    bb.MakeTransparent();
                    //toolStrip1.ImageScalingSize = new Size((int)(10), (int)(10));
                    tsb1.Image = bb;
                }
            }
            else if (item is UrlShortcutInfo)
            {
                Bitmap bb = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
                var gr = Graphics.FromImage(bb);
                gr.Clear(Color.Transparent);
                gr.DrawString(item.Name, new Font("Arial", 32), Brushes.Black, 0, 0);
                bb.MakeTransparent();
                tsb1.ToolTipText = item.Name + " : " + (item as UrlShortcutInfo).Url;
                tsb1.Image = bb;
            }
            else
            {
                Bitmap bb = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
                var gr = Graphics.FromImage(bb);
                gr.Clear(Color.Transparent);
                gr.DrawString(item.Name, new Font("Arial", 32), Brushes.Black, 0, 0);
                bb.MakeTransparent();
                tsb1.ToolTipText = item.Name + " : " + (item as CmdShortcutInfo).Args + " workdir: " + (item as CmdShortcutInfo).WorkDir;
                tsb1.Image = bb;
            }
        }

        private void Tsb1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Tsb1_DragDrop(object sender, DragEventArgs e)
        {
            var fn = (sender as ToolStripButton).Tag as ShortcutInfo;
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                var str = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (fn is AppShortcutInfo)
                {
                    (fn as AppShortcutInfo).TempArguments = str.First();
                    fn.Run();
                }
            }

            if (e.Data.GetData(DataFormats.Text) != null)
            {
                var str = (string)e.Data.GetData(DataFormats.Text);
                if (fn is AppShortcutInfo)
                {
                    (fn as AppShortcutInfo).TempArguments = str;
                    fn.Run();
                }
            }


        }

        private void Tsb1_Click(object sender, EventArgs e)
        {
            var fn = (sender as ToolStripButton).Tag as ShortcutInfo;
            fn.Run();

        }

        private void LoadPlugins()
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var p1 = Path.Combine(bd, "Plugins");
            if (!Directory.Exists(p1)) return;
            var dir = new DirectoryInfo(p1);
            foreach (var item in dir.GetDirectories())
            {
                foreach (var fitem in item.GetFiles())
                {
                    TryLoadPlugin(fitem.FullName);
                }
            }
        }

        public enum LoadPluginResultEnum
        {
            Success, Fail,
            //todo: make field in plugin attrib, AllowMultipleInstances..
            AlreadyExist
        }

        Tuple<LoadPluginResultEnum, PluginInfoAttribute, IPlugin> TryLoadPlugin(string filename)
        {
            IPlugin ret = null;
            PluginInfoAttribute attr = null;
            try
            {
                var asm = Assembly.LoadFrom(filename);
                var tps = asm.GetTypes().Where(z => typeof(IPlugin).IsAssignableFrom(z)).ToArray();
                foreach (var zitem in tps)
                {
                    var aaa = zitem.GetCustomAttributes(true);
                    if (aaa.Any(z => z.GetType() == typeof(PluginInfoAttribute)))
                    {
                        attr = aaa.First(z => z.GetType() == typeof(PluginInfoAttribute)) as PluginInfoAttribute;
                    }

                    if (PluginsInstances.Any(z => z.GetType() == zitem))
                    {
                        return new Tuple<LoadPluginResultEnum, PluginInfoAttribute, IPlugin>(LoadPluginResultEnum.AlreadyExist, attr, null);
                    }

                    var plg = Activator.CreateInstance(zitem) as IPlugin;
                    ret = plg;
                    plg.Activate(new PluginContext() { Container = mdi.MainForm });
                    PluginsInstances.Add(plg);
                    break;//first plugin only? can one lib contains multiple plugins?

                }
            }
            catch (Exception ex)
            {

            }
            if (ret != null)
            {
                return new Tuple<LoadPluginResultEnum, PluginInfoAttribute, IPlugin>(LoadPluginResultEnum.Success, attr, ret);
            }
            else
            {
                return new Tuple<LoadPluginResultEnum, PluginInfoAttribute, IPlugin>(LoadPluginResultEnum.Fail, attr, ret);
            }
        }

        public static List<IPlugin> PluginsInstances = new List<IPlugin>();


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenWindow(new Explorer());
        }

        public static mdi MainForm;

        public MenuStrip MainMenu
        {
            get
            {
                return menuStrip1;
            }
        }

        public ILibrary[] Libraries => Stuff.Libraries.ToArray();

        public ITagInfo[] Tags => Stuff.Tags.ToArray();

        public bool ShowHiddenTags => Stuff.ShowHidden;

        public void OpenWindow(Form frm)
        {
            frm.MdiParent = this;
            frm.Show();
        }

        public void OpenWindow(Control cc)
        {
            var frm = new Form();
            frm.Width = cc.Width;
            frm.Height = cc.Height;

            cc.Dock = DockStyle.Fill;
            frm.Controls.Add(cc);
            frm.MdiParent = this;
            frm.Show();

        }
        private void ToolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void Mdi_Load(object sender, EventArgs e)
        {

        }

        private void FgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow(new Explorer());
        }

        private void WebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox1 a = new AboutBox1();
            a.ShowDialog();
        }

        private void WindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void WindowsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            for (int i = windowsToolStripMenuItem.DropDownItems.Count - 1; i > 0; i--)
            {
                windowsToolStripMenuItem.DropDownItems.Remove(windowsToolStripMenuItem.DropDownItems[i]);
            }
            windowsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            foreach (var item in MdiChildren)
            {
                var w = new ToolStripMenuItem(item.Text) { Tag = item };
                windowsToolStripMenuItem.DropDownItems.Add(w);
                w.Click += W_Click;
            }
        }

        private void W_Click(object sender, EventArgs e)
        {
            var m = sender as ToolStripMenuItem;
            (m.Tag as Form).Activate();
        }

        private void InstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plugins|*.dll;*.exe";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var res = TryLoadPlugin(ofd.FileName);
                var ret = res.Item2;
                switch (res.Item1)
                {
                    case LoadPluginResultEnum.Success:
                        Stuff.Info($"Plugin {ret.Name} loaded successfully!");
                        break;
                    case LoadPluginResultEnum.Fail:
                        Stuff.Error($"Plugin {ret.Name} load error.");
                        break;
                    case LoadPluginResultEnum.AlreadyExist:
                        Stuff.Warning($"Plugin {ret.Name} already loaded.");
                        break;
                }
            }
        }

        private void Mdi_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Stuff.IsDirty)
            {
                var res = MessageBox.Show("Save changes (tabs, libraries etc.)?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (res)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.Yes:
                        Stuff.UnmountAll();
                        Stuff.SaveSettings();
                        break;

                }
            }
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void HorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void VerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void DesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow(new Desktop());
        }

        private void ContextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Tag == null) return;
            var mtb = (contextMenuStrip1.Tag as MyToolStripButton);
            var s = mtb.Tag as ShortcutInfo;
            Stuff.Shortcuts.Remove(s);
            Stuff.IsDirty = true;
            toolStrip1.Items.Remove(mtb);
            if (toolStrip1.Items.Count == 0) { toolStrip1.Visible = false; }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow s = new SettingsWindow();
            s.ShowDialog();
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Tag == null) return;
            var mtb = (contextMenuStrip1.Tag as MyToolStripButton);
            var s = mtb.Tag as ShortcutInfo;
            RenameDialog rnd = new RenameDialog();
            rnd.Value = s.Name;
            if (rnd.ShowDialog() == DialogResult.OK)
            {
                s.Name = rnd.Value;
                mtb.Text = rnd.Value;
                Stuff.IsDirty = true;
            }
        }

        private void CopyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Tag == null) return;
            var mtb = (contextMenuStrip1.Tag as MyToolStripButton);

            var s = mtb.Tag as ShortcutInfo;
            if (s is AppShortcutInfo)
            {
                Clipboard.SetText((s as AppShortcutInfo).Path);
            }
            if (s is UrlShortcutInfo)
            {
                Clipboard.SetText((s as UrlShortcutInfo).Url);
            }
        }

        private void YoutubeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        
        private void ProxyServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TitaniumProxyManager pm = new TitaniumProxyManager();
            //ProxyManagerWindow pm = new ProxyManagerWindow();
            pm.MdiParent = this;
            
            pm.Show();
        }

        private void MountListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MountListWindow mlw = new MountListWindow();
            mlw.MdiParent = this;
            mlw.Show();
        }

        private void vkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VkExtractorForm f = new VkExtractorForm();
            f.MdiParent = this;
            f.Show();
        }

        private void IndexesListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IndexesManagerWindow mlw = new IndexesManagerWindow();
            mlw.MdiParent = this;
            mlw.Show();
        }

        private void IsoEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void BrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow(new Browser());
        }

        private void UrlBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow(new UrlBookmarksWindow());
        }

        private void IndexToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void PartialIndexMountListToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CartridgeEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CartridgeEditorWindow mlw = new CartridgeEditorWindow();
            mlw.MdiParent = this;
            mlw.Show();
        }

        private void MetaInfoRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MetaInfoManager mim = new MetaInfoManager();
            mim.MdiParent = this;
            mim.Show();
        }

        public void AddExplorerPreviewExtension(ExplorerPreviewExtension e)
        {
            Explorer.PreviewExtensions.Add(e);
        }
    }

    public class ConnectionInfo
    {
        public List<string> Log = new List<string>();
        public NetworkStream Stream;
        public TcpClient Client;
        public IPAddress Ip;
        public int Port;
        public object Tag;

    }

    public class UserInfo
    {
        public string Name;
    }
}
