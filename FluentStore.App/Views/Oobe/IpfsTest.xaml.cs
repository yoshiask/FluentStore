using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using FluentStore.Services;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Text;
using OwlCore.Kubo;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IpfsTest : WizardPageBase
    {
        private readonly IIpfsService _ipfsService;
        private CancellationTokenSource _cts;
        
        public IpfsTest(IIpfsService ipfsService)
        {
            _ipfsService = ipfsService;

            CanAdvance = false;
            Loaded += IpfsTest_Loaded;
            Unloaded += IpfsTest_Unloaded;

            this.InitializeComponent();
        }

        private void IpfsTest_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void IpfsTest_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    _cts = new();

                    await _ipfsService.BootstrapAsync(Ioc.Default, _cts.Token);
                    await _ipfsService.TestAsync(_cts.Token);

                    // Config is valid, save the settings
                    if (Ioc.Default.GetService<ISettingsService>() is Helpers.Settings settings)
                        await settings.SaveAsync();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CanAdvance = true;

                        CenterPanel.Children.Clear();
                        CenterPanel.Children.Add(new TextBlock
                        {
                            HorizontalTextAlignment = TextAlignment.Center,
                            Inlines =
                        {
                            new Run
                            {
                                Text = "Successfully connected to IPFS!",
                                FontSize = 20,
                                FontWeight = FontWeights.SemiBold
                            },
                            new LineBreak(),
                            new LineBreak(),
                            new Run
                            {
                                Text = "Please continue to choose your plugins."
                            },
                        }
                        });
                    });
                }
                catch (Exception ex)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        CenterPanel.Children.Clear();
                        CenterPanel.Children.Add(new TextBlock
                        {
                            HorizontalTextAlignment = TextAlignment.Center,
                            Inlines =
                            {
                                new Run
                                {
                                    Text = "Failed to connect to IPFS",
                                    FontSize = 20,
                                    FontWeight = FontWeights.SemiBold
                                },
                                new LineBreak(),
                                new LineBreak(),
                                new Run
                                {
                                    Text = ex.Message
                                },
                            }
                        });
                    });
                }
            });
        }
    }
}
