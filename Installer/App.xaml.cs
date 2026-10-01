using System;
using System.Runtime.InteropServices;
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

        public static Version Version { get; } = new Version(0, 4, 2, 0);
        public static string VersionString => Version.ToString();

        public static string RequiredDotNetRuntimeId = "Microsoft.WindowsDesktop.App";
        public static uint RequiredDotNetRuntimeVersion = 10;

        public App()
        {
            DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            InstallerWindow.ShowErrorMessage(e.Exception.ToString());
        }

        public static Uri GetDotNetInstallerUri()
        {
            return new Uri(RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe",
                Architecture.X86 => "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x86.exe",
                Architecture.Arm64 => "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-arm64.exe",
                _ => throw new NotSupportedException($"Unsupported architecture: {RuntimeInformation.OSArchitecture}")
            });
        }
    }
}
