using System;
using System.Text.RegularExpressions;

namespace ProxyLib
{
    public class ItemContentType : ItemBase
    {

        private string _Value = "text/plain";
        private string _Charset = "utf8";

        
        public string Value
        {
            get
            {
                return _Value;
            }
        }

        
        public string Charset
        {
            get
            {
                return _Charset;
            }
        }

        public ItemContentType(string source) : base(source)
        {
            if (String.IsNullOrEmpty(source)) return;
            
            int typeTail = source.IndexOf(";");
            if (typeTail == -1)
            { 
                _Value = source.Trim().ToLower();
                return; 
            }
            _Value = source.Substring(0, typeTail).Trim().ToLower();
            
            string p = source.Substring(typeTail + 1, source.Length - typeTail - 1);
            Regex myReg = new Regex(@"(?<key>.+?)=((""(?<value>.+?)"")|((?<value>[^\;]+)))[\;]{0,1}", RegexOptions.Singleline);
            MatchCollection mc = myReg.Matches(p);
            foreach (Match m in mc)
            {
                if (m.Groups["key"].Value.Trim().ToLower() == "charset")
                {
                    _Charset = m.Groups["value"].Value;
                    
                }
            }
        }

    }
}
