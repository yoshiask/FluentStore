using System.Windows;
using System.Windows.Controls;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S00_Welcome.xaml
    /// </summary>
    public partial class S00_Welcome : Page
    {
        public S00_Welcome()
        {
            InitializeComponent();
        }

        private void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText();
            App.InstallerWindow.SetNextButtonEnabled();
            App.InstallerWindow.SetBackButtonEnabled(false);
            App.InstallerWindow.SetCancelButtonEnabled();
        }
    }
}
