using System.Text;
using System.Windows.Controls;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S01_License.xaml
    /// </summary>
    public partial class S02_License : Page
    {
        public S02_License()
        {
            InitializeComponent();
            LicenseBox.Text = Encoding.ASCII.GetString(DefaultResources.LICENSE);
        }

        private void Step_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText();
            App.InstallerWindow.SetNextButtonEnabled(AcceptBox.IsChecked ?? false);
            App.InstallerWindow.SetBackButtonEnabled();
            App.InstallerWindow.SetCancelButtonEnabled();
        }

        private void AcceptBox_CheckChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is not CheckBox acceptBox)
                return;
            App.InstallerWindow.SetNextButtonEnabled(acceptBox.IsChecked ?? false);
        }
    }
}
