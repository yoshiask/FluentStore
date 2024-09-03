using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FluentStore.Views;

public sealed partial class StartupWizard : ViewBase
{
    public StartupWizard()
    {
        this.InitializeComponent();

        Pages = new()
        {
            new("Welcome to Fluent Store", "Let's get you set up.", new ImageIcon { Source = new BitmapImage(new("ms-appx:///Assets/Square71x71Logo.png")) })
        };
        SelectedPage = Pages[0];
    }

    public List<PageInfo> Pages { get; }

    public PageInfo SelectedPage { get; set; }


    public event EventHandler SetupCompleted;
}
