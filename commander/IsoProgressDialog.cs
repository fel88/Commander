using IsoLib.DiscUtils;
using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace commander
{
    public partial class IsoProgressDialog : Form
    {
        public IsoProgressDialog()
        {
            InitializeComponent();
            Shown += IsoProgressDialog_Shown;
        }

        private void IsoProgressDialog_Shown(object sender, EventArgs e)
        {
            th.Start();
        }

        public static void PopulateFromFolder(CDBuilder builder, IDirectoryInfo di, string basePath)
        {
            foreach (IFileInfo file in di.GetFiles())
            {
                if (file is VirtualFileInfo)
                {
                    var vfi = file as VirtualFileInfo;
                    builder.AddFile(file.FullName.Substring(basePath.Length), vfi.FileInfo.FullName);
                }
                else
                {
                    builder.AddFile(file.FullName.Substring(basePath.Length), file.FullName);
                }
            }

            foreach (IDirectoryInfo dir in di.GetDirectories())
            {
                PopulateFromFolder(builder, dir, basePath);
            }
        }
        private static string GenerateMetaXml(IFileInfo[] fls, IDirectoryInfo root)
        {
            //var fls = Stuff.GetAllFiles(dir);
            List<TagInfo> tags = new List<TagInfo>();
            foreach (var item in fls)
            {
                var ww = Stuff.Tags.Where(z => z.ContainsFile(item));
                tags.AddRange(ww);
            }
            tags = tags.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<tags>");
            foreach (var item in tags)
            {
                sb.AppendLine($"<tag name=\"{item.Name}\">");
                var aa = fls.Where(z => item.ContainsFile(z)).ToArray();
                foreach (var aitem in aa)
                {
                    sb.AppendLine($"<file><![CDATA[{aitem.FullName.Replace(root.FullName, "").Trim(new char[] { '\\' })}]]></file>");
                }
                sb.AppendLine($"</tag>");
            }
            sb.AppendLine("</tags>");
            sb.AppendLine("</root>");

            return sb.ToString();

        }
        Thread th;
        PackToIsoSettings packStg;
        internal void Run(PackToIsoSettings stg)
        {
            packStg = stg;
            label1.Text = "Iso image path: " + stg.Path;
            Text = "iso writing..";
            stg.ProgressReport = (now, total) =>
            {
                float f = now / (float)total;
                this.Invoke((Action)(() =>
                {
                    progressBar1.Value = (int)Math.Round(f * 100);
                }));

            };
            stg.AfterPackFinished = () =>
            {
                Stuff.Info("Pack to iso complete!");
                Close();
            };

            th = new Thread(() =>
            {
                this.Invoke((Action)(() =>
                {
                    label1.Text = "collecting files..";
                }));

                CDBuilder builder = new CDBuilder();
                builder.ProgresReport = stg.ProgressReport;
                List<IFileInfo> files = new List<IFileInfo>();
                foreach (var item in stg.Dirs)
                {
                    Stuff.GetAllFiles(item, files);
                }
                files.AddRange(stg.Files);

                if (stg.IncludeMeta)
                {
                    var mm = GenerateMetaXml(files.ToArray(), stg.Root);
                    builder.AddDirectory(".indx");
                    builder.AddFile(".indx\\meta.xml", Encoding.UTF8.GetBytes(mm));
                }
                builder.VolumeIdentifier = stg.VolumeId;
                if (stg.BeforePackStart != null)
                {
                    stg.BeforePackStart();
                }
                foreach (var item in stg.Dirs)
                {
                    PopulateFromFolder(builder, item, item.FullName);
                }

                foreach (var item in stg.Files)
                {
                    if (item is VirtualFileInfo)
                    {
                        var vfi = item as VirtualFileInfo;
                        builder.AddFile(item.FullName, vfi.FileInfo.FullName);
                    }
                    else
                    {
                        builder.AddFile(item.FullName, item.FullName);
                    }
                }
                this.Invoke((Action)(() =>
                {
                    label1.Text = "Building image: " + stg.Path;
                }));
                try
                {
                    builder.Build(stg.Path);
                    if (stg.AfterPackFinished != null)
                    {
                        stg.AfterPackFinished();
                    }
                }
                catch (ThreadAbortException ex)
                {
                    if (File.Exists(packStg.Path))
                    {
                        File.Delete(packStg.Path);
                    }
                }

            });
            th.IsBackground = true;
         
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Stuff.Question("Are your sure to cancel?") == DialogResult.Yes)
            {
                th.Abort();
                Close();
            }
        }
    }
}
