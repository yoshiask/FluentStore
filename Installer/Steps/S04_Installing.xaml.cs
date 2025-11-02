using Installer.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Windows.Management.Deployment;
using Windows.Web.Http;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S03_Installing.xaml
    /// </summary>
    public partial class S04_Installing : Page
    {
        private readonly CancellationTokenSource cts = new();

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

            await Task.Run(async () => await InstallAsync(cts.Token));
        }

        private async Task InstallAsync(CancellationToken token = default)
        {
            X509Store? store = null;
            X509Certificate2? cert = null;

            try
            {
                token.ThrowIfCancellationRequested();

                // Install certificate
                QueueOutputBoxWriteLine("Preparing certificate...");
                cert = new(DefaultResources.Cert);
                store = new(StoreName.TrustedPeople, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
                QueueOutputBoxWriteLine($"Certificate added with subject '{cert.Subject}'");

                token.ThrowIfCancellationRequested();

                // Install .NET runtime
                await InstallDotNetRuntime();

                token.ThrowIfCancellationRequested();

                // Install package
                QueueOutputBoxWriteLine("Installing package...");
                var appInstallerPath = "https://ipfs.askharoun.com/FluentStore/BetaInstaller/FluentStoreBeta.appinstaller";
                await InstallWithPackageManager(appInstallerPath, token);

                QueueOutputBoxWriteLine($"Fluent Store Beta was successfully installed.");

                // Install succeeded
                Dispatcher.Invoke(delegate
                {
                    App.InstallerWindow.SetNextButtonEnabled(true);
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
            finally
            {
                // Even if the install failed, we want to clean up after ourselves
                if (store is not null && cert is not null)
                {
                    store?.Remove(cert);
                    QueueOutputBoxWriteLine("Removed certificate");
                }
            }
        }

        private void UpdateProgress(uint progress, string verb = "Installing")
        {
            Dispatcher.Invoke(delegate
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = progress;
                QueueOutputBoxWriteLine($"{verb} {progress:#0}%");
            });
        }

        private void QueueOutputBoxWriteLine(string line)
        {
            Dispatcher.Invoke(delegate
            {
                OutputBox.Text += line + "\r\n";
            });
        }

        private void ShowErrorMessage(Exception ex)
        {
            Dispatcher.Invoke(delegate
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                QueueOutputBoxWriteLine(ex.ToString());

                string message = ex.Data.Contains("RestrictedDescription")
                    ? ex.Data["RestrictedDescription"].ToString().Trim()
                    : ex.Message;
                App.InstallerWindow.ShowErrorMessage(message);
            });
        }

        public void Cancel() => cts.Cancel();

        private async Task InstallWithPackageManager(string installerPath, CancellationToken token = default)
        {
            PackageManager pm = new();

            var defaultVolume = pm.GetDefaultPackageVolume();

            var operation = pm.AddPackageByAppInstallerFileAsync(new Uri(installerPath), default, defaultVolume);
            var task = operation.AsTask(token, new Progress<DeploymentProgress>(OnProgress));

            var installResult = await task;
            if (!installResult.IsRegistered)
                throw installResult.ExtendedErrorCode;

            void OnProgress(DeploymentProgress e) => UpdateProgress(e.percentage);
        }

        private async Task InstallDotNetRuntime(CancellationToken token = default)
        {
            // Check for installed runtime (doesn't need to be foolproof; just avoid downloading if possible)
            QueueOutputBoxWriteLine("Checking for .NET runtime...");

            try
            {
                var dotnetProcess = Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "dotnet",
                    Arguments = "--list-runtimes"
                });

                Regex rxRuntime = new(@"^(?<id>[\w.]+)\s+((?<major>\d+)\.\S+)");

                while (true)
                {
                    // Ensure dotnet process closes when cancelled
                    if (token.IsCancellationRequested)
                    {
                        dotnetProcess.Kill();
                        token.ThrowIfCancellationRequested();
                        return;
                    }

                    var line = await dotnetProcess.StandardOutput.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        break;

                    var match = rxRuntime.Match(line);
                    if (!match.Success)
                        continue;

                    var id = match.Groups["id"].Value;
                    var ver = match.Groups["ver"].Value;
                    var major = uint.Parse(match.Groups["major"].Value);

                    if (major >= App.RequiredDotNetRuntimeVersion && id.Equals(App.RequiredDotNetRuntimeId, StringComparison.OrdinalIgnoreCase))
                    {
                        QueueOutputBoxWriteLine($"Found compatible .NET runtime: {id} {ver}");
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // Swallow exceptions when checking for .NET, it's probably not installed
            }

            // Download .NET runtime
            QueueOutputBoxWriteLine($"No compatible runtime found. Downloading from {App.DotNetInstallerUrl}...");

            var lastProgress = uint.MaxValue;
            HttpClient http = new();
            using var installerHttpResponse = await http.GetAsync(new Uri(App.DotNetInstallerUrl))
                .AsTask(token, new Progress<HttpProgress>(OnDownloadProgress));

            installerHttpResponse.EnsureSuccessStatusCode();

            var installerPath = Path.GetTempFileName();
            using (var installerDstStream = new FileStream(installerPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await installerHttpResponse.Content
                    .WriteToStreamAsync(installerDstStream.AsOutputStream())
                    .AsTask(token);
            }

            // Run installer
            QueueOutputBoxWriteLine("Installing .NET runtime...");
            var installProcess = Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = installerPath,
                Arguments = "/install /quiet /norestart"
            });
            installProcess.WaitForExit();

            // Clean up installer
            File.Delete(installerPath);

            switch (installProcess.ExitCode)
            {
                case 0:
                    QueueOutputBoxWriteLine(".NET runtime was successfully installed.");
                    return;

                case 3010:
                    QueueOutputBoxWriteLine(".NET runtime was installed but may require a system restart.");
                    return;

                case 1618:
                    QueueOutputBoxWriteLine("Failed to install .NET runtime. Ensure no other installations are in progress.");
                    return;

                default:
                    if (MsiExitCodes.Messages.TryGetValue(installProcess.ExitCode, out var message))
                        QueueOutputBoxWriteLine(message);
                    throw new Exception($"Failed to install .NET runtime. Installer exited with code {installProcess.ExitCode}.");
            }

            void OnDownloadProgress(HttpProgress progress)
            {
                if (progress.TotalBytesToReceive is null)
                    return;

                var fraction = (float)progress.BytesReceived / progress.TotalBytesToReceive;
                var percent = (uint)fraction * 100;

                if (lastProgress != percent)
                {
                    lastProgress = percent;
                    UpdateProgress(percent, "Downloading .NET runtime");
                }
            }
        }

        private void OutputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBoxBase;
            box?.ScrollToEnd();
        }
    }
}
