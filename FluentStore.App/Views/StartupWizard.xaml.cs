using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FluentStore.Services;
using FluentStore.Views.Oobe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace FluentStore.Views;

public sealed partial class StartupWizard : ViewBase
{
    private WizardPageBase _page;

    public StartupWizard()
    {
        this.InitializeComponent();

        NextCommand = new RelayCommand(NextPage);
        PreviousCommand = new RelayCommand(PreviousPage);

        Pages = new()
        {
            new("Welcome to Fluent Store", "", new ImageIcon { Source = new BitmapImage(new("ms-appx:///Assets/StoreLogo.png")) })
            {
                PageType = typeof(Welcome)
            },
            new("Configure IPFS", "", new ImageIcon { Source = new BitmapImage(new("https://raw.githubusercontent.com/ipfs-inactive/logo/master/raster-generated/ipfs-logo-128-ice.png")) })
            {
                PageType = typeof(IpfsClient)
            },
            new("Configure IPFS", "", new ImageIcon { Source = new BitmapImage(new("https://raw.githubusercontent.com/ipfs-inactive/logo/master/raster-generated/ipfs-logo-128-ice.png")) })
            {
                PageType = typeof(IpfsTest)
            },
            new("Install Plugins", "", new SymbolIcon(Symbol.AllApps) { Margin = new(8) })
            {
                PageType = typeof(Plugins)
            },
        };

        UpdatePage();
    }

    public IRelayCommand NextCommand { get; }
    public IRelayCommand PreviousCommand { get; }

    public List<PageInfo> Pages { get; }

    public PageInfo SelectedPageInfo
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
        if (SelectedPageIndex >= Pages.Count)
        {
            SetupCompleted?.Invoke(this, EventArgs.Empty);
            return;
        }

        NextButton.IsEnabled = false;
        PreviousButton.IsEnabled = SelectedPageIndex > 0;

        TitleBlock.Text = SelectedPageInfo.Title;
        IconPresenter.Content = SelectedPageInfo.Icon;

        _page = (WizardPageBase)ActivatorUtilities.CreateInstance(Ioc.Default, SelectedPageInfo.PageType);
        OobePresenter.Content = _page;
        NextButton.IsEnabled = _page.CanAdvance;

        _page.RegisterPropertyChangedCallback(WizardPageBase.CanAdvanceProperty, OnCanAdvanceChanged);
    }

    private void OnCanAdvanceChanged(DependencyObject sender, DependencyProperty dp)
    {
        NextButton.IsEnabled = _page.CanAdvance;
    }
}
