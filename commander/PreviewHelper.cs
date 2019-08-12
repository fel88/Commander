using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;

namespace commander
{
    public class PreviewHelper
    {

        #region preview
        void RunPreview()
        {
            PreviewThreadLoader = new Thread(ThreadLoader);
            PreviewThreadLoader.IsBackground = true;
            PreviewThreadLoader.Start();
        }

        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            HideHint();
        }
        private ListViewItem lastHovered = null;
        private void HideHint()
        {
            if (lastHighlighted != null)
            {
                lastHighlighted.BackColor = lastColor;
            }
            preview.Parent = null;
        }
        PreviewControl preview = new PreviewControl();
        public bool ShowPreview = true;

        public static bool AllowHints = true;
        public static HintModeEnum HintMode = HintModeEnum.Tags;
        public enum HintModeEnum
        {
            Tags, Image
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!AllowHints) return;

            //preview.Location = new Point(e.X + 20, e.Y);
            var itemat = listView1.GetItemAt(e.Location.X, e.Location.Y);
            if (itemat == null) { HideHint(); return; }
            if (!(itemat.Tag is IFileInfo)) { HideHint(); return; }

            if (itemat == lastHovered)
            {
                return;
            }

            lastHovered = itemat;
            var s = itemat.Tag as IFileInfo;

