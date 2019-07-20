namespace commander
{
    partial class mdi
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.webToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fgToolStripMenuItem,
            this.toolStripMenuItem1,
            this.webToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.pluginsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1071, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fgToolStripMenuItem
            // 
            this.fgToolStripMenuItem.Name = "fgToolStripMenuItem";
            this.fgToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.fgToolStripMenuItem.Text = "File explorer";
            this.fgToolStripMenuItem.Click += new System.EventHandler(this.FgToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(52, 20);
            this.toolStripMenuItem1.Text = "About";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1_Click);
            // 
            // webToolStripMenuItem
            // 
            this.webToolStripMenuItem.Name = "webToolStripMenuItem";
            this.webToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.webToolStripMenuItem.Text = "Web";
            this.webToolStripMenuItem.Click += new System.EventHandler(this.WebToolStripMenuItem_Click);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            this.windowsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.WindowsToolStripMenuItem_DropDownOpening);
            this.windowsToolStripMenuItem.Click += new System.EventHandler(this.WindowsToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installToolStripMenuItem});
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            // 
            // installToolStripMenuItem
            // 
            this.installToolStripMenuItem.Name = "installToolStripMenuItem";
            this.installToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.installToolStripMenuItem.Text = "Install";
            this.installToolStripMenuItem.Click += new System.EventHandler(this.InstallToolStripMenuItem_Click);
            // 
            // mdi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1071, 653);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "mdi";
            this.Text = "Commander";
            this.Load += new System.EventHandler(this.Mdi_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem webToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installToolStripMenuItem;
    }
}