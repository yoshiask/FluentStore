using System.Diagnostics;
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://josh.askharoun.com/Fluent%20Store/help/installer-faq",
                UseShellExecute = true
            });
        }
    }
}
