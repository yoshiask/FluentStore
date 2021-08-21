using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentStore
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private NavigationService NavService;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnActivated(e);
        }

        /// <inheritdoc/>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            NavService = Ioc.Default.GetService<INavigationService>() as NavigationService;
            ExtendIntoTitlebar();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Add support for accelerator keys. 
                // Listen to the window directly so the app responds
                // to accelerator keys regardless of which element has focus.
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
                // Add support for system back requests. 
                SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
                // Add support for mouse navigation buttons. 
                Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            NavService.AppFrame = rootFrame;
            Tuple<Type, object> destination = new Tuple<Type, object>(typeof(Views.HomeView), null);

            if (args is LaunchActivatedEventArgs launchArgs && launchArgs.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    destination = NavService.ParseProtocol(launchArgs.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs ptclArgs = args as ProtocolActivatedEventArgs;
                // The received URI is eventArgs.Uri.AbsoluteUri

                destination = NavService.ParseProtocol(ptclArgs.Uri);
            }
            rootFrame.Navigate(typeof(MainPage), destination);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
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

        private static void ExtendIntoTitlebar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = (Color)App.Current.Resources["SystemBaseHighColor"];
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
            ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
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
                    if (NavService.CurrentFrame.CanGoBack)
                    {
                        NavService.NavigateBack();
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else if (e.VirtualKey == VirtualKey.Right)
                {
                    if (NavService.CurrentFrame.CanGoForward)
                    {
                        NavService.NavigateForward();
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
                if (NavService.CurrentFrame.CanGoBack)
                {
                    NavService.NavigateBack();
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
                if (NavService.CurrentFrame.CanGoBack)
                {
                    NavService.NavigateBack();
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else if (e.CurrentPoint.Properties.IsXButton2Pressed)
            {
                if (NavService.CurrentFrame.CanGoForward)
                {
                    NavService.NavigateForward();
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
