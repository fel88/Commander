using System;
using System.Collections.Generic;

namespace ProxyLib
{
    public class ItemCollection : Dictionary<string, ItemBase>
    {
        public ItemCollection() : base(StringComparer.CurrentCultureIgnoreCase) { }

        public void AddItem(string key, string source)
        {
            switch (key.Trim().ToLower())
            {
                case "host":
                    
                    this.Add(key, new ItemHost(source));
                    break;

                case "content-type":
                    
                    this.Add(key, new ItemContentType(source));
                    break;

                default:
                    
                    this.Add(key, new ItemBase(source));
                    break;
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (string k in this.Keys)
            {
                ItemBase itm = this[k];
                if (!String.IsNullOrEmpty(result)) result += "\r\n";
                result += String.Format("{0}: {1}", k, itm.Source);
            }
            return result;
        }
    }
}
