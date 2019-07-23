namespace commander
{
    partial class FileListControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.executeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openExplorerHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInImageViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCmdHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calcMemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findFileRepeatsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calcMd5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addShortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView1.ContextMenuStrip = this.contextMenuStrip2;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(515, 271);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView1_KeyDown);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 180;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Modified time";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "MD5";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Flags";
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.executeToolStripMenuItem,
            this.openExplorerHereToolStripMenuItem,
            this.openInHexToolStripMenuItem,
            this.searcToolStripMenuItem,
            this.openInImageViewerToolStripMenuItem,
            this.openCmdHereToolStripMenuItem,
            this.calcMemToolStripMenuItem,
            this.findFileRepeatsToolStripMenuItem,
            this.calcMd5ToolStripMenuItem,
            this.newToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.makeLibraryToolStripMenuItem,
            this.setTagsToolStripMenuItem,
            this.addShortcutToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(188, 334);
            this.contextMenuStrip2.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.ContextMenuStrip2_Closing);
            // 
            // executeToolStripMenuItem
            // 
            this.executeToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
            this.executeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.executeToolStripMenuItem.Text = "execute";
            this.executeToolStripMenuItem.Click += new System.EventHandler(this.ExecuteToolStripMenuItem_Click);
            // 
            // openExplorerHereToolStripMenuItem
            // 
            this.openExplorerHereToolStripMenuItem.Name = "openExplorerHereToolStripMenuItem";
            this.openExplorerHereToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openExplorerHereToolStripMenuItem.Text = "open explorer here";
            this.openExplorerHereToolStripMenuItem.Click += new System.EventHandler(this.openExplorerHereToolStripMenuItem_Click);
            // 
            // openInHexToolStripMenuItem
            // 
            this.openInHexToolStripMenuItem.Name = "openInHexToolStripMenuItem";
            this.openInHexToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openInHexToolStripMenuItem.Text = "open in hex";
            this.openInHexToolStripMenuItem.Click += new System.EventHandler(this.openInHexToolStripMenuItem_Click);
            // 
            // searcToolStripMenuItem
            // 
            this.searcToolStripMenuItem.Name = "searcToolStripMenuItem";
            this.searcToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.searcToolStripMenuItem.Text = "search";
            this.searcToolStripMenuItem.Click += new System.EventHandler(this.searcToolStripMenuItem_Click);
            // 
            // openInImageViewerToolStripMenuItem
            // 
            this.openInImageViewerToolStripMenuItem.Name = "openInImageViewerToolStripMenuItem";
            this.openInImageViewerToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openInImageViewerToolStripMenuItem.Text = "open in image viewer";
            this.openInImageViewerToolStripMenuItem.Click += new System.EventHandler(this.openInImageViewerToolStripMenuItem_Click);
            // 
            // openCmdHereToolStripMenuItem
            // 
            this.openCmdHereToolStripMenuItem.Name = "openCmdHereToolStripMenuItem";
            this.openCmdHereToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openCmdHereToolStripMenuItem.Text = "open cmd here";
            this.openCmdHereToolStripMenuItem.Click += new System.EventHandler(this.openCmdHereToolStripMenuItem_Click);
            // 
            // calcMemToolStripMenuItem
            // 
            this.calcMemToolStripMenuItem.Name = "calcMemToolStripMenuItem";
            this.calcMemToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.calcMemToolStripMenuItem.Text = "calc mem";
            this.calcMemToolStripMenuItem.Click += new System.EventHandler(this.calcMemToolStripMenuItem_Click);
            // 
            // findFileRepeatsToolStripMenuItem
            // 
            this.findFileRepeatsToolStripMenuItem.Name = "findFileRepeatsToolStripMenuItem";
            this.findFileRepeatsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.findFileRepeatsToolStripMenuItem.Text = "find file repeats";
            this.findFileRepeatsToolStripMenuItem.Click += new System.EventHandler(this.findFileRepeatsToolStripMenuItem_Click);
            // 
            // calcMd5ToolStripMenuItem
            // 
            this.calcMd5ToolStripMenuItem.Name = "calcMd5ToolStripMenuItem";
            this.calcMd5ToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.calcMd5ToolStripMenuItem.Text = "calc md5";
            this.calcMd5ToolStripMenuItem.Click += new System.EventHandler(this.CalcMd5ToolStripMenuItem_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.folderToolStripMenuItem,
            this.txtFileToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.newToolStripMenuItem.Text = "new";
            // 
            // folderToolStripMenuItem
            // 
            this.folderToolStripMenuItem.Name = "folderToolStripMenuItem";
            this.folderToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.folderToolStripMenuItem.Text = "folder";
            this.folderToolStripMenuItem.Click += new System.EventHandler(this.FolderToolStripMenuItem_Click);
            // 
            // txtFileToolStripMenuItem
            // 
            this.txtFileToolStripMenuItem.Name = "txtFileToolStripMenuItem";
            this.txtFileToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.txtFileToolStripMenuItem.Text = "txt file";
            this.txtFileToolStripMenuItem.Click += new System.EventHandler(this.TxtFileToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.deleteToolStripMenuItem.Text = "delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // makeLibraryToolStripMenuItem
            // 
            this.makeLibraryToolStripMenuItem.Name = "makeLibraryToolStripMenuItem";
            this.makeLibraryToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.makeLibraryToolStripMenuItem.Text = "make library";
            this.makeLibraryToolStripMenuItem.Click += new System.EventHandler(this.MakeLibraryToolStripMenuItem_Click);
            // 
            // setTagsToolStripMenuItem
            // 
            this.setTagsToolStripMenuItem.Name = "setTagsToolStripMenuItem";
            this.setTagsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.setTagsToolStripMenuItem.Text = "tags";
            this.setTagsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.SetTagsToolStripMenuItem_DropDownOpening);
            // 
            // addShortcutToolStripMenuItem
            // 
            this.addShortcutToolStripMenuItem.Name = "addShortcutToolStripMenuItem";
            this.addShortcutToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.addShortcutToolStripMenuItem.Text = "add shortcut";
            this.addShortcutToolStripMenuItem.Click += new System.EventHandler(this.AddShortcutToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(521, 357);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboBox2);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBox2);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(521, 80);
            this.panel1.TabIndex = 2;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(186, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Mode:";
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "Filesystem",
            "Libraries",
            "Tags"});
            this.comboBox2.Location = new System.Drawing.Point(224, 27);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(91, 21);
            this.comboBox2.TabIndex = 9;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.ComboBox2_SelectedIndexChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(3, 4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(42, 21);
            this.comboBox1.TabIndex = 8;
            this.comboBox1.DropDown += new System.EventHandler(this.ComboBox1_DropDown);
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(38, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "+tab";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(121, 31);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(59, 17);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "dir filter";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Location = new System.Drawing.Point(41, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(463, 25);
            this.panel2.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(41, 28);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(67, 20);
            this.textBox2.TabIndex = 1;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(47, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(457, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.listView1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 83);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(515, 271);
            this.panel3.TabIndex = 3;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeTabToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(138, 26);
            // 
            // removeTabToolStripMenuItem
            // 
            this.removeTabToolStripMenuItem.Name = "removeTabToolStripMenuItem";
            this.removeTabToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.removeTabToolStripMenuItem.Text = "Remove tab";
            this.removeTabToolStripMenuItem.Click += new System.EventHandler(this.removeTabToolStripMenuItem_Click);
            // 
            // FileListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FileListControl";
            this.Size = new System.Drawing.Size(521, 357);
            this.contextMenuStrip2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeTabToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem openExplorerHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInHexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInImageViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCmdHereToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolStripMenuItem calcMemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findFileRepeatsToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolStripMenuItem calcMd5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem folderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem txtFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ToolStripMenuItem makeLibraryToolStripMenuItem;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTagsToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStripMenuItem addShortcutToolStripMenuItem;
    }
}
