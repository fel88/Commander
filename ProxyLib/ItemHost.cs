using System;
using System.Text.RegularExpressions;

namespace ProxyLib
{
    public class ItemHost : ItemBase
    {

        private string _Host = String.Empty;
        private int _Port = 80;

        
        public string Host
        {
            get
            {
                return _Host;
            }
        }

        
        public int Port
        {
            get
            {
                return _Port;
            }
        }

        public ItemHost(string source) : base(source)
        {            
            Regex myReg = new Regex(@"^(((?<host>.+?):(?<port>\d+?))|(?<host>.+?))$");
            Match m = myReg.Match(source);
            _Host = m.Groups["host"].Value;
            if (!int.TryParse(m.Groups["port"].Value, out _Port))
            { 
                _Port = 80;
            }
        }

    }
}
