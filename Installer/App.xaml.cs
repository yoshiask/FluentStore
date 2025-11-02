using System;
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

        public static Version Version { get; } = new Version(0, 4, 1, 0);
        public static string VersionString => Version.ToString();

        public static string RequiredDotNetRuntimeId = "Microsoft.WindowsDesktop.App";
        public static uint RequiredDotNetRuntimeVersion = 8;
        public static string DotNetInstallerUrl { get; } =
#if X64
            "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe";
#elif X86
            "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x86.exe";
#elif ARM64
            "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-arm64.exe";
#endif

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
