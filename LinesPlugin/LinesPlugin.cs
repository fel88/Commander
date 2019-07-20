using PluginLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinesPlugin
{
    [PluginInfo(Name = "Lines", AllowMultipleInstances = false)]
    public class LinesPlugin : IPlugin
    {
        public string Name => "Lines";

        public static IAppContainer Container;
        public void Activate(PluginContext ctx)
        {
            ToolStripMenuItem mitem = null;
            Container = ctx.Container;
            foreach (var item in ctx.Container.MainMenu.Items.OfType<ToolStripMenuItem>())
            {
                if (item.Text == "Games")
                {
                    mitem = item;
                    break;
                }
            }
            if (mitem == null)
            {
                mitem = new ToolStripMenuItem("Games") { };
                ctx.Container.MainMenu.Items.Add(mitem);
            }
            var g = new ToolStripMenuItem("Lines");
            g.Click += G_Click;
            mitem.DropDownItems.Add(g);
        }

        private void G_Click(object sender, EventArgs e)
        {
            Container.OpenWindow(new Lines());
        }
    }
}
