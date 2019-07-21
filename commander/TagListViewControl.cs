﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace commander
{
    public partial class TagListViewControl : UserControl
    {
        public TagListViewControl()
        {
            InitializeComponent();
            Stuff.SetDoubleBuffered(listView1);
        }


        public void Init(FileListControl fc)
        {
            parent = fc;
            listView1.KeyDown += ListView1_KeyDown;
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            ExecuteSelected();
        }

        FileListControl parent;

        public object tagRootObject = new object();
        public TagInfo SelectedTag
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is TagInfo)
                    {
                        return si as TagInfo;
                    }
                }

                return null;
            }
        }
        public void UpdateList(TagInfo tag)
        {
            if (tag == null)
            {
                listView1.Items.Clear();
                parent.CurrentTag = null;

                var filter = parent.Filter;
                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                foreach (var item in Stuff.Tags)
                {
                    if (!Stuff.ShowHidden && item.IsHidden) continue;
                    if (!IsFilterPass(item.Name, fltrs)) continue;
                    listView1.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                }
            }
            else
            {

                listView1.Items.Clear();
                //   fc.SetPath(path);
                //textBox1.Text = path;
                parent.CurrentTag = tag;
                parent.SetPath(tag.Name);
                var filter = parent.Filter;

                // var p = new DirectoryInfo(path);
                //   fc.CurrentDirectory = p;
                ImageList list = new ImageList();
                list.TransparentColor = Color.Black;
                var fltrs = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => z.ToLower()).ToArray();

                listView1.SmallImageList = list;
                listView1.LargeImageList = list;

                listView1.Items.Add(new ListViewItem(new string[] { "..", "", "" }) { Tag = tagRootObject });
                foreach (var finfo in tag.Files)
                {
                    try
                    {
                        var f = new FileInfo(finfo);
                        var bmp = FileListControl.GetBitmapOfFile(f.FullName);
                        bmp.MakeTransparent();
                        list.Images.Add(bmp);
                        if (!IsFilterPass(f.Name, fltrs)) continue;

                        listView1.Items.Add(
                            new ListViewItem(new string[]
                            {
                        f.Name, Stuff.GetUserFriendlyFileSize(f.Length)
                            })
                            {
                                Tag = f,
                                ImageIndex = list.Images.Count - 1

                            });
                    }
                    catch (FileNotFoundException ex)
                    {
                        listView1.Items.Add(
                            new ListViewItem(new string[]
                            {
                                Path.GetFileName(
                                    finfo)
                            })
                            {
                                BackColor = Color.LightPink,
                                Tag = ex,

                            });
                    }
                }
            }
        }
        public bool IsFilterPass(string str, string[] filters)
        {
            str = str.ToLower();
            if (filters.Length == 0) return true;
            return filters.Any(z => str.Contains(z));
        }
        public FileInfo SelectedFile
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var si = listView1.SelectedItems[0].Tag;
                    if (si is FileInfo)
                    {
                        return si as FileInfo;
                    }
                }

                return null;
            }
        }
        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedFile == null) return;
            if (parent.SelectedFileChanged != null)
            {
                parent.SelectedFileChanged(SelectedFile);
            }
        }

        private void AddTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stuff.AddTag(new TagInfo() { Name = "tag1" });
            UpdateList(null);
        }

        void ExecuteSelected()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    UpdateList(tag as TagInfo);
                }
                else
                if (listView1.SelectedItems[0].Tag == tagRootObject)
                {
                    UpdateList(null);
                }
                else if (tag is FileInfo)
                {
                    var f = tag as FileInfo;
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.WorkingDirectory = f.DirectoryName;
                    psi.FileName = f.FullName;
                    //Process.Start(f.FullName);
                    Process.Start(psi);
                }
            }
        }
        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExecuteSelected();
        }


        private void TrueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    if (!(tag as TagInfo).IsHidden)
                    {
                        (tag as TagInfo).IsHidden = true;
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }
                }
            }
        }

        private void FalseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    if ((tag as TagInfo).IsHidden)
                    {
                        (tag as TagInfo).IsHidden = false;
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }
                }
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var tag = listView1.SelectedItems[0].Tag;
                if (tag is TagInfo)
                {
                    var t = tag as TagInfo;
                    if (Stuff.Question("Are you sure to delete " + t.Name + " tag and all files?") == DialogResult.Yes)
                    {
                        Stuff.Tags.Remove(t);
                        Stuff.IsDirty = true;
                        UpdateList(null);
                    }
                }
                else if (tag is FileInfo)
                {
                    var f = tag as FileInfo;
                
                    SelectedTag.DeleteFile(f.FullName);
                    UpdateList(SelectedTag);
                }
            }
        }
    }
}