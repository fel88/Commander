using System.Windows.Forms;

namespace commander
{
    public class MyToolStripButton : ToolStripButton
    {
        ContextMenuStrip _contextMenuStrip;

        public ContextMenuStrip ContextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                
                if (_contextMenuStrip != null)
                    _contextMenuStrip.Show((Cursor.Position));
                _contextMenuStrip.Tag = this;
            }
        }
    }

}
