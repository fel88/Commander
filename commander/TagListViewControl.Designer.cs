﻿namespace commander
{
    partial class TagListViewControl
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setHiddenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.falseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.operationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indexToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ocrToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deduplicationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.memToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.addSynonymToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toIsoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.followToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imgDedupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(447, 286);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 250;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 150;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTagToolStripMenuItem,
            this.setHiddenToolStripMenuItem,
            this.operationsToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.packToolStripMenuItem,
            this.copyPathToolStripMenuItem,
            this.followToolStripMenuItem,
            this.tagPanelToolStripMenuItem,
            this.propertiesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 224);
            // 
            // addTagToolStripMenuItem
            // 
            this.addTagToolStripMenuItem.Name = "addTagToolStripMenuItem";
            this.addTagToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addTagToolStripMenuItem.Text = "add tag";
            this.addTagToolStripMenuItem.Click += new System.EventHandler(this.AddTagToolStripMenuItem_Click);
            // 
            // setHiddenToolStripMenuItem
            // 
            this.setHiddenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trueToolStripMenuItem,
            this.falseToolStripMenuItem});
            this.setHiddenToolStripMenuItem.Name = "setHiddenToolStripMenuItem";
            this.setHiddenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.setHiddenToolStripMenuItem.Text = "set hide";
            // 
            // trueToolStripMenuItem
            // 
            this.trueToolStripMenuItem.Name = "trueToolStripMenuItem";
            this.trueToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.trueToolStripMenuItem.Text = "true";
            this.trueToolStripMenuItem.Click += new System.EventHandler(this.TrueToolStripMenuItem_Click);
            // 
            // falseToolStripMenuItem
            // 
            this.falseToolStripMenuItem.Name = "falseToolStripMenuItem";
            this.falseToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.falseToolStripMenuItem.Text = "false";
            this.falseToolStripMenuItem.Click += new System.EventHandler(this.FalseToolStripMenuItem_Click);
            // 
            // operationsToolStripMenuItem
            // 
            this.operationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indexToolStripMenuItem1,
            this.ocrToolStripMenuItem1,
            this.deduplicationToolStripMenuItem1,
            this.memToolStripMenuItem1,
            this.addSynonymToolStripMenuItem,
            this.imgDedupToolStripMenuItem});
            this.operationsToolStripMenuItem.Name = "operationsToolStripMenuItem";
            this.operationsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.operationsToolStripMenuItem.Text = "operations";
            // 
            // indexToolStripMenuItem1
            // 
            this.indexToolStripMenuItem1.Name = "indexToolStripMenuItem1";
            this.indexToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.indexToolStripMenuItem1.Text = "index";
            this.indexToolStripMenuItem1.Click += new System.EventHandler(this.IndexToolStripMenuItem1_Click);
            // 
            // ocrToolStripMenuItem1
            // 
            this.ocrToolStripMenuItem1.Name = "ocrToolStripMenuItem1";
            this.ocrToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.ocrToolStripMenuItem1.Text = "ocr";
            this.ocrToolStripMenuItem1.Click += new System.EventHandler(this.OcrToolStripMenuItem1_Click);
            // 
            // deduplicationToolStripMenuItem1
            // 
            this.deduplicationToolStripMenuItem1.Name = "deduplicationToolStripMenuItem1";
            this.deduplicationToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.deduplicationToolStripMenuItem1.Text = "deduplication";
            this.deduplicationToolStripMenuItem1.Click += new System.EventHandler(this.DeduplicationToolStripMenuItem1_Click);
            // 
            // memToolStripMenuItem1
            // 
            this.memToolStripMenuItem1.Name = "memToolStripMenuItem1";
            this.memToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.memToolStripMenuItem1.Text = "mem";
            this.memToolStripMenuItem1.Click += new System.EventHandler(this.MemToolStripMenuItem1_Click);
            // 
            // addSynonymToolStripMenuItem
            // 
            this.addSynonymToolStripMenuItem.Name = "addSynonymToolStripMenuItem";
            this.addSynonymToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addSynonymToolStripMenuItem.Text = "add synonym";
            this.addSynonymToolStripMenuItem.Click += new System.EventHandler(this.AddSynonymToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteToolStripMenuItem.Text = "delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // packToolStripMenuItem
            // 
            this.packToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toIsoToolStripMenuItem});
            this.packToolStripMenuItem.Name = "packToolStripMenuItem";
            this.packToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.packToolStripMenuItem.Text = "pack";
            // 
            // toIsoToolStripMenuItem
            // 
            this.toIsoToolStripMenuItem.Name = "toIsoToolStripMenuItem";
            this.toIsoToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.toIsoToolStripMenuItem.Text = "to iso";
            this.toIsoToolStripMenuItem.Click += new System.EventHandler(this.ToIsoToolStripMenuItem_Click);
            // 
            // copyPathToolStripMenuItem
            // 
            this.copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            this.copyPathToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyPathToolStripMenuItem.Text = "copy path";
            this.copyPathToolStripMenuItem.Click += new System.EventHandler(this.copyPathToolStripMenuItem_Click);
            // 
            // followToolStripMenuItem
            // 
            this.followToolStripMenuItem.Name = "followToolStripMenuItem";
            this.followToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.followToolStripMenuItem.Text = "follow";
            this.followToolStripMenuItem.Click += new System.EventHandler(this.FollowToolStripMenuItem_Click);
            // 
            // tagPanelToolStripMenuItem
            // 
            this.tagPanelToolStripMenuItem.Name = "tagPanelToolStripMenuItem";
            this.tagPanelToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.tagPanelToolStripMenuItem.Text = "tag panel";
            this.tagPanelToolStripMenuItem.Click += new System.EventHandler(this.TagPanelToolStripMenuItem_Click);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.propertiesToolStripMenuItem.Text = "properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.PropertiesToolStripMenuItem_Click);
            // 
            // imgDedupToolStripMenuItem
            // 
            this.imgDedupToolStripMenuItem.Name = "imgDedupToolStripMenuItem";
            this.imgDedupToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.imgDedupToolStripMenuItem.Text = "img dedup";
            this.imgDedupToolStripMenuItem.Click += new System.EventHandler(this.ImgDedupToolStripMenuItem_Click);
            // 
            // TagListViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Name = "TagListViewControl";
            this.Size = new System.Drawing.Size(447, 286);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setHiddenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem falseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem packToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toIsoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem followToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagPanelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem operationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ocrToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem indexToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deduplicationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem memToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSynonymToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imgDedupToolStripMenuItem;
    }
}
