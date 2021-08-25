using System.Windows;
using System.Windows.Threading;

namespace Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ((MainWindow)MainWindow).ShowErrorMessage(e.Exception.ToString());
        }
    }
}
