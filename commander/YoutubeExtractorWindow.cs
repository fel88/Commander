using PluginLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using YoutubeExtractor;

namespace commander
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            webBrowser1.Navigated += WebBrowser1_Navigated;
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            textBox1.Text = e.Url.ToString();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                webBrowser1.Navigate(textBox1.Text);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(textBox1.Text.Replace("player_","player-"), false);
            listView1.Items.Clear();
            foreach (var item in videoInfos.OrderByDescending(z => z.Resolution))
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.ToString(), item.Resolution + "", item.VideoExtension + "",item.AudioType.ToString() }) { Tag = item });
            }
        }

        private void DownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var video = listView1.SelectedItems[0].Tag as VideoInfo;
            /*
           * If the video has a decrypted signature, decipher it
           */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = video.VideoExtension;
            sfd.FileName = RemoveIllegalPathCharacters(video.Title) + video.VideoExtension;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                progressBar1.Visible = true;
                Thread th = new Thread(() =>
                {
                    var videoDownloader = new VideoDownloader(video,
              sfd.FileName
              );


                    videoDownloader.DownloadProgressChanged += VideoDownloader_DownloadProgressChanged;


                    videoDownloader.Execute();
                });
                th.IsBackground = true;
                th.Start();

            }

        }
        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
        private void VideoDownloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            progressBar1.Value = (int)e.ProgressPercentage;
        }
    }
}
