﻿using CommunityToolkit.Mvvm.DependencyInjection;
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

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

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

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Ioc.Default.GetService<IIpfsService>()?.Dispose();
            Ioc.Default.GetService<LoggerService>()?.Dispose();

            Exit();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _singleInstanceApp.Launch(args.Arguments);
        }

        private async void OnSingleInstanceLaunched(object? sender, SingleInstanceLaunchEventArgs e)
        {
            var log = Ioc.Default.GetService<LoggerService>();
            var navService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
            var pluginLoader = Ioc.Default.GetRequiredService<PluginLoader>();
            var appStartupService = Ioc.Default.GetRequiredService<AppStartupInfo>();
            
            await pluginLoader.InitAsync();

            ProtocolResult result = navService.ParseProtocol(e.Arguments, e.IsFirstInstance);
            log?.Log($"Parse protocol result: {result}");

            appStartupService.LaunchResult = result;
            appStartupService.IsFirstLaunch = e.IsFirstLaunch;
            appStartupService.IsFirstInstance = e.IsFirstInstance;

            log?.Log($"Is first instance?: {e.IsFirstInstance}");
            log?.Log($"Is first launch?: {e.IsFirstLaunch}");
            log?.Log($"Redirect activation?: {result.RedirectActivation}");
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
                _log.SetLogLevel(Settings.Default.LoggingLevel);

                // Start OOBE if not configured
                if (!Settings.Default.IsOobeCompleted)
                {
                    Window.DispatcherQueue.TryEnqueue(() =>
                    {
                        var setupWizard = new Views.StartupWizard();
                        setupWizard.SetupCompleted += OnOobeCompleted;

                        Window.WindowContent.Navigate(setupWizard);
                    });
                    return;
                }
            }

            await CompleteAppInitialization();
        }

        private async void OnOobeCompleted(object sender, EventArgs e)
        {
            Settings.Default.IsOobeCompleted = true;
            await Settings.Default.SaveAsync();

            await CompleteAppInitialization();
        }

        private async Task CompleteAppInitialization()
        {
            var log = Ioc.Default.GetService<LoggerService>();
            var appStartupService = Ioc.Default.GetRequiredService<AppStartupInfo>();
            var navService = Ioc.Default.GetRequiredService<NavigationServiceBase>();

            if (appStartupService.IsFirstLaunch)
            {
                var pluginLoader = Ioc.Default.GetRequiredService<PluginLoader>();

                log?.Log($"Began installing pending plugins");
                await pluginLoader.HandlePendingOperations();
                log?.Log($"Finished install pending plugins");

                // Load plugins and initialize package and account services
                log?.Log($"Began loading plugins");
                await pluginLoader.LoadPlugins();
                log?.Log($"Finished loading plugins");

                // Attempt to silently sign into any saved accounts
                var pkgSvc = Ioc.Default.GetRequiredService<PackageService>();
                await pkgSvc.TrySlientSignInAsync();

                // Update last launched version
                Settings.Default.LastLaunchedVersion = Windows.ApplicationModel.Package.Current.Id.Version.ToVersion();
                await Settings.Default.SaveAsync();

                // Connect to IPFS node
                var ipfsService = Ioc.Default.GetRequiredService<IIpfsService>();

                if (!ipfsService.IsRunning)
                {
                    try
                    {
                        await ipfsService.ConnectOrBootstrapAsync(Ioc.Default);
                    }
                    catch (Exception ex)
                    {
                        _log?.LogError(ex, "Failed to connect to Kubo");
                    }
                }

                SDK.Downloads.AbstractStorageHelper.IpfsClient = ipfsService.Client;
            }

            if (!appStartupService.LaunchResult.RedirectActivation || appStartupService.IsFirstInstance)
            {
                log?.Log($"Navigating to {appStartupService.LaunchResult.Page}");

                // Make sure to run on UI thread
                Current.Window.DispatcherQueue.TryEnqueue(() =>
                {
                    Window.WindowContent.Navigate(new MainPage());

                    // Make sure users can't navigate back to
                    // a null page or the splash screen
                    Window.WindowContent.ClearNavigationStack();
                    navService.Navigate(appStartupService.LaunchResult.Page, appStartupService.LaunchResult.Parameter);
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

            // Persist error message to file
            var pathManager = Services.GetRequiredService<ICommonPathManager>();
            var errorFileName = $"Crash_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm-ss-fff}.txt";
            var errorFilePath = System.IO.Path.Combine(pathManager.GetDefaultLogDirectory().FullName, errorFileName);
            using (var textWriter = System.IO.File.CreateText(errorFilePath))
                textWriter.Write(ex.ToString());

            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

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
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("View", $"crash/{errorFileName}")
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

            Exit();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<AppStartupInfo>();

            PackagedPathManager pathManager = new();
            services.AddSingleton<ICommonPathManager>(pathManager);

            Settings settings = new(pathManager);
            services.AddSingleton<ISettingsService>(settings);

            var logFile = pathManager.CreateLogFile();
            var logFileStream = logFile.Open(System.IO.FileMode.Create);
            _log = new LoggerService(logFileStream);
            services.AddSingleton(_log);
            services.AddSingleton<ILogger>(_log);

            services.AddSingleton<NavigationServiceBase, NavigationService>();
            services.AddSingleton<IPasswordVaultService, PasswordVaultService>();
            services.AddSingleton<IIpfsService, IpfsService>();
            services.AddSingleton<Microsoft.Marketplace.Storefront.Contracts.StorefrontApi>();
            services.AddSingleton<FluentStoreAPI.FluentStoreApiClient>();
            services.AddSingleton<PackageService>();
            services.AddSingleton<PluginLoader>();

            return services.BuildServiceProvider();
        }
    }
}
