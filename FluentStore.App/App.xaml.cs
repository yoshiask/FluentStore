using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using FluentStore.Helpers;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Plugins;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
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
        private readonly SingleInstanceDesktopApp _singleInstanceApp;
        private LoggerService _log;

        public const string AppName = "Fluent Store";

        public MainWindow Window { get; private set; }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Set up error reporting handlers
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) => _log?.UnhandledException(e.Exception, LogLevel.Error);
            AppDomain.CurrentDomain.UnhandledException += (sender, e)
              => OnUnhandledException(e.ExceptionObject as Exception ?? new Exception());
            UnhandledException += (sender, e) => OnUnhandledException(e.Exception);
            TaskScheduler.UnobservedTaskException += (sender, e) => OnUnhandledException(e.Exception);

            // Set up IoC services
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);

            _singleInstanceApp = new SingleInstanceDesktopApp("FluentStoreBeta");
            _singleInstanceApp.Launched += OnSingleInstanceLaunched;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _singleInstanceApp.Launch(args.Arguments);
        }

        private async void OnSingleInstanceLaunched(object? sender, SingleInstanceLaunchEventArgs e)
        {
            var log = Ioc.Default.GetService<LoggerService>();
            var navService = Ioc.Default.GetRequiredService<INavigationService>();
            var pluginLoader = Ioc.Default.GetRequiredService<PluginLoader>();

            ProtocolResult result = navService.ParseProtocol(e.Arguments, isFirstInstance: e.IsFirstInstance);
            log?.Log($"Parse protocol result: {result}");

            log?.Log($"Is first instance?: {e.IsFirstInstance}");
            log?.Log($"Is first launch?: {e.IsFirstLaunch}");
            log?.Log($"Single-instance launch args: {e.Arguments}");

            if (e.IsFirstLaunch)
            {
                Window = new()
                {
                    Title = AppName
                };
                
                // Make sure to run on UI thread
                Window.DispatcherQueue.TryEnqueue(() =>
                {
                    Window.WindowContent.Navigate(new Views.SplashScreen());
                    Window.Activate();
                });

                await Settings.Default.LoadAsync();

                // Check if app was updated
                switch (Settings.Default.GetAppUpdateStatus())
                {
                    case AppUpdateStatus.NewlyInstalled:
                        // Download and install default plugins
                        log?.Log($"Began installing default plugins");
                        await pluginLoader.InstallDefaultPlugins();
                        log?.Log($"Finished installing plugins");
                        break;

                    default:
                        // Always install pending plugins
                        log?.Log($"Began installing pending plugins");
                        await pluginLoader.InstallPendingPlugins();
                        log?.Log($"Finished install pending plugins");
                        break;
                }

                // Load plugins and initialize package and account services
                var passwordVaultService = Ioc.Default.GetRequiredService<IPasswordVaultService>();
                var pkgSvc = Ioc.Default.GetRequiredService<PackageService>();
                Settings.Default.PackageHandlerEnabledStateChanged += pkgSvc.UpdatePackageHandlerEnabledStates;

                log?.Log($"Began loading plugins");
                var pluginLoadResult = await pluginLoader.LoadPlugins();
                pkgSvc.PackageHandlers = pluginLoadResult.PackageHandlers;
                log?.Log($"Finished loading plugins");

                // Attempt to silently sign into any saved accounts
                await pkgSvc.TrySlientSignInAsync();

                // Update last launched version
                var appVersion = Windows.ApplicationModel.Package.Current.Id.Version.ToVersion();
                Settings.Default.LastLaunchedVersion = appVersion;
                await Settings.Default.SaveAsync();
            }
            log?.Log($"Redirect activation?: {result.RedirectActivation}");

            if (!result.RedirectActivation || e.IsFirstInstance)
            {
                log?.Log($"Navigating to {result.Page}");

                // Make sure to run on UI thread
                Current.Window.DispatcherQueue.TryEnqueue(() =>
                {
                    Window.WindowContent.Navigate(new MainPage());

                    // Make sure users can't navigate back to
                    // a null page or the splash screen
                    Window.WindowContent.ClearNavigationStack();

                    navService.Navigate(result.Page, result.Parameter);
                });
            }
        }

        private void OnUnhandledException(Exception ex)
        {
            /// Adapted from https://github.com/files-community/Files/blob/ace2f355ec87f4ca27975c25026636be8514f1e0/Files/App.xaml.cs#L432

            _log?.UnhandledException(ex, LogLevel.Critical);

#if DEBUG
            System.Diagnostics.Debugger.Launch();
            System.Diagnostics.Debugger.Break(); // Please check "Output Window" for exception details (View -> Output Window) (CTRL + ALT + O)
#endif

            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

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
        /// Configures the services for the application.
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            PackagedPathManager pathManager = new();
            services.AddSingleton<ICommonPathManager>(pathManager);

            Settings settings = new(pathManager);
            services.AddSingleton<ISettingsService>(settings);

            var logFile = pathManager.CreateLogFile();
            var logFileStream = logFile.Open(System.IO.FileMode.Create);
            _log = new LoggerService(logFileStream)
            {
                LogLevel = settings.LoggingLevel.ToMsLogLevel()
            };
            services.AddSingleton(_log);
            services.AddSingleton<ILogger>(_log);

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPasswordVaultService, PasswordVaultService>();
            services.AddSingleton<Microsoft.Marketplace.Storefront.Contracts.StorefrontApi>();
            services.AddSingleton<FluentStoreAPI.FluentStoreAPI>();
            services.AddSingleton<PackageService>();
            services.AddSingleton<PluginLoader>();

            return services.BuildServiceProvider();
        }
    }
}
