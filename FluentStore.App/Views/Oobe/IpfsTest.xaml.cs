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
        
        public IpfsTest(IIpfsService ipfsService)
        {
            _ipfsService = ipfsService;

            CanAdvance = false;
            Loaded += IpfsTest_Loaded;

            this.InitializeComponent();
        }

        private async void IpfsTest_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _ipfsService.BootstrapAsync(Ioc.Default);

                var cid = Ipfs.Cid.Decode("QmZtmD2qt6fJot32nabSP3CUjicnypEBz7bHVDhPQt9aAy");
                var client = _ipfsService.Client;

                IpfsFile testFile = new(cid, client);
                using var stream = await testFile.OpenStreamAsync();

                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer);
                var testString = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (testString != "version 1 of my text\n")
                    throw new Exception("Connected to IPFS, but test file was invalid. Please check your IPFS configuration.");

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
            }
            catch (Exception ex)
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
            }
        }
    }
}
