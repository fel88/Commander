using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace commander
{
    public class Server : TcpRoutine
    {
        public void Init(int port)
        {
            InitTcp(IPAddress.Any, port, ThreadProcessor, () => new UserInfo());

        }

        public void ThreadProcessor(NetworkStream stream, object obj)
        {
            var cinfo = obj as ConnectionInfo;
            var uinfo = cinfo.Tag as UserInfo;
            StreamReader reader = new StreamReader(stream);
            BinaryReader breader = new BinaryReader(stream);
            BinaryWriter wrt2 = new BinaryWriter(stream);

            StreamWriter swrt2 = new StreamWriter(stream);

            TcpClient client = new TcpClient();


            BinaryWriter cwrt =null;
            BinaryReader crdr=null;
            List<string> lns = new List<string>();
            Thread tt = new Thread(() =>
            {
                while (true)
                {

                    lock (lns)
                    {
                        byte[] bb = new byte[1024];
                        var ln = crdr.Read(bb,0,bb.Length);
                        wrt2.Write(bb,0,bb.Length);
                        wrt2.Flush();
                    }
                }
            });
            tt.IsBackground = true;
           

            while (true)
            {
                try
                {
                    var line = reader.ReadLine();
                    /*if (cwrt != null)
                    {
                        cwrt.WriteLine(line);
                    }*/
                    /*lock (lns)
                    {
                        foreach (var item in lns)
                        {
                          
                        }
                        lns.Clear();
                    }*/


                    cinfo.Log.Add(line);
                    if (line.StartsWith("CONNECT"))
                    {
                       
                        var arr1 = line.Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var addr = arr1[1];
                        if (addr.Contains("wiki"))
                        {
                            var port = arr1[2];

                            client.Connect(addr, int.Parse(port));
                            var strm = client.GetStream();
                            cwrt = new BinaryWriter(strm);
                            crdr = new BinaryReader(strm);
                            tt.Start();
                            swrt2.WriteLine("HTTP/1.1 200 OK");
                            swrt2.Flush();
                        }
                    }

                   



                }
                /*catch (IOException)
             {
                 break;
             }*/
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;

                    TcpRoutine.ErrorSend(stream);
                }
            }

        }

    }
}
