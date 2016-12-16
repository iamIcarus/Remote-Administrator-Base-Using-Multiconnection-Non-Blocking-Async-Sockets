using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Interaction logic for ServerBuilder.xaml
    /// </summary>
    public partial class ServerBuilder : Window
    {
        public ServerBuilder()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            String UserSettings = string.Format("[STOP]|{0}|{1}|", txtHost.Text, txtPort.Text);

            Microsoft.Win32.OpenFileDialog dlgOpen = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlgOpen.DefaultExt = ".exe";
            dlgOpen.Filter = "Executable|*.exe";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlgOpen.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {

                byte[] UserSettingsBytes = Encoding.ASCII.GetBytes(UserSettings);

                // Open document 
                string filename = dlgOpen.FileName;
                byte[] data = File.ReadAllBytes(filename);

                byte[] dataFinal = new byte[data.Length + UserSettingsBytes.Length];

                Buffer.BlockCopy(data, 0, dataFinal, 0, data.Length);

                Buffer.BlockCopy(UserSettingsBytes, 0, dataFinal, dataFinal.Length - UserSettingsBytes.Length, UserSettingsBytes.Length);



                 if (MessageBox.Show("Configurations generated successfully , save new configured server file?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                 {
                     Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog();
                     dlgSave.FileName = "Server"; // Default file name
                     dlgSave.DefaultExt = ".exe"; // Default file extension
                     dlgSave.Filter = "Executable|*.exe"; // Filter files by extension

                     // Show save file dialog box
                     result = dlgSave.ShowDialog();

                     // Process save file dialog box results
                     if (result == true)
                     {
                         // Save document
                         filename = dlgSave.FileName;
                         File.WriteAllBytes(filename, dataFinal);
                         this.DialogResult = true;
                     }
                 
                 }



            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
