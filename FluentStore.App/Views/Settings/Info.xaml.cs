﻿using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using OwlCore.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class Info : UserControl
    {
        private readonly NavigationServiceBase NavigationService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
        private readonly ICommonPathManager CommonPathManager = Ioc.Default.GetRequiredService<ICommonPathManager>();

        public Info()
        {
            this.InitializeComponent();
        }

        private void OpenLogDirButton_Click(object sender, RoutedEventArgs e)
        {

            // Add a trailing slash to ensure that Explorer opens the folder,
            // and not a file that might have the same name
            System.Diagnostics.Process.Start("explorer.exe", $"\"{CommonPathManager.GetDefaultLogDirectory()}{System.IO.Path.DirectorySeparatorChar}\"");
        }

        private void LogClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in CommonPathManager.GetDefaultLogDirectory().EnumerateFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
        }

        private void LogLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not Selector selector)
                return;

            if (selector.SelectedValue is not LogLevel newLogLevel)
                return;

            var currentLogLevel = Helpers.Settings.Default.LoggingLevel;
            if (currentLogLevel == newLogLevel)
                return;

            Helpers.Settings.Default.LoggingLevel = newLogLevel;

            var log = Ioc.Default.GetService<LoggerService>();
            log?.SetLogLevel(newLogLevel);
        }

        private async void SendFeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("https://github.com/yoshiask/FluentStore/issues/new/choose");
        }

        private async void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("http://josh.askharoun.com/donate");
        }
    }
}
