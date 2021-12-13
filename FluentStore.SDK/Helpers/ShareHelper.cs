using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;

namespace FluentStore.SDK.Helpers
{
    public static class ShareHelper
    {
        static readonly Guid _dtm_iid = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

        static IDataTransferManagerInterop DataTransferManagerInterop
        {
            get
            {
                return DataTransferManager.As<IDataTransferManagerInterop>();
            }
        }

        public static DataTransferManager GetDataTransferManager(Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return GetDataTransferManager(hwnd);
        }

        public static DataTransferManager GetDataTransferManager(IntPtr appWindow)
        {
            IntPtr result = DataTransferManagerInterop.GetForWindow(appWindow, _dtm_iid);
            DataTransferManager dataTransferManager = WinRT.MarshalInterface<DataTransferManager>.FromAbi(result);
            return dataTransferManager;
        }

        public static void ShowShareUIForWindow(IntPtr hwnd)
        {
            DataTransferManagerInterop.ShowShareUIForWindow(hwnd);
        }
        public static void ShowShareUIForWindow(Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            ShowShareUIForWindow(hwnd);
        }

        [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDataTransferManagerInterop
        {
            IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }
    }
}
