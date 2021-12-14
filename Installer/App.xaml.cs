using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow InstallerWindow;
        public static DirectoryInfo InstallerDir;

        public static Version Version { get; } = new Version(0, 2, 1, 0);
        public static string VersionString => Version.ToString();

        public App()
        {
            DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            InstallerWindow.ShowErrorMessage(e.Exception.ToString());
        }
    }
}
