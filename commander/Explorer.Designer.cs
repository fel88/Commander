namespace commander
{
    partial class Explorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Explorer));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tablePreviewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableTextEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.compareMD5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.fileListControl1 = new commander.FileListControl();
            this.fileListControl2 = new commander.FileListControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1004, 560);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton3,
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1004, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(46, 22);
            this.toolStripButton3.Text = "Search";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tablesToolStripMenuItem,
            this.tablePreviewerToolStripMenuItem,
            this.tableTextEditorToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(56, 22);
            this.toolStripDropDownButton1.Text = "Layout";
            // 
            // tablesToolStripMenuItem
            // 
            this.tablesToolStripMenuItem.Name = "tablesToolStripMenuItem";
            this.tablesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.tablesToolStripMenuItem.Text = "table -> table";
            this.tablesToolStripMenuItem.Click += new System.EventHandler(this.TablesToolStripMenuItem_Click);
            // 
            // tablePreviewerToolStripMenuItem
            // 
            this.tablePreviewerToolStripMenuItem.Name = "tablePreviewerToolStripMenuItem";
            this.tablePreviewerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.tablePreviewerToolStripMenuItem.Text = "table -> previewer";
            this.tablePreviewerToolStripMenuItem.Click += new System.EventHandler(this.TablePreviewerToolStripMenuItem_Click);
            // 
            // tableTextEditorToolStripMenuItem
            // 
            this.tableTextEditorToolStripMenuItem.Name = "tableTextEditorToolStripMenuItem";
            this.tableTextEditorToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.tableTextEditorToolStripMenuItem.Text = "table -> text editor";
            this.tableTextEditorToolStripMenuItem.Click += new System.EventHandler(this.TableTextEditorToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compareMD5ToolStripMenuItem,
            this.compareBinaryToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(78, 22);
            this.toolStripDropDownButton2.Text = "Operations";
            // 
            // compareMD5ToolStripMenuItem
            // 
            this.compareMD5ToolStripMenuItem.Name = "compareMD5ToolStripMenuItem";
            this.compareMD5ToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.compareMD5ToolStripMenuItem.Text = "Compare MD5";
            this.compareMD5ToolStripMenuItem.Click += new System.EventHandler(this.CompareMD5ToolStripMenuItem_Click);
            // 
            // compareBinaryToolStripMenuItem
            // 
            this.compareBinaryToolStripMenuItem.Name = "compareBinaryToolStripMenuItem";
            this.compareBinaryToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.compareBinaryToolStripMenuItem.Text = "Compare binary";
            this.compareBinaryToolStripMenuItem.Click += new System.EventHandler(this.CompareBinaryToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.fileListControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.fileListControl2);
            this.splitContainer1.Size = new System.Drawing.Size(998, 529);
            this.splitContainer1.SplitterDistance = 499;
            this.splitContainer1.TabIndex = 3;
            // 
            // fileListControl1
            // 
            this.fileListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileListControl1.Location = new System.Drawing.Point(0, 0);
            this.fileListControl1.Name = "fileListControl1";
            this.fileListControl1.Size = new System.Drawing.Size(499, 529);
            this.fileListControl1.TabIndex = 0;
            this.fileListControl1.Load += new System.EventHandler(this.fileListControl1_Load);
            // 
            // fileListControl2
            // 
            this.fileListControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileListControl2.Location = new System.Drawing.Point(0, 0);
            this.fileListControl2.Name = "fileListControl2";
            this.fileListControl2.Size = new System.Drawing.Size(495, 529);
            this.fileListControl2.TabIndex = 1;
            // 
            // Explorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 560);
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.Name = "Explorer";
            this.Text = "Explorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Explorer_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FileListControl fileListControl1;
        private FileListControl fileListControl2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem tablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tablePreviewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem compareMD5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareBinaryToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem tableTextEditorToolStripMenuItem;
    }
}

