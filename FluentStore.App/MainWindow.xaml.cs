using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using HRESULT = Vanara.PInvoke.HRESULT;
using DwmApi = Vanara.PInvoke.DwmApi;
using WinUIEx;

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
        private Stack<object> m_navStack = new();

        public MainWindow()
        {
            this.InitializeComponent();

            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            TaskBarIcon = Icon.FromFile(@"Assets\AppIcon.ico");

            WeakReferenceMessenger.Default.Register(this);

            var titleBar = AppWindow?.TitleBar;
            if (titleBar != null)
            {
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(10, 0, 0, 0);
                SetTitleBar(CustomTitleBar);
            }
        }

        public IntPtr Handle => m_hwnd;

        public void Navigate(Type type)
        {
            var page = type.GetConstructor(Type.EmptyTypes).Invoke(null) as UIElement;
            Navigate(page);
        }

        public void Navigate(UIElement newContent)
        {
            var oldContent = WindowContent.Content;
            m_navStack.Push(oldContent);
            if (TryGetAppContent(out var oldAppContent))
                oldAppContent.OnNavigatedFrom();

            WindowContent.Content = newContent;
            if (TryGetAppContent(out var newAppContent))
                newAppContent.OnNavigatedTo();
        }

        public bool NavigateBack()
        {
            if (m_navStack.TryPop(out var oldContent))
            {
                if (TryGetAppContent(out var newAppContent))
                    newAppContent.OnNavigatedFrom();

                WindowContent.Content = oldContent;
                if (TryGetAppContent(out var oldAppContent))
                    oldAppContent.OnNavigatedTo();
                return true;
            }
            return false;
        }

        public bool TryGetAppContent(out IAppContent content)
        {
            content = WindowContent.Content as IAppContent;
            return content != null;
        }

        public void ClearNavigationStack() => m_navStack.Clear();

        private unsafe HRESULT EnableThemeAwareTitleBar()
        {
            BOOL useDark = BOOL.TRUE;
            return DwmApi.DwmSetWindowAttribute(m_hwnd, (DwmApi.DWMWINDOWATTRIBUTE)20, new(&useDark), sizeof(BOOL));
        }

        enum BOOL
        {
            FALSE = 0,
            TRUE = 1
        };

        void IRecipient<SetPageHeaderMessage>.Receive(SetPageHeaderMessage m)
        {
            Title = App.AppName;
            if (TryGetAppContent(out var content) && content.IsCompact)
                Title += " - " + m.Value;
        }
    }
}
