using PluginLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace commander
{
    public partial class mdi : Form, IAppContainer
    {
        public mdi()
        {
            InitializeComponent();

            LoadPlugins();
          

            /*MemInfoReport test = new MemInfoReport();
            test.MdiParent = this;
            test.Sectors.Add(new ReportSectorInfo() {   Percentage=250f/360, Name = "test" });
            test.Sectors.Add(new ReportSectorInfo() {  Percentage=100/360f, Name = "test1" });
            test.Show();
            */
            //f.WindowState = FormWindowState.Maximized;
            Stuff.LoadSettings();
            Stuff.IsDirty = false;
            MainForm = this;

            OpenWindow(new Explorer());            
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
            OpenWindow(new Browser());
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
            ofd.Filter = "Plugins|*.dll";
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
    }
}
