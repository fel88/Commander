//using ProxyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace commander
{
    //public class SimpleProxyCache : IProxyCache
    //{

    //    public void Store(string path)
    //    {
    //        lock (Cache)
    //        {
    //            StringBuilder sb = new StringBuilder();
    //            sb.AppendLine("<?xml version=\"1.0\"?>");
    //            sb.AppendLine("<root>");
    //            foreach (var item in Cache)
    //            {
    //                sb.AppendLine($"<item key=\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Key))}\">");
    //                var b64 = Convert.ToBase64String(item.Value);
    //                sb.AppendLine(b64);
    //                sb.AppendLine($"</item>");
    //            }

    //            sb.AppendLine("</root>");

    //            File.WriteAllText(path, sb.ToString());
    //        }
    //    }
    //    public void Restore(string path)
    //    {
    //        lock (Cache)
    //        {
    //            XDocument doc = XDocument.Load(path);
    //            Cache.Clear();
    //            foreach (var item in doc.Descendants("item"))
    //            {
    //                var key = item.Attribute("key").Value;
    //                key = Encoding.UTF8.GetString(Convert.FromBase64String(key));
    //                var val = item.Value;
    //                var b = Convert.FromBase64String(val);
    //                Cache.Add(key, b);
    //            }
    //        }
    //    }

    //    public byte[] GetData(string url)
    //    {
    //        lock (Cache)
    //        {
    //            return Cache[url];
    //        }
    //    }

    //    public Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();
    //    public bool HasResource(string url)
    //    {
    //        lock (Cache)
    //        {
    //            return Cache.ContainsKey(url);
    //        }
    //    }

    //    public void SetData(string url, byte[] data)
    //    {
    //        lock (Cache)
    //        {
              
    //            if (data.Length == 0) return;
    //            if (HasResource(url)) return;
    //            if (url.Contains("forces"))
    //            {
    //                Cache.Add(url, data);
    //            }
    //        }
    //    }
    //}

}



