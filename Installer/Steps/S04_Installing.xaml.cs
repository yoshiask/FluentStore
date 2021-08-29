using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static Installer.Utils.InstallScriptLocalization;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S03_Installing.xaml
    /// </summary>
    public partial class S04_Installing : Page
    {
        private DirectoryInfo TempFolder;
        private FileInfo ZipFile;
        private Process psProc;
        private const string regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock";
        private bool? AllowAllTrustedApps = null;
        private bool? AllowDevelopmentWithoutDevLicense = null;

        public S04_Installing()
        {
            InitializeComponent();
        }

        private async void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText();
            App.InstallerWindow.SetNextButtonEnabled(false);
            App.InstallerWindow.SetBackButtonEnabled(false);
            App.InstallerWindow.SetCancelButtonEnabled();

            // Extract ZIP to temp folder
            OutputBox.Text += "Extracting installer...\r\n";
            TempFolder = Directory.CreateDirectory(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FluentStoreInstaller"));
            ZipFile = new(Path.Combine(TempFolder.FullName, "FluentStore_Beta.zip"));
            App.InstallerDir = new(Path.Combine(TempFolder.FullName, "FluentStore_Beta"));

            Debug.WriteLine("Extracting to " + App.InstallerDir.FullName);
            File.WriteAllBytes(ZipFile.FullName, DefaultResources.FluentStore_Beta);
            if (App.InstallerDir.Exists)
                App.InstallerDir.Delete(true);
            System.IO.Compression.ZipFile.ExtractToDirectory(ZipFile.FullName, App.InstallerDir.FullName);

            // Run Install.ps1
            ProcessStartInfo startInfo = new()
            {
                FileName = "powershell",
                WorkingDirectory = App.InstallerDir.FullName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            psProc = Process.Start(startInfo);
            if (psProc == null)
            {
                // Script failed to start
                ProgressBar.Visibility = Visibility.Collapsed;
                App.InstallerWindow.ShowErrorMessage("Failed to start install script.");
                return;
            }

            psProc.EnableRaisingEvents = true;
            psProc.ErrorDataReceived += PsProc_ErrorDataReceived;
            psProc.Exited += (s, evt) =>
            {
                psProc?.Dispose();
                Debug.WriteLine("Console closed");
            };
            if (!psProc.Start())
            {
                // Script failed to start
                ProgressBar.Visibility = Visibility.Collapsed;
                App.InstallerWindow.ShowErrorMessage("Failed to start install script.");
                return;
            }
            OutputBox.Text += "Beginning install...\r\n";
            psProc.BeginErrorReadLine();
            await psProc.StandardOutput.ReadLineAsync();
            await Task.Delay(100);

            // Set execution policy for process to allow script to run
            await psProc.StandardInput.WriteLineAsync("Get-ExecutionPolicy");
            await Task.Delay(100);
            await psProc.StandardInput.WriteLineAsync("Set-ExecutionPolicy -Scope Process Unrestricted");
            await Task.Delay(100);

            // Set developer license so the install script doesn't open settings
            using RegistryKey? hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey? regKey = hklm.OpenSubKey(regPath, true);
            if (regKey == null)
                regKey = Registry.LocalMachine.CreateSubKey(regPath, true);

            int? regVal = regKey.GetValue("AllowAllTrustedApps") as int?;
            if (regVal.HasValue)
                AllowAllTrustedApps = regVal.Value > 0 ? true : false;
            regVal = regKey.GetValue("AllowDevelopmentWithoutDevLicense") as int?;
            if (regVal.HasValue)
                AllowDevelopmentWithoutDevLicense = regVal.Value > 0 ? true : false;

            regKey.SetValue("AllowAllTrustedApps", true, RegistryValueKind.DWord);
            regKey.SetValue("AllowDevelopmentWithoutDevLicense", true, RegistryValueKind.DWord);
            regKey.Close();

            // Start install script
            await psProc.StandardInput.WriteLineAsync(".\\Install.ps1");

            bool isError = false;
            string hresultMsg = string.Empty;
            while (!psProc.HasExited)
            {
                string? line = await psProc.StandardOutput.ReadLineAsync();
                if (line == null)
                    continue;

                Debug.WriteLine("\tOut> " + line);
                OutputBox.Text += line.Replace(App.InstallerDir.FullName, "$(InstallerPath)") + "\r\n";
                if (line.Contains("HRESULT"))
                {
                    // Error occurred
                    isError = true;
                    int idxHR = line.IndexOf("HRESULT: ");
                    hresultMsg = line.Substring(idxHR);
                }
                else if (isError)
                {
                    // Read rest of error
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // End of error message
                        isError = false;
                        ProgressBar.Visibility = Visibility.Collapsed;
                        App.InstallerWindow.ShowErrorMessage(hresultMsg);
                        return;
                    }
                    else
                    {
                        hresultMsg += line;
                    }
                }

                else if (line.StartsWith("Press enter to continue"))
                {
                    await psProc.StandardInput.WriteLineAsync();
                }
                else if (line == GetLocalizedString(KEY_Success))
                {
                    // Install succeeded
                    App.InstallerWindow.NextStep();
                }
                else if (line.StartsWith(GetLocalizedString(KEY_Error)))
                {
                    // Install failed
                    ProgressBar.Visibility = Visibility.Collapsed;
                    App.InstallerWindow.ShowErrorMessage(line);
                    return;
                }
            }

            if (!isError)
            {
                Debug.WriteLine("PowerShell script exited");
                Debug.WriteLine("Removing temporary files");
                TempFolder.Delete(true);
            }
        }

        public void Cancel()
        {
            try
            {
                if (psProc != null && !psProc.HasExited)
                {
                    Debug.WriteLine("Killing PowerShell script");
                    psProc?.Kill();
                    psProc?.Close();
                }

                using RegistryKey? hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey? regKey = hklm.OpenSubKey(regPath, true);
                if (regKey != null)
                {
                    if (AllowAllTrustedApps.HasValue)
                        regKey.SetValue("AllowAllTrustedApps", AllowAllTrustedApps.Value, RegistryValueKind.DWord);
                    else
                        regKey.DeleteValue("AllowAllTrustedApps");

                    if (AllowDevelopmentWithoutDevLicense.HasValue)
                        regKey.SetValue("AllowDevelopmentWithoutDevLicense", AllowDevelopmentWithoutDevLicense.Value, RegistryValueKind.DWord);
                    else
                        regKey.DeleteValue("AllowDevelopmentWithoutDevLicense");

                    regKey.Close();
                }

                Debug.WriteLine("Removing temporary files");
                TempFolder?.Delete(true);
            }
            catch (Exception)
            {
                Debug.WriteLine("Error while cancelling");
            }
        }

        private void PsProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine("\tERR> " + e.Data);
        }

        private void OutputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBoxBase;
            box?.ScrollToEnd();
        }
    }
}
