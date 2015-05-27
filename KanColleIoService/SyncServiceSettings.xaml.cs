using System;
using System.Collections.Generic;
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

namespace KanColleIoService
{
    /// <summary>
    /// Логика взаимодействия для SyncServiceSettings.xaml
    /// </summary>
    public partial class SyncServiceSettings : UserControl
    {
        public SyncServiceSettings()
        {
            InitializeComponent();
        }

        private async void LogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SyncService.Current.LogIn(Username.Text, Password.Password);

                Username.IsEnabled = false;
                Password.IsEnabled = false;

                Button logInButton = sender as Button;
                logInButton.IsEnabled = false;
                logInButton.Content = "Success";
            }
            catch (APIException ex)
            {
                SyncService.Current.ReportException(ex);
            }
        }
    }
}
