using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for AttackWindow.xaml
    /// </summary>
    public partial class AttackWindow : Window
    {
        public String Host { get { return txtHost.Text; } }
        public String Port { get { return txtPort.Text; } }
        public String Timeout { get { return txtTimeout.Text; } }

        bool isActive = false;

        private Helpers.ConnectionManager Connections;
        private int AttackIndex;

        public AttackWindow(Helpers.ConnectionManager Connections, int p, String Title)
        {
            InitializeComponent();

            this.Connections = Connections;
            this.AttackIndex = p;
            lblTitle.Content = Title;

            Closing += OnWindowClosing;
        }
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Connections.BroadcastStopAttack(AttackIndex);
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            IPAddress ip;
            bool IsValidIP = IPAddress.TryParse(txtHost.Text, out ip);

            if (!isActive)
            {
                if (!IsValidIP)
                {
                    MessageBox.Show("Host IP not Valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                isActive = true;
                btnStart.Content = "Stop";
                Connections.BroadcastAttack(AttackIndex,Host,Port,Timeout);
            }
            else
            {
                isActive = false;
                btnStart.Content = "Start";
                Connections.BroadcastStopAttack(AttackIndex);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
