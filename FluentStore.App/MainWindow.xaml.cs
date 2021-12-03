using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private IntPtr m_hwnd;

        public MainWindow()
        {
            this.InitializeComponent();

            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            LoadIcon(@"Assets\AppIcon.ico");

            WeakReferenceMessenger.Default.Register<ViewModels.Messages.SetPageHeaderMessage>(this, (r, m) =>
            {
                Title = App.AppName;
                if (MainContent.IsCompact)
                    Title += " - " + m.Value;
            });
        }

        private void LoadIcon(string iconName)
        {
            IntPtr hIcon = PInvoke.User32.LoadImage(IntPtr.Zero, iconName,
                PInvoke.User32.ImageType.IMAGE_ICON, 16, 16, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);

            var hresult = PInvoke.Kernel32.GetLastError();
            if (hresult != PInvoke.Win32ErrorCode.ERROR_SUCCESS)
                throw new PInvoke.Win32Exception(hresult);

            PInvoke.User32.SendMessage(m_hwnd, PInvoke.User32.WindowMessage.WM_SETICON, IntPtr.Zero, hIcon);

            hresult = PInvoke.Kernel32.GetLastError();
            if (hresult != PInvoke.Win32ErrorCode.ERROR_SUCCESS)
                throw new PInvoke.Win32Exception(hresult);
        }
    }
}
