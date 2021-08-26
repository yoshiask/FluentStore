using Octokit;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using TaskDialogInterop;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S01_Updates.xaml
    /// </summary>
    public partial class S01_Updates : System.Windows.Controls.Page
    {
        public S01_Updates()
        {
            InitializeComponent();
        }

        private async void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText();
            App.InstallerWindow.SetNextButtonEnabled();
            App.InstallerWindow.SetBackButtonEnabled();
            App.InstallerWindow.SetCancelButtonEnabled();

            try
            {
                var github = new GitHubClient(new ProductHeaderValue("FluentStoreInstaller"));
                var latest = await github.Repository.Release.GetLatest("yoshiask", "FluentStore");
                Regex rx = new(@"(?<major>\d+)\.(?<minor>\d+)(?:\.(?<build>\d+)(?:\.(?<revision>\d+))?)?");
                Match m = rx.Match(latest.TagName);
                if (m == null || !m.Success)
                    return;
                int major = int.Parse(m.Groups["major"].Value);
                int minor = int.Parse(m.Groups["minor"].Value);
                bool hasBuild = int.TryParse(m.Groups["build"].Value, out int build);
                bool hasRevision = int.TryParse(m.Groups["revision"].Value, out int revision);
                Version latestVersion = new(
                    major,
                    minor,
                    hasBuild ? build : 0,
                    hasRevision ? revision : 0);

                // Compare latest version to current version
                if (latestVersion > App.Version)
                {
                    // Prompt user to exit and download latest
                    TaskDialogOptions config = new();

                    config.Owner = App.InstallerWindow;
                    config.Title = "Fluent Store Installer";
                    config.MainInstruction = "A newer version of Fluent Store is available";
                    config.Content = "Would you like to exit the setup and open the download page?";
                    config.CommonButtons = TaskDialogCommonButtons.YesNoCancel;
                    config.MainIcon = VistaTaskDialogIcon.Information;

                    TaskDialogResult res = TaskDialog.Show(config);
                    switch (res.Result)
                    {
                        case TaskDialogSimpleResult.Yes:
                            Process.Start(latest.HtmlUrl);
                            App.InstallerWindow.Cancel(false);
                            return;

                        case TaskDialogSimpleResult.Cancel:
                            App.InstallerWindow.Cancel();
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Failed to check for updates
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }

            App.InstallerWindow.NextStep();
        }
    }
}
