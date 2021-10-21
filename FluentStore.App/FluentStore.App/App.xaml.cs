using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private NavigationService m_navService;
        private MainWindow m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
            => OnUnhandledException(e.Exception);

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
            => OnUnhandledException(e.Exception);

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
            => OnActivated(args.UWPLaunchActivatedEventArgs);

        ///<inheritdoc/>
        protected virtual void OnActivated(Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            m_navService = Ioc.Default.GetService<INavigationService>() as NavigationService;
            //ExtendIntoTitlebar();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (m_window == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                m_window = new MainWindow();

                // TODO: Is there a WinUI 3 equivalent to the system back button?

                // Add support for accelerator keys. 
                // Listen to the window directly so the app responds
                // to accelerator keys regardless of which element has focus.
                //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;

                // Add support for system back requests.
                //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;

                // Add support for mouse navigation buttons. 
                //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

                if (args.PreviousExecutionState == Windows.ApplicationModel.Activation.ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
            }
            m_navService.AppFrame = null;
            Tuple<Type, object> destination = new(typeof(Views.HomeView), null);

            // TODO: Is there a WinUI 3 equivalent?
            //if (args is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs launchArgs && launchArgs.PrelaunchActivated == false)
            //{
            //    if (rootFrame.Content == null)
            //    {
            //        // When the navigation stack isn't restored navigate to the first page,
            //        // configuring the new page by passing required information as a navigation
            //        // parameter
            //        destination = m_navService.ParseProtocol(launchArgs.Arguments);
            //    }
            //    // Ensure the current window is active
            //    Window.Current.Activate();
            //}

            if (args is Windows.ApplicationModel.Activation.ProtocolActivatedEventArgs ptclArgs)
            {
                destination = m_navService.ParseProtocol(ptclArgs.Uri);
            }
            else if (args is Windows.ApplicationModel.Activation.ToastNotificationActivatedEventArgs toastArgs)
            {
                destination = m_navService.ParseProtocol(toastArgs.Argument);
            }
            m_navService.Navigate(destination.Item1, destination.Item2);

            // Ensure the current window is active
            m_window.Activate();
        }

        private void OnUnhandledException(Exception ex)
        {
            /// Mostly yoinked from https://github.com/files-community/Files/blob/ace2f355ec87f4ca27975c25026636be8514f1e0/Files/App.xaml.cs#L432

            LoggerService Logger = Ioc.Default.GetService<LoggerService>();
            string formattedException = string.Empty;

            formattedException += "--------- UNHANDLED EXCEPTION ---------";
            if (ex != null)
            {
                formattedException += $"\n>>>> HRESULT: {ex.HResult}\n";
                if (ex.Message != null)
                {
                    formattedException += "\n--- MESSAGE ---";
                    formattedException += ex.Message;
                }
                if (ex.StackTrace != null)
                {
                    formattedException += "\n--- STACKTRACE ---";
                    formattedException += ex.StackTrace;
                }
                if (ex.Source != null)
                {
                    formattedException += "\n--- SOURCE ---";
                    formattedException += ex.Source;
                }
                if (ex.InnerException != null)
                {
                    formattedException += "\n--- INNER ---";
                    formattedException += ex.InnerException;
                }
            }
            else
            {
                formattedException += "\nException is null!\n";
            }

            formattedException += "---------------------------------------";

#if DEBUG
            System.Diagnostics.Debug.WriteLine(formattedException);

            System.Diagnostics.Debugger.Break(); // Please check "Output Window" for exception details (View -> Output Window) (CTRL + ALT + O)
#endif

            Logger?.UnhandledException(ex, ex.Message);

            // Encode error message and stack trace
            string message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ex.ToString()));

            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Oops!"
                            },
                            new AdaptiveText()
                            {
                                Text = "An error occurred that Fluent Store could not recover from."
                            }
                        },
                        //AppLogoOverride = new ToastGenericAppLogo()
                        //{
                        //    Source = "ms-appx:///Assets/error.png"
                        //}
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("View", $"crash?msg={message}")
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new Microsoft.Marketplace.Storefront.Contracts.StorefrontApi());
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPasswordVaultService, PasswordVaultService>();
            services.AddSingleton(new FluentStoreAPI.FluentStoreAPI());
            services.AddSingleton(new WinGetRun.WinGetApi());
            services.AddSingleton(typeof(UserService));
            services.AddSingleton(typeof(LoggerService));
            services.AddSingleton(new SDK.PackageService());
            services.AddSingleton<ISettingsService>(Helpers.Settings.Default);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked on every keystroke, including system keys such as Alt key combinations.
        /// Used to detect keyboard navigation between pages even when the page itself
        /// doesn't have focus.
        /// </summary>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            // When Alt+Left are pressed navigate back.
            // When Alt+Right are pressed navigate forward.
            if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
                && (e.VirtualKey == VirtualKey.Left || e.VirtualKey == VirtualKey.Right)
                && e.KeyStatus.IsMenuKeyDown && !e.Handled)
            {
                if (e.VirtualKey == VirtualKey.Left)
                {
                    if (m_navService.CurrentFrame.CanGoBack)
                    {
                        m_navService.NavigateBack();
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else if (e.VirtualKey == VirtualKey.Right)
                {
                    if (m_navService.CurrentFrame.CanGoForward)
                    {
                        m_navService.NavigateForward();
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handle system back requests.
        /// </summary>
        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                if (m_navService.CurrentFrame.CanGoBack)
                {
                    m_navService.NavigateBack();
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
        }

        /// <summary>
        /// Handle mouse back button.
        /// </summary>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
        {
            // For this event, e.Handled arrives as 'true'.
            if (e.CurrentPoint.Properties.IsXButton1Pressed)
            {
                if (m_navService.CurrentFrame.CanGoBack)
                {
                    m_navService.NavigateBack();
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else if (e.CurrentPoint.Properties.IsXButton2Pressed)
            {
                if (m_navService.CurrentFrame.CanGoForward)
                {
                    m_navService.NavigateForward();
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
        }
    }
}
