using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using TaskDialogInterop;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirectoryInfo TempFolder;
        private FileInfo ZipFile;
        private DirectoryInfo InstallerDir;
        private Process psProc;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Extract ZIP to temp folder
            ProgressBlock.Text = "Extracting installer...";
            TempFolder = Directory.CreateDirectory(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FluentStoreInstaller"));
            ZipFile = new(Path.Combine(TempFolder.FullName, "FluentStore_Beta.zip"));
            InstallerDir = new(Path.Combine(TempFolder.FullName, "FluentStore_Beta"));

            Debug.WriteLine("Extracting to " + InstallerDir.FullName);
            File.WriteAllBytes(ZipFile.FullName, DefaultResources.FluentStore_Beta);
            if (InstallerDir.Exists)
                InstallerDir.Delete(true);
            System.IO.Compression.ZipFile.ExtractToDirectory(ZipFile.FullName, InstallerDir.FullName);

            // Run Install.ps1
            ProgressBlock.Text = "Starting install script...";
            ProcessStartInfo startInfo = new()
            {
                FileName = "powershell",
                WorkingDirectory = InstallerDir.FullName,
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
                ProgressBlock.Text = "Failed to start install script.";
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
                ProgressBlock.Text = "Failed to start install script.";
                return;
            }
            ProgressBlock.Text = "Installing...";
            psProc.BeginErrorReadLine();
            await psProc.StandardOutput.ReadLineAsync();
            await Task.Delay(100);

            // Set execution policy for process to allow script to run
            await psProc.StandardInput.WriteLineAsync("Get-ExecutionPolicy");
            await Task.Delay(100);
            await psProc.StandardInput.WriteLineAsync("Set-ExecutionPolicy -Scope Process Unrestricted");
            await Task.Delay(100);
            await psProc.StandardInput.WriteLineAsync(".\\Install.ps1");

            bool isError = false;
            string hresultMsg = string.Empty;
            while (!psProc.HasExited)
            {
                string? line = await psProc.StandardOutput.ReadLineAsync();
                if (line == null)
                    continue;

                Debug.WriteLine("\tOut> " + line);
                OutputBox.Text += line.Replace(InstallerDir.FullName, "$(InstallerPath)") + "\r\n";
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
                        ProgressBlock.Text = "Could not install the app.";
                        ProgressBar.Visibility = Visibility.Collapsed;
                        ShowErrorMessage(hresultMsg);
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
                else if (line.StartsWith("Success"))
                {
                    // Install succeeded
                    ProgressBlock.Text = "Fluent Store just got installed.";
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
                else if (line.StartsWith("Error"))
                {
                    // Install failed
                    ProgressBlock.Text = "Could not install the app.";
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ShowErrorMessage(line);
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

                Debug.WriteLine("Removing temporary files");
                TempFolder?.Delete(true);
            }
            catch (Exception)
            {
                Debug.WriteLine("Error while cancelling");
            }
            App.Current.Shutdown();
        }

        public void ShowErrorMessage(string msg)
        {
            TaskDialogOptions config = new();

            config.Owner = this;
            config.Title = "Fluent Store Installer";
            config.MainInstruction = "Install failed";
            config.Content = msg + "\r\n\r\n" +
                             "You may run the setup again at another time to complete the installation.";
            config.CommonButtons = TaskDialogCommonButtons.Close;
            config.MainIcon = VistaTaskDialogIcon.Error;

            TaskDialogResult res = TaskDialog.Show(config);
            if (res.Result == TaskDialogSimpleResult.Close)
                Cancel();
        }

        private void PsProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine("\tERR> " + e.Data);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Confirm cancel
            TaskDialogOptions config = new TaskDialogOptions();

            config.Owner = this;
            config.Title = "Fluent Store Installer";
            config.MainInstruction = "Are you sure you want to cancel?";
            config.Content = "Setup is not complete. If you exit now, the app will not be installed.\r\n\r\n" +
                             "You may run the setup again at another time to complete the installation.";
            config.CommonButtons = TaskDialogCommonButtons.YesNo;
            config.MainIcon = VistaTaskDialogIcon.Warning;

            TaskDialogResult res = TaskDialog.Show(config);
            if (res.Result == TaskDialogSimpleResult.Yes)
                Cancel();
        }

        private void OutputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var box = sender as TextBoxBase;
            box?.ScrollToEnd();
        }
    }
}
