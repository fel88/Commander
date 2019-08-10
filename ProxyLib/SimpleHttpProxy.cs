using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ProxyLib
{
    public class SimpleHttpProxyServer
    {

        private static string _ProxyAddress = "127.0.0.1";
        private static int _ProxyPort = 8888;

        private static bool _NeedAuth = false;
        private static string _Login = "test";
        private static string _Password = "123123";

        private static bool _AppendHtml = false;

        private static bool _AllowBlackList = true;
        private static string[] _BlackList = null;

        public static bool UseCache;
        public static bool AllowStoreInCache;
        public static IProxyCache Cache;




        private static byte[] GetHTTPError(int statusCode, string statusMessage)
        {
            FileInfo FI = new FileInfo(String.Format("HTTP{0}.htm", statusCode));

            byte[] headers = Encoding.ASCII.GetBytes(String.Format("HTTP/1.1 {0} {1}\r\n{3}Content-Type: text/html\r\nContent-Length: {2}\r\n\r\n", statusCode, statusMessage, FI.Length, (statusCode == 401 ? "WWW-Authenticate: Basic realm=\"ProxyServer Example\"\r\n" : "")));
            byte[] result = null;

            // search for html-file HTTP[error code].htm
            using (FileStream fs = new FileStream(FI.FullName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8))
                {
                    result = new byte[headers.Length + fs.Length];
                    Buffer.BlockCopy(headers, 0, result, 0, headers.Length);
                    Buffer.BlockCopy(br.ReadBytes(Convert.ToInt32(fs.Length)), 0, result, headers.Length, Convert.ToInt32(fs.Length));
                }
            }


            return result;
        }


        private static void WriteLog(string msg, params object[] args)
        {
            Console.WriteLine(DateTime.Now.ToString() + " : " + msg, args);
        }
        public static void Run()
        {

            WriteLog("Starting server...");


            if (_AllowBlackList && File.Exists("BlackList.txt")) _BlackList = File.ReadAllLines("BlackList.txt");
            // --

            TcpListener myTCP = new TcpListener(IPAddress.Parse(_ProxyAddress), _ProxyPort);

            myTCP.Start();

            WriteLog("Proxy-server successfuly started, waiting requests to {0}:{1}.", _ProxyAddress, _ProxyPort);
            while (true)
            {
                if (myTCP.Pending())
                {
                    Thread t = new Thread(ExecuteRequest);
                    t.IsBackground = true;
                    t.Start(myTCP.AcceptSocket());
                }
            }

            myTCP.Stop();

        }
        private static void ExecuteRequest(object arg)
        {
            try
            {
                using (Socket myClient = (Socket)arg)
                {
                    if (myClient.Connected)
                    {

                        byte[] httpRequest = ReadToEnd(myClient);


                        Parser http = new Parser(httpRequest);

                        if (http.Items == null || http.Items.Count <= 0 || !http.Items.ContainsKey("Host"))
                        {
                            WriteLog("REQUEST {0} bytes, headers not found.", httpRequest.Length);
                        }
                        else
                        {

                            WriteLog("REQUEST {0} bytes, method{1}, host {2}:{3}", httpRequest.Length, http.Method, http.Host, http.Port);
                            // WriteLog(http.GetHeadersAsString());


                            byte[] response = null;


                            if (_NeedAuth)
                            {
                                if (!http.Items.ContainsKey("Authorization"))
                                {

                                    response = GetHTTPError(401, "Unauthorized");
                                    myClient.Send(response, response.Length, SocketFlags.None);
                                    return;
                                }
                                else
                                {

                                    string auth = Encoding.UTF8.GetString(Convert.FromBase64String(http.Items["Authorization"].Source.Replace("Basic ", "")));
                                    string login = auth.Split(":".ToCharArray())[0];
                                    string pwd = auth.Split(":".ToCharArray())[1];
                                    if (_Login != login || _Password != pwd)
                                    {
                                        // wrong login or password
                                        response = GetHTTPError(401, "Unauthorized");
                                        myClient.Send(response, response.Length, SocketFlags.None);
                                        return;
                                    }
                                }
                            }

                            //check blacklist
                            if (_AllowBlackList && _BlackList != null && Array.IndexOf(_BlackList, http.Host.ToLower()) != -1)
                            {

                                response = GetHTTPError(403, "Forbidden");
                                myClient.Send(response, response.Length, SocketFlags.None);
                                return;
                            }


                            bool ready = false;
                            if (UseCache)
                            {
                                if (Cache.HasResource(http.Path))
                                {
                                    ready = true;
                                    response = Cache.GetData(http.Path);
                                    if (response != null) myClient.Send(response, response.Length, SocketFlags.None);
                                }
                            }
                            if (!ready)
                            {

                                IPHostEntry myIPHostEntry = Dns.GetHostEntry(http.Host);

                                if (myIPHostEntry == null || myIPHostEntry.AddressList == null || myIPHostEntry.AddressList.Length <= 0)
                                {
                                    WriteLog("Can't determine IP-adress by host {0}.", http.Host);
                                }
                                else
                                {

                                    IPEndPoint myIPEndPoint = new IPEndPoint(myIPHostEntry.AddressList[0], http.Port);


                                    if (http.Method == Parser.MethodsList.CONNECT)
                                    {

                                        WriteLog("Protocol HTTPS not impemented.");
                                        response = GetHTTPError(501, "Not Implemented");
                                    } // HTTP.Parser.MethodsList.CONNECT
                                    else
                                    {


                                        //if (!ready)
                                        {
                                            //redirect
                                            using (Socket myRerouting = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                                            {
                                                myRerouting.Connect(myIPEndPoint);
                                                if (myRerouting.Send(httpRequest, httpRequest.Length, SocketFlags.None) != httpRequest.Length)
                                                {
                                                    WriteLog("Data to host {0} was not sended...", http.Host);
                                                }
                                                else
                                                {
                                                    //getting answer
                                                    Parser httpResponse = new Parser(ReadToEnd(myRerouting));
                                                    if (httpResponse.Source != null && httpResponse.Source.Length > 0)
                                                    {
                                                        WriteLog("Request recieved {0} bytes, status code {1}", httpResponse.Source.Length, httpResponse.StatusCode);
                                                        // WriteLog(httpResponse.GetHeadersAsString());


                                                        response = httpResponse.Source;


                                                        switch (httpResponse.StatusCode)
                                                        {
                                                            case 400:
                                                            case 403:
                                                            case 404:
                                                            case 407:
                                                            case 500:
                                                            case 501:
                                                            case 502:
                                                            case 503:
                                                                response = GetHTTPError(httpResponse.StatusCode, httpResponse.StatusMessage);
                                                                break;

                                                            default:

                                                                if (_AppendHtml)
                                                                {
                                                                    //check if html
                                                                    if (httpResponse.Items.ContainsKey("Content-Type") && ((ItemContentType)httpResponse.Items["Content-Type"]).Value == "text/html")
                                                                    {
                                                                        //get body
                                                                        string body = httpResponse.GetBodyAsString();


                                                                        body = Regex.Replace(body, "<title>(?<title>.*?)</title>", "<title>ProxyServer - $1</title>");


                                                                        body = Regex.Replace(body, "(<body.*?>)", "$1<div style='height:20px;width:100%;background-color:black;color:white;font-weight:bold;text-align:center;'>Example of Proxy Server</div>");


                                                                        httpResponse.SetStringBody(body);


                                                                        response = httpResponse.Source;
                                                                    }
                                                                }
                                                                //you can change image data too
                                                                break;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        WriteLog("Recieved answer 0 byte");
                                                    }

                                                }
                                                myRerouting.Close();
                                            }
                                        } // HTTP.Parser.MethodsList.CONNECT
                                        if (AllowStoreInCache)
                                        {
                                            Cache.SetData(http.Path, response);
                                        }
                                    }

                                    if (response != null) myClient.Send(response, response.Length, SocketFlags.None);
                                }
                            }
                        }


                        myClient.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("Error: ", ex.Message);
            }
        }

        private static byte[] ReadToEnd(Socket mySocket)
        {
            byte[] b = new byte[mySocket.ReceiveBufferSize];
            int len = 0;
            using (MemoryStream m = new MemoryStream())
            {
                while (mySocket.Poll(1000000, SelectMode.SelectRead) && (len = mySocket.Receive(b, mySocket.ReceiveBufferSize, SocketFlags.None)) > 0)
                {
                    m.Write(b, 0, len);
                }
                return m.ToArray();
            }
        }
    }


    public interface IProxyCache
    {
        void SetData(string url, byte[] data);
        bool HasResource(string url);
        byte[] GetData(string url);
    }
}
