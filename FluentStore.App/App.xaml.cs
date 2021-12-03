using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Notifications;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
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
        public const string AppName = "Fluent Store";

        public MainWindow Window { get; private set; }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            //Suspending += OnSuspending;
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
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new()
            {
                Title = "Fluent Store"
            };

            var NavService = Ioc.Default.GetService<INavigationService>() as NavigationService;
            (Type page, object parameter) destination = (typeof(Views.HomeView), null);
            try
            {
                var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs().Data;
                if (activatedArgs is IProtocolActivatedEventArgs ptclArgs)
                {
                    destination = NavService.ParseProtocol(ptclArgs.Uri);
                }
            }
            finally
            {
                NavService.Navigate(destination.page, destination.parameter);
                Window.Activate();
            }
        }

        private static void OnUnhandledException(Exception ex)
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
            services.AddSingleton(new Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.StoreEdgeFDApi());
            services.AddSingleton(typeof(UserService));
            services.AddSingleton(typeof(LoggerService));
            services.AddSingleton(new SDK.PackageService());
            services.AddSingleton<ISettingsService>(Helpers.Settings.Default);

            return services.BuildServiceProvider();
        }

        private void ExtendIntoTitlebar()
        {
            Window.ExtendsContentIntoTitleBar = true;
            //m_window.SetTitleBar(m_window.CustomTitleBar);
        }
    }
}
