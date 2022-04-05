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
            List<IDirectoryInfo> dirs = new List<IDirectoryInfo>();
            dirs.Add(root);
            HashSet<string> hs = new HashSet<string>();
            foreach (var item in fls)
            {
                IDirectoryInfo p = item.Directory;
                while (true)
                {
                    if (p.FullName == root.FullName) break;

                    if (hs.Add(p.FullName))
                        dirs.Add(p);

                    p = p.Parent;
                }
            }
            List<DirectoryEntry> dentries = new List<DirectoryEntry>();
            int dId = 0;
            foreach (var item in dirs)
            {
                dentries.Add(new DirectoryEntry() { Path = item.FullName, Id = dId++ });
            }

            foreach (var item in fls)
            {
                var ww = Stuff.Tags.Where(z => z.ContainsFile(item));
                tags.AddRange(ww);
            }
            tags = tags.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<index>");
            sb.AppendLine("<entries>");
            foreach (var item in dentries)
            {
                sb.AppendLine($"<directory id=\"{item.Id}\"><![CDATA[{item.Path.Replace(root.FullName, string.Empty)}]]></directory>");
            }
            int fId = 0;
            List<FileEntry> fEntries = new List<FileEntry>();

            foreach (var item in fls)
            {
                var dirEntry = dentries.First(z => z.Path == item.DirectoryName);
                fEntries.Add(new FileEntry() { Id = fId, Directory = dirEntry, Name = item.Name });
                sb.AppendLine($"<file dirId=\"{dirEntry.Id}\" id=\"{fId++}\"><![CDATA[{item.FullName.Replace(dirEntry.Path, string.Empty).Trim(new char[] { '\\' })}]]></file>");
            }
            sb.AppendLine("</entries>");
            sb.AppendLine("<tags>");
            foreach (var item in tags)
            {
                sb.AppendLine($"<tag name=\"{item.Name}\">");
                sb.AppendLine("<synonyms>");
                foreach (var sitem in item.Synonyms)
                {
                    sb.Append("<synonym>");
                    sb.Append(sitem);
                    sb.Append("</synonym>");
                }
                sb.AppendLine("</synonyms>");
                var aa = fls.Where(z => item.ContainsFile(z)).ToArray();
                sb.Append($"<file id=\"");
                foreach (var fitem in fEntries)
                {
                    if (aa.Any(u => u.FullName.ToLower() == fitem.FullName.ToLower()))
                        sb.Append($"{fitem.Id};");
                }
                sb.AppendLine($"\"/>");

                /*foreach (var aitem in aa)
                {
                    sb.AppendLine($"<file><![CDATA[{aitem.FullName.Replace(root.FullName, "").Trim(new char[] { '\\' })}]]></file>");
                }*/
                sb.AppendLine($"</tag>");
            }
            sb.AppendLine("</tags>");
            sb.AppendLine("</index>");
            sb.AppendLine("</root>");

            return sb.ToString();

        }
        Thread th;
        PackToIsoSettings packStg;
        internal void Run(PackToIsoSettings stg)
        {
            packStg = stg;
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
                Invoke((Action)(() =>
                {
                    label1.Text = "Building image: " + stg.Path;
                }));
                try
                {
                    builder.Build(stg.Path);
                    Invoke((Action)(() =>
                    {
                        stg.AfterPackFinished?.Invoke();
                    }));
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
            th.Start();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Stuff.Question("Are your sure to cancel?") == DialogResult.Yes)
            {
                th.Abort();
                Close();
            }
        }

        PackToIsoSettings ps;
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Visible = true;
            button2.Enabled = false;
            Run(ps);
        }

        internal void Init(PackToIsoSettings stg)
        {
            ps = stg;
            textBox1.Text = stg.VolumeId;

            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var item in stg.Dirs)
            {
                Stuff.GetAllFiles(item, files);
            }

            files.AddRange(ps.Files);
            foreach (var item in files)
            {
                listView1.Items.Add(new ListViewItem(item.FullName.Replace(stg.Root.FullName, "")) { Tag = item });
            }
            label1.Text = "Iso image path: " + stg.Path;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ps.VolumeId = textBox1.Text;
        }
        bool flatFiles = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            flatFiles = checkBox1.Checked;
        }
    }
}
