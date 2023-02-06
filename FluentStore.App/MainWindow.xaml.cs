using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using HRESULT = Vanara.PInvoke.HRESULT;
using DwmApi = Vanara.PInvoke.DwmApi;
using WinUIEx;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx, IRecipient<SetPageHeaderMessage>
    {
        private IntPtr m_hwnd;

        public MainWindow()
        {
            this.InitializeComponent();

            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            
            var navService = CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default.GetService<Services.NavigationService>();
            if (navService != null)
            {
                navService.SetMainWindowHandle(m_hwnd);
                navService.AppFrame = WindowContent;
            }

            TaskBarIcon = Icon.FromFile(@"Assets\AppIcon.ico");

            WeakReferenceMessenger.Default.Register(this);

            var titleBar = AppWindow?.TitleBar;
            if (titleBar != null && AppWindowTitleBar.IsCustomizationSupported())
            {
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(10, 0, 0, 0);
                SetTitleBar(CustomTitleBar);
            }
            else
            {
                CustomTitleBar.Visibility = Visibility.Collapsed;
                UnloadObject(CustomTitleBar);

                // Enable theme-aware title bar
                unsafe
                {
                    BOOL enableImmersiveDarkMode = BOOL.TRUE;
                    DwmApi.DwmSetWindowAttribute(m_hwnd, (DwmApi.DWMWINDOWATTRIBUTE)20, (IntPtr)(&enableImmersiveDarkMode), sizeof(BOOL));
                }
            }
        }

        public IntPtr Handle => m_hwnd;

        enum BOOL
        {
            FALSE = 0,
            TRUE = 1
        };

        void IRecipient<SetPageHeaderMessage>.Receive(SetPageHeaderMessage m)
        {
            Title = App.AppName;
            if (WindowContent.TryGetContent(out var content) && content.IsCompact)
                Title += " - " + m.Value;
        }
    }
}
