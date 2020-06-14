using PluginLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace commander
{
    public class FileIndex
    {
        public bool ContainsFile(IFileInfo f)
        {
            return _fileEntries.Any(z => z.FullName.ToLower() == f.FullName.ToLower());            
        }
        
        List<FileEntry> _fileEntries = new List<FileEntry>();

        public string FileName;
        public string RootPath;
        public void Load(Stream st, IDirectoryInfo root, string fname)
        {
            FileName = fname;
            RootPath = root.FullName;
            var s = XDocument.Load(st);
            var fr = s.Descendants("settings").First();

            List<DirectoryEntry> direntries = new List<DirectoryEntry>();
            #region load directory and files entries
            var entries = s.Descendants("entries").First();
            foreach (var item in entries.Descendants("directory"))
            {
                var id = int.Parse(item.Attribute("id").Value);
                var path = item.Value;
                //var dir = new DirectoryInfo(path);
                //path = dir.Parent.GetDirectories(dir.Name).First().FullName;

                direntries.Add(new DirectoryEntry() { Id = id, Path = Path.Combine(root.FullName, path) });
            }
            foreach (var item in entries.Descendants("file"))
            {
                var id = int.Parse(item.Attribute("id").Value);
                var dirId = int.Parse(item.Attribute("dirId").Value);
                var name = item.Value;

                var dir = direntries.First(z => z.Id == dirId);
                //var path = Path.Combine(dir.Path, name);
                //var diri = new DirectoryInfo(dir.Path);                
                //name = diri.GetFiles(name).First().Name;

                _fileEntries.Add(new FileEntry() { Id = id, Directory = dir, Name = name });
            }
            #endregion


            #region meta

            var metas = s.Descendants("meta").FirstOrDefault();
            if (metas != null)
            {
                foreach (var item in metas.Descendants("file"))
                {
                    var fid = int.Parse(item.Attribute("fileId").Value);
                    var f = _fileEntries.First(z => z.Id == fid);
                    Stuff.MetaInfos.Add(new FileMetaInfo() { File = new FileInfoWrapper(new FileInfo(f.FullName)) });
                    var minf = Stuff.MetaInfos.Last();

                    foreach (var kitem in item.Descendants())
                    {
                        if (kitem.Name == "keywordsMetaInfo")
                        {
                            minf.Infos.Add(new KeywordsMetaInfo() { Parent = minf, Keywords = kitem.Value });
                        }

                    }

                }
            }
            #endregion


            foreach (var descendant in s.Descendants("tag"))
            {
                var name = descendant.Attribute("name").Value;

                string flags = "";
                if (descendant.Attribute("flags") != null) { flags = descendant.Attribute("flags").Value; }

                var tag = new TagInfo() { Name = name, IsHidden = flags.Contains("hidden") };

                var snms = descendant.Descendants("synonym");
                foreach (var item in snms)
                {
                    tag.Synonyms.Add(item.Value.Trim());
                }

                tag = Stuff.AddTag(tag);
                foreach (var item in descendant.Descendants("file"))
                {
                    var arr1 = item.Attribute("id").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    foreach (var aitem in arr1)
                    {
                        var ff = _fileEntries.First(z => z.Id == aitem);
                        tag.AddFile(new FileInfoWrapper(ff.FullName), false);
                    }
                }
            }
        }
    }


}