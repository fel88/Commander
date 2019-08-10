using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyLib
{
    public class Parser
    {
        public enum MethodsList
        {            
            GET,            
            POST,
            /// <summary>
            /// https
            /// </summary>
            CONNECT
            // others
        }

        private MethodsList _Method = MethodsList.GET;
        private string _HTTPVersion = "1.1";
        private ItemCollection _Items = null;
        private string _Path = String.Empty;
        private byte[] _Source = null;

        private int _StatusCode = 0;
        private string _StatusMessage = string.Empty;

        private int _HeadersTail = -1;

        
        public MethodsList Method
        {
            get
            {
                return _Method;
            }
        }

        
        public string Path
        {
            get
            {
                return _Path;
            }
        }

        /// <summary>
        /// Headers
        /// </summary>
        public ItemCollection Items
        {
            get
            {
                return _Items;
            }
        }

        
        public byte[] Source
        {
            get
            {
                return _Source;
            }
        }

        
        public string Host
        {
            get
            {
                if (!_Items.ContainsKey("Host")) return String.Empty;
                return ((ItemHost)_Items["Host"]).Host;
            }
        }

        
        public int Port
        {
            get
            {
                if (!_Items.ContainsKey("Host")) return 80;
                return ((ItemHost)_Items["Host"]).Port;
            }
        }

        
        public int StatusCode
        {
            get
            {
                return _StatusCode;
            }
        }

        
        public string StatusMessage
        {
            get
            {
                return _StatusMessage;
            }
        }

        public Parser(byte[] source)
        {
            if (source == null || source.Length <= 0) return;
            _Source = source;

            _Items = new ItemCollection();
            // data to text
            string sourceString = GetSourceAsString();

            // request
            // first line containt request method, path and version of HTTP protocol
            string httpInfo = sourceString.Substring(0, sourceString.IndexOf("\r\n"));
            Regex myReg = new Regex(@"(?<method>.+)\s+(?<path>.+)\s+HTTP/(?<version>[\d\.]+)", RegexOptions.Multiline);
            
            
            Console.WriteLine(httpInfo);
            if (myReg.IsMatch(httpInfo))
            {
                Match m = myReg.Match(httpInfo);
                if (m.Groups["method"].Value.ToUpper() == "POST")
                {
                    _Method = MethodsList.POST;
                }
                else if (m.Groups["method"].Value.ToUpper() == "CONNECT")
                {
                    _Method = MethodsList.CONNECT;
                }
                else
                {
                    _Method = MethodsList.GET;
                }
                // also you can detect method like this 
                // _Method = (MethodsList)Enum.Parse(typeof(MethodsList), m.Groups["method"].Value.ToUpper());
                

                _Path = m.Groups["path"].Value;
                _HTTPVersion = m.Groups["version"].Value;
            }
            else
            {
                // answer
                // first line contain status code
                myReg = new Regex(@"HTTP/(?<version>[\d\.]+)\s+(?<status>\d+)\s*(?<msg>.*)", RegexOptions.Multiline);
                Match m = myReg.Match(httpInfo);
                int.TryParse(m.Groups["status"].Value, out _StatusCode);
                _StatusMessage = m.Groups["msg"].Value;
                _HTTPVersion = m.Groups["version"].Value;
            }

            // extract headers (until two new line symbols)
            _HeadersTail = sourceString.IndexOf("\r\n\r\n");
            if (_HeadersTail != -1)
            { // tail was found, separate headers
                sourceString = sourceString.Substring(sourceString.IndexOf("\r\n") + 2, _HeadersTail - sourceString.IndexOf("\r\n") - 2);
            }

            // parse headers and put its to collection
            myReg = new Regex(@"^(?<key>[^\x3A]+)\:\s{1}(?<value>.+)$", RegexOptions.Multiline);
            MatchCollection mc = myReg.Matches(sourceString);
            foreach (Match mm in mc)
            {
                string key = mm.Groups["key"].Value;
                if (!_Items.ContainsKey(key))
                {                    
                    _Items.AddItem(key, mm.Groups["value"].Value.Trim("\r\n ".ToCharArray()));
                }
            }
        }

        /// <summary>
        /// return original data as string
        /// </summary>
        public string GetSourceAsString()
        {
            Encoding e = Encoding.UTF8;
            // detect encoding
            if (_Items != null && _Items.ContainsKey("Content-Type") && !String.IsNullOrEmpty(((ItemContentType)_Items["Content-Type"]).Charset))
            {
                //use encoding to decode
                try
                {
                    e = Encoding.GetEncoding(((ItemContentType)_Items["Content-Type"]).Charset);
                }
                catch { }
            }
            return e.GetString(_Source);
        }

        /// <summary>
        /// returns headers as string
        /// </summary>
        /// <returns></returns>
        public string GetHeadersAsString()
        {
            if (_Items == null) return String.Empty;
            return _Items.ToString();
        }

        /// <summary>
        /// returns content as byte array
        /// </summary>
        public byte[] GetBody()
        {
            // if there is no tail then body also not exist
            if (_HeadersTail == -1) return null;
            // extract body, starting from end
            byte[] result = new byte[_Source.Length - _HeadersTail - 4];
            Buffer.BlockCopy(_Source, _HeadersTail + 4, result, 0, result.Length);
            // body can be zipped , check it
            if (_Items != null && _Items.ContainsKey("Content-Encoding") && _Items["Content-Encoding"].Source.ToLower() == "gzip")
            {
                //uncompress
                GZipStream myGzip = new GZipStream(new MemoryStream(result), CompressionMode.Decompress);
                using (MemoryStream m = new MemoryStream())
                {
                    byte[] buffer = new byte[512];
                    int len = 0;
                    while ((len = myGzip.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        m.Write(buffer, 0, len);
                    }
                    result = m.ToArray();
                }
            }
            
            return result;
        }

        /// <summary>
        /// return body as string
        /// </summary>
        public string GetBodyAsString()
        {
            Encoding e = Encoding.UTF8;
            
            if (_Items != null && _Items.ContainsKey("Content-Type") && !String.IsNullOrEmpty(((ItemContentType)_Items["Content-Type"]).Charset))
            {                
                try
                {
                    e = Encoding.GetEncoding(((ItemContentType)_Items["Content-Type"]).Charset);
                }
                catch { }
            }
            return e.GetString(GetBody());
        }

        
        public void SetStringBody(string newBody)
        {
            
            if (_StatusCode <= 0)
            {                
                throw new Exception("error.");
            }
            Encoding e = Encoding.UTF8;
            string result = String.Format("HTTP/{0} {1} {2}", _HTTPVersion, _StatusCode, _StatusMessage);
            foreach (string k in _Items.Keys)
            {
                ItemBase itm = _Items[k];
                if (!String.IsNullOrEmpty(result)) result += "\r\n";
                if (k.ToLower() == "content-length")
                {                    
                    result += String.Format("{0}: {1}", k, newBody.Length);
                }
                else if (k.ToLower() == "content-encoding" && itm.Source.ToLower() == "gzip")
                {
                    
                }
                else
                {                    
                    result += String.Format("{0}: {1}", k, itm.Source);                    
                    if (k.ToLower() == "content-type" && !String.IsNullOrEmpty(((ItemContentType)_Items["Content-Type"]).Charset))
                    {                        
                        try
                        {
                            e = Encoding.GetEncoding(((ItemContentType)_Items["Content-Type"]).Charset);
                        }
                        catch { }
                    }
                }
            }
            
            result += "\r\n\r\n";            
            result += newBody;            
            _Source = e.GetBytes(result);
        }

    }
}
