using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FluentStore.Services;
using FluentStore.Views.Oobe;
using Microsoft.Extensions.DependencyInjection;
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

        NextCommand = new RelayCommand(NextPage);
        PreviousCommand = new RelayCommand(PreviousPage);

        Pages = new()
        {
            new("Welcome to Fluent Store", "", new ImageIcon { Source = new BitmapImage(new("ms-appx:///Assets/StoreLogo.png")) })
            {
                PageType = typeof(Oobe.Welcome)
            },
            new("Configure IPFS", "", new ImageIcon { Source = new BitmapImage(new("https://raw.githubusercontent.com/ipfs-inactive/logo/master/raster-generated/ipfs-logo-128-ice.png")) })
            {
                PageType = typeof(Oobe.IpfsClient)
            },
            new("Install Plugins", "", new SymbolIcon(Symbol.AllApps) { Margin = new(8) })
            {
                PageType = typeof(Oobe.Plugins)
            },
        };

        UpdatePage();
    }

    public IRelayCommand NextCommand { get; }
    public IRelayCommand PreviousCommand { get; }

    public List<PageInfo> Pages { get; }

    public PageInfo SelectedPage
    {
        get => Pages[SelectedPageIndex];
        set => Pages[SelectedPageIndex] = value;
    }

    public int SelectedPageIndex { get; set; }

    public event EventHandler SetupCompleted;

    private void NextPage()
    {
        SelectedPageIndex++;
        UpdatePage();
    }

    private void PreviousPage()
    {
        SelectedPageIndex--;
        UpdatePage();
    }

    private void UpdatePage()
    {
        NextButton.IsEnabled = SelectedPageIndex < Pages.Count - 1;
        PreviousButton.IsEnabled = SelectedPageIndex > 0;

        TitleBlock.Text = SelectedPage.Title;
        IconPresenter.Content = SelectedPage.Icon;

        var page = (WizardPageBase)ActivatorUtilities.CreateInstance(Ioc.Default, SelectedPage.PageType);
        OobePresenter.Content = page;
    }
}
