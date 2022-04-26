using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class General : UserControl
    {
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        public General()
        {
            this.InitializeComponent();
        }

        private void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadCache cache = new(createIfDoesNotExist: false);
            cache.Clear();
        }

        private async void SendFeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("https://github.com/yoshiask/FluentStore/issues/new/choose");
        }

        private async void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("http://josh.askharoun.com/donate");
        }

        private void CrashButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            throw new System.Exception("An unhandled exception was thrown. The app should have crashed and pushed a notification " +
                "that allows the user to view and report the error.");
#endif
        }
    }
}