            if (HintMode == HintModeEnum.Tags)
            {
                var ss = Stuff.GetTagsOfFile(s.FullName);
                if (!ss.Any())
                {
                    HideHint();
                    return;
                }
            }
            if (HintMode == HintModeEnum.Image)
            {
                if (!File.Exists(s.FullName) || !(s.Extension.ToLower().EndsWith("png") || s.Extension.ToLower().EndsWith("jpg") || s.Extension.ToLower().EndsWith("bmp")))
                {
                    HideHint();
                    return;
                }
            }
            if (ShowPreview) ShowHint(e, itemat, s);
        }

        private int _previewWidth = 200;
        private object FLoaderPathLocker = new object();
        private int _previewHeight = 200;
        private ListViewItem lastHighlighted = null;
        private Color lastColor;
        private IFileInfo previewCut;
        private string loaderPath = "";
        private string previewPath = "";

        private void LoadAsyncPreview(IFileInfo cut)
        {
            previewPath = cut.FullName;
            lock (FLoaderPathLocker)
            {
                previewCut = cut;
                loaderPath = cut.FullName;
            }

            if (preview.Bitmap != null)
            {
                Bitmap bmp = preview.Bitmap;
                preview.Bitmap = null;
                bmp.Dispose();
            }
        }

        private void ShowHint(MouseEventArgs e, ListViewItem node, IFileInfo fi)
        {
            if (lastHighlighted != null)
            {

                lastHighlighted.BackColor = lastColor;
            }
            lastHighlighted = node;

            lastColor = node.BackColor;
            node.BackColor = SystemColors.Highlight;

            Control toplevel = (FileListControl as Control).TopLevelControl;
            toplevel = listView1;
            if (toplevel != null)
            {
                LoadAsyncPreview(fi);

                preview.BackColor = SystemColors.Highlight;
                preview.Width = _previewWidth + PreviewControl.PreviewGap * 2;
                preview.Height = _previewHeight + PreviewControl.PreviewGap * 2;

                UpdatePreviewLocation(node);

                toplevel.Controls.Add(preview);
                toplevel.Controls.SetChildIndex(preview, 0);
            }
            else
            {
                preview.Parent = null;
            }
        }

        void UpdatePreviewLocation(ListViewItem node)
        {
            Control toplevel = (FileListControl as Control).TopLevelControl;
            toplevel = listView1;
            Point p_node = toplevel.PointToClient(listView1.PointToScreen(node.Bounds.Location));
            var bnds = node.Bounds;
            preview.Location =
                  new Point(
                      Math.Min(
                          listView1.Left + listView1.Width - preview.Width - SystemInformation.VerticalScrollBarWidth - 10,
                          listView1.Left + bnds.Width
                      ),
                      Math.Min(p_node.Y, listView1.Height - preview.Height));
        }

        private Thread PreviewThreadLoader;

        private bool StopThreadLoader = false;

        private void ThreadLoader()
        {
            while (!StopThreadLoader)
            {
                Thread.MemoryBarrier();
                try
                {
                    string lp = "";
                    lock (FLoaderPathLocker)
                    {
                        lp = loaderPath;
                        loaderPath = "";
                    }

                    if (lp != "")
                    {
                        string lpbuff = lp;

                        string ext = Path.GetExtension(lp);

                        Bitmap bmp = null;
                        if (HintMode == HintModeEnum.Tags)
                        {
                            bmp = GetTagHint();
                        }

                        if (HintMode == HintModeEnum.Image)
                        {
                            bmp = GetPreviewBmp(this, lp);
                        }

                       (FileListControl as Control).BeginInvoke((MethodInvoker)delegate { PreviewLoadedInvoked(lpbuff, bmp); });
                    }
                }
                catch { }
                Thread.Sleep(10);
            }
        }

        private Bitmap GetTagHint()
        {

            var tags = Stuff.GetTagsOfFile(previewCut.FullName);
            var grr = (FileListControl as Control).CreateGraphics();
            float w = 0;
            var font = new Font("Arial", 8);
            List<List<TagInfo>> ttt = new List<List<TagInfo>>();

            for (int i = 0; i < tags.Length; i++)
            {
                if ((i % 3) == 0) { ttt.Add(new List<TagInfo>()); }
                ttt.Last().Add(tags[i]);
            }
            foreach (var item in ttt)
            {

                float w0 = 0;
                foreach (var zitem in item)
                {
                    var ms = grr.MeasureString(zitem.Name, font);
                    w0 += ms.Width + 5;
                }
                w = Math.Max(w, w0);
            }

            grr.Dispose();

            var bmp = new Bitmap((int)w, ttt.Count() * 14, PixelFormat.Format32bppArgb);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.Transparent);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float xx = 0;
            float yy = 0;
            foreach (var zitem in ttt)
            {
                foreach (var item in zitem)
                {
                    var ms = gr.MeasureString(item.Name, font);

                    gr.FillRectangle(Brushes.White, xx, yy, ms.Width + 3, ms.Height);
                    gr.DrawRectangle(Pens.Black, xx, yy, ms.Width + 3, ms.Height);
                    gr.DrawString(item.Name, font, Brushes.Blue, xx, yy);
                    xx += gr.MeasureString(item.Name, font).Width + 5;
                }
                yy += 14;
                xx = 0;
            }
            gr.Dispose();
            _previewWidth = bmp.Width;
            _previewHeight = bmp.Height;
            preview.Width = _previewWidth + PreviewControl.PreviewGap * 2;
            preview.Height = _previewHeight + PreviewControl.PreviewGap * 2;
            UpdatePreviewLocation(lastHovered);
            return bmp;
        }


        private void PreviewLoadedInvoked(string drawingname, Bitmap bmp)
        {

            try
            {
                if (previewPath == drawingname)
                {
                    if (preview.Bitmap != null)
                    {
                        Bitmap bmp_old = preview.Bitmap;
                        preview.Bitmap = bmp;
                        bmp_old.Dispose();
                    }
                    else
                    {
                        preview.Bitmap = bmp;
                    }
                }
            }
            catch
            { }
        }

        private Bitmap GetPreviewBmp(object sender, string drawingName)
        {
            try
            {

                var bmp0 = Bitmap.FromFile(previewCut.FullName);
                var aspect = bmp0.Height / (float)bmp0.Width;
                _previewHeight = (int)(_previewWidth * aspect);

                Bitmap bmp = new Bitmap(_previewWidth, _previewHeight);
                var gr = Graphics.FromImage(bmp);
                var koef = bmp.Width / (float)bmp0.Width;
                gr.ScaleTransform(koef, koef);


                gr.DrawImage(bmp0, 0, 0);

                string dim = bmp0.Width.ToString("0.#") + "x" + bmp0.Height.ToString("0.#");
                bmp0.Dispose();
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Font fontDim = new Font("Arial", 8, FontStyle.Regular);
                Brush brushDim = new SolidBrush(Color.FromArgb(255, 255, 255));
                Brush brushDimBack = new SolidBrush(Color.FromArgb(164, 64, 64, 164));
                var sfDim = new StringFormat();

                sfDim.LineAlignment = StringAlignment.Far;
                sfDim.Alignment = StringAlignment.Near;
                gr.ResetTransform();
                gr.FillRectangle(brushDimBack, new RectangleF(0, bmp.Height - 14, gr.MeasureString(dim, fontDim).Width, 14));
                gr.DrawString(dim, fontDim, brushDim, new PointF(0, bmp.Height), sfDim);
                gr.Dispose();

                preview.Width = _previewWidth + 10;
                preview.Height = _previewHeight + 10;

                return bmp;
            }
            catch (Exception ex)
            {
                Bitmap bmp = new Bitmap(_previewWidth, _previewHeight);
                return bmp;
            }
        }
        #endregion

        ListView listView1;
        IFileListControl FileListControl;
        public void Append(IFileListControl flc, ListView lv)
        {
            FileListControl = flc;            
            listView1 = lv;
            lv.MouseLeave += ListView1_MouseLeave;
            lv.MouseMove += listView1_MouseMove;
            RunPreview();
        }

        public void Stop()
        {
            if (PreviewThreadLoader != null)
            {
                PreviewThreadLoader.Abort();
            }
        }
    }
}

