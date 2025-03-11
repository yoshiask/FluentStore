using System.Windows;
using System.Windows.Controls;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S04_Completed.xaml
    /// </summary>
    public partial class S05_Completed : Page
    {
        public S05_Completed()
        {
            InitializeComponent();
        }

        private void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText("Launch");
            App.InstallerWindow.SetNextButtonEnabled();
            App.InstallerWindow.SetBackButtonEnabled(false);
            App.InstallerWindow.SetCancelButtonEnabled(false);
        }
    }
}
