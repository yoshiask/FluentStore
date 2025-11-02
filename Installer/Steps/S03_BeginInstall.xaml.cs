using System.Windows;
using System.Windows.Controls;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S02_BeginInstall.xaml
    /// </summary>
    public partial class S03_BeginInstall : Page
    {
        public S03_BeginInstall()
        {
            InitializeComponent();
        }

        private void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText("Install");
            App.InstallerWindow.SetNextButtonEnabled();
            App.InstallerWindow.SetBackButtonEnabled();
            App.InstallerWindow.SetCancelButtonEnabled();
        }
    }
}
