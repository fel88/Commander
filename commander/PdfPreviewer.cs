using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfiumViewer;

namespace commander
{
    public partial class PdfPreviewer : UserControl
    {
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

        public void Init(string fullName)
        {
            pdfViewer1.Document?.Dispose();
            pdfViewer1.Document = OpenDocument(fullName);
        }
    }
}
