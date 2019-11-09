using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfiumViewer;
using PluginLib;

namespace commander
{
    public partial class PdfPreviewer : UserControl
    {
        public IFileInfo CurrentFile { get; internal set; }

        public PdfPreviewer()
        {
            InitializeComponent();
            
        }
        private PdfDocument OpenDocument(string fileName)
        {
            try
            {
                return PdfDocument.Load(this, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private PdfDocument OpenDocument(IFileInfo file)
        {
            try
            {
                
                return PdfDocument.Load(this, file.Filesystem.OpenReadOnlyStream(file));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void Init(IFileInfo fl)
        {
            CurrentFile = fl;
            pdfViewer1.Document?.Dispose();
            pdfViewer1.Document = OpenDocument(fl);            
        }

        internal void Reset()
        {
            var temp = pdfViewer1.Document;            
            pdfViewer1.Document = null;
            temp.Dispose();
        }
    }
}
