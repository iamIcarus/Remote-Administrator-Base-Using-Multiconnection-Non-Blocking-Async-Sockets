using Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Client.Helpers
{
    public class ConnectionManager
    {
        public EventHandler<ConnectionEventArgs> OnServerChange;
        public EventHandler<Responce> OnResponceReceived;

        private const int PORT = 100;
        private const int BUFFER_SIZE = 2048;
        private const int PING_INTERVAL = 5000;

        private static readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Dictionary<Guid, Socket> connectedSockets = new Dictionary<Guid, Socket>();
        private static readonly Dictionary<Guid, DateTime> pingTimeTable = new Dictionary<Guid, DateTime>();

        private static System.Timers.Timer CheckConnectionsTimer;

        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public ConnectionManager()
        {
            clientSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            clientSocket.Listen(0);
            clientSocket.BeginAccept(AcceptCallback, null);


            CheckConnectionsTimer = new System.Timers.Timer(5000);
            CheckConnectionsTimer.Elapsed += CheckConnectionsEvent;
            CheckConnectionsTimer.AutoReset = true;
            CheckConnectionsTimer.Enabled = true;

        }
          public void CloseAllSockets()
        {
            foreach (var entry in connectedSockets)
            {
                var socket = entry.Value;
                socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                socket.Close();
            }

            clientSocket.Close();
        }


          void CheckConnectionsEvent(Object source, System.Timers.ElapsedEventArgs e)
          {
              //List my alter wile thread is running

              try
              {
                  foreach (var entry in connectedSockets)
                      Task.Factory.StartNew(() => { Ping(entry.Value); });
              }
              catch(Exception ex)
              {

              }
          }


          private void AcceptCallback(IAsyncResult AR)
          {
              Socket socket;

              try
              {
                  socket = clientSocket.EndAccept(AR);
              }
              catch (ObjectDisposedException)
              {
                  return;
              }

              // Generate new Id
              Guid Id = Guid.NewGuid();

              //Add server to sockets
              connectedSockets.Add(Id,socket);
              socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
              clientSocket.BeginAccept(AcceptCallback, null);

              //Notify Client
              TriggerOnServerChange(Id, 0);

              Task.Factory.StartNew(() => { RequestInfo(socket); });
          }
          
          private void RequestInfo(Socket sock)
          {
              JavaScriptSerializer js = new JavaScriptSerializer();
              string json = js.Serialize(new Message() { Code = "Request Info", Data = String.Empty });
              byte[] msg = Encoding.ASCII.GetBytes(json);

              SendData(msg, sock);
          }

          private void Ping(Socket sock)
          {
              Guid Id = connectedSockets.SingleOrDefault(x => x.Value == sock).Key;

              JavaScriptSerializer js = new JavaScriptSerializer();
              string json = js.Serialize(new Message() { Code = "Ping", Data = String.Empty });
              byte[] msg = Encoding.ASCII.GetBytes(json);


              //Insert Time into TimeTable
              if (pingTimeTable.ContainsKey(Id))
                  pingTimeTable[Id] = DateTime.Now;
              else
                  pingTimeTable.Add(Id, DateTime.Now);

              SendData(msg, sock);
          }

          private void SendData(byte[] msg, Socket sock)
          {
              try
              {
                  int ret = sock.Send(msg, 0, msg.Length, SocketFlags.None);

                  if (ret > 0)
                      return;
              }
              catch (Exception e)
              {
                    //Log Error
              }

              CloseSocket(sock);
          }

          private void CloseSocket(Socket sock)
          {
              Guid Id = connectedSockets.SingleOrDefault(x => x.Value == sock).Key;

              TriggerOnServerChange(Id, 1); // Notify UI to remove from list
              sock.Close();
              connectedSockets.Remove(Id);
          }

          private void ReceiveCallback(IAsyncResult AR)
          {
              Socket current = (Socket)AR.AsyncState;

              //Get the Socket Id
              Guid Id = connectedSockets.SingleOrDefault(x => x.Value == current).Key;

              int size;
              
              try
              {
                  if (!current.Connected)
                  {
                      CloseSocket(current);
                      return;
                  }

                  size = current.EndReceive(AR);
              }
              catch (SocketException)
              {
                  //Client forcefully disconnected , close the connection
                  CloseSocket(current);
                  return;
              }

              HandleMessage(ParseMessage(size), Id);


              try
              {
                  current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
              }
              catch(Exception e)
              {
                  CloseSocket(current);
                  return;
              }
          }

          private void HandleMessage(Message msg, Guid SocketId)
          {
              if (msg == null || msg.Code == null)
                  return;

                switch(msg.Code)
                {
                    case "Error":
                        break;

                    case "Pong":
                        handlePongResponce(SocketId);
                        break;

                    case "Request Info":
                        handleRequestInfo(SocketId, msg);
                        break;


                    default:
                        break;
                }



          }



        private Message ParseMessage(int size)
          {
              byte[] recBuf = new byte[size];
              Array.Copy(buffer, recBuf, size);
              string text = Encoding.ASCII.GetString(recBuf);

            Message ret = new Message();
              try
              {
                  JavaScriptSerializer js = new JavaScriptSerializer();
                  ret = js.Deserialize<Message>(text);
              }
            catch
              {
                  ret.Code = "Error";
                  ret.Data = "";
              }

              return ret;
          }

        #region Broadcast
        public void BroadcastCommand()
        {
            Task.Factory.StartNew(() =>
            {
                String Data = string.Format("Start|{0}|{1}|{2}|{3}", "d1", "d2", "d3", "d4");

                foreach (var entry in connectedSockets)
                {
                    var socket = entry.Value;

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string json = js.Serialize(new Message() { Code = "Command", Data = Data });
                    byte[] msg = Encoding.ASCII.GetBytes(json);

                    SendData(msg,socket);        
                }


            });

        }

        #endregion


        #region Handle Responces
        private void handleRequestInfo(Guid SocketId, Message msg)
        {

            Socket  sock = connectedSockets[SocketId];
            List<object> ret = new List<object>();
            ret.Add(msg.Data);
            ret.Add(((System.Net.IPEndPoint)sock.RemoteEndPoint).Address.ToString());

            TriggerOnResponceReceived(SocketId, "Request Info", ret);
        }
        void handlePongResponce(Guid SocketId)
        {

            if (!pingTimeTable.ContainsKey(SocketId))
                return;


            DateTime timeNow = DateTime.Now;


            DateTime timeSend = pingTimeTable[SocketId];

            double diffInMilisec = (timeNow - timeSend).TotalMilliseconds;

            List<object> ret = new List<object>();
            ret.Add(diffInMilisec);

            TriggerOnResponceReceived(SocketId, "Ping", ret);
        }
        #endregion


        #region Event Notifications
        private void TriggerOnServerChange(Guid Id, int Type)
          {
              EventHandler<ConnectionEventArgs> tmp = null;

              tmp = OnServerChange;
              if (tmp != null)
                  tmp(this, new ConnectionEventArgs() { ConnectionType = Type, Id = Id });
          }

          private void TriggerOnResponceReceived(Guid Id, String Code, List<object> Data)
          {
              EventHandler<Responce> tmp = null;

              tmp = OnResponceReceived;

              if (tmp != null)
                  tmp(this, new Responce() { SockId = Id, Code = Code, Data = Data });
          }

        
        #endregion

    }

   
}
