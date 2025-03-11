using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Windows.Management.Deployment;

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

        private void UpdateProgress(uint progress)
        {
            Dispatcher.Invoke(delegate
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = progress;
                QueueOutputBoxWriteLine($"Installing {progress:#0}%");
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
                throw new Exception(installResult.ErrorText);

            void OnProgress(DeploymentProgress e) => UpdateProgress(e.percentage);
        }

        private void OutputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBoxBase;
            box?.ScrollToEnd();
        }
    }
}
