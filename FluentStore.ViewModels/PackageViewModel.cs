using FluentStore.SDK;
using FluentStore.SDK.Attributes;
using FluentStore.SDK.Images;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using OwlCore.AbstractUI.ViewModels;
using OwlCore.AbstractUI.Models;
using CommunityToolkit.Diagnostics;

namespace FluentStore.ViewModels
{
    public class PackageViewModel : ObservableObject
    {
        public PackageViewModel()
        {
            ViewPackageCommand = new AsyncRelayCommand<object>(ViewPackage);
            RefreshCommand = new AsyncRelayCommand(Refresh);
        }
        public PackageViewModel(PackageBase package) : this()
        {
            Guard.IsNotNull(package);
            Package = package;
        }

        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
        public readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly LoggerService Logger = Ioc.Default.GetService<LoggerService>();

        private PackageBase _Package;
        public PackageBase Package
        {
            get => _Package;
            set
            {
                SetProperty(ref _Package, value);

                // Reset cached properties
                DisplayProperties = null;
                DisplayAdditionalInformationProperties = null;
                UpdateImages();

                // Update derived properties
                IsCollection = Package != null && Package.GetType().IsAssignableTo(typeof(SDK.Packages.IPackageCollection));
                CanEdit = Package?.PackageHandler?.CanEditPackage(Package) ?? false;
                CanDelete = Package?.PackageHandler?.CanDeletePackage(Package) ?? false;
                HasReviews = Package?.ReviewSummary?.HasReviews ?? false;

                Package.GetCannotBeInstalledReason().ContinueWith((task, obj) =>
                {
                    if (!task.IsCompletedSuccessfully)
                        return;

                    var vm = (PackageViewModel)obj;
                    string reason = task.Result;

                    vm.CannotInstallReason = reason ?? "Download and install now";
                    vm.CanInstall = reason is null;
                }, this);
            }
        }

        private IAsyncRelayCommand<object> _ViewPackageCommand;
        public IAsyncRelayCommand<object> ViewPackageCommand
        {
            get => _ViewPackageCommand;
            set => SetProperty(ref _ViewPackageCommand, value);
        }

        private IAsyncRelayCommand<object> _DownloadCommand;
        public IAsyncRelayCommand<object> DownloadCommand
        {
            get => _DownloadCommand;
            set => SetProperty(ref _DownloadCommand, value);
        }

        private IAsyncRelayCommand<object> _InstallCommand;
        public IAsyncRelayCommand<object> InstallCommand
        {
            get => _InstallCommand;
            set => SetProperty(ref _InstallCommand, value);
        }

        private IAsyncRelayCommand<object> _SaveToCollectionCommand;
        public IAsyncRelayCommand<object> SaveToCollectionCommand
        {
            get => _SaveToCollectionCommand;
            set => SetProperty(ref _SaveToCollectionCommand, value);
        }

        private IAsyncRelayCommand _RefreshCommand;
        public IAsyncRelayCommand RefreshCommand
        {
            get => _RefreshCommand;
            set => SetProperty(ref _RefreshCommand, value);
        }

        private bool _IsCollection;
        public bool IsCollection
        {
            get => _IsCollection;
            private set => SetProperty(ref _IsCollection, value);
        }

        private bool _CanEdit;
        public bool CanEdit
        {
            get => _CanEdit;
            set => SetProperty(ref _CanEdit, value);
        }

        private bool _CanDelete;
        public bool CanDelete
        {
            get => _CanDelete;
            set => SetProperty(ref _CanDelete, value);
        }

        private bool _CanInstall = false;
        public bool CanInstall
        {
            get => _CanInstall;
            set => SetProperty(ref _CanInstall, value);
        }

        private string _CannotInstallReason = "Unknown";
        public string CannotInstallReason
        {
            get => _CannotInstallReason;
            set => SetProperty(ref _CannotInstallReason, value);
        }

        private ImageBase _AppIcon;
        public ImageBase AppIcon
        {
            get => _AppIcon;
            set => SetProperty(ref _AppIcon, value);
        }

        private ImageBase _HeroImage;
        public ImageBase HeroImage
        {
            get => _HeroImage;
            set => SetProperty(ref _HeroImage, value);
        }

        private List<ImageBase> _Screenshots;
        public List<ImageBase> Screenshots
        {
            get => _Screenshots;
            set => SetProperty(ref _Screenshots, value);
        }

        private ImageBase _SelectedScreenshot;
        public ImageBase SelectedScreenshot
        {
            get => _SelectedScreenshot;
            set
            {
                SelectedScreenshotNumLabel = (Screenshots.IndexOf(value) + 1).ToString();
                SetProperty(ref _SelectedScreenshot, value);
            }
        }

        private string _SelectedScreenshotNum;
        public string SelectedScreenshotNumLabel
        {
            get => _SelectedScreenshotNum;
            set => SetProperty(ref _SelectedScreenshotNum, value);
        }

        public string AverageRatingString => Package != null && Package.HasReviewSummary && Package.ReviewSummary.HasAverageRating
            ? Package.ReviewSummary.AverageRating.ToString("F1")
            : string.Empty;

        private bool _HasReviews;
        public bool HasReviews
        {
            get => _HasReviews;
            set => SetProperty(ref _HasReviews, value);
        }

        private bool _IsInstalling;
        public bool IsInstalling
        {
            get => _IsInstalling;
            set => SetProperty(ref _IsInstalling, value);
        }

        public async Task ViewPackage(object obj = null)
        {
            PackageViewModel pvm;
            if (obj == null)
            {
                pvm = this;
            }
            else
            {
                pvm = obj switch
                {
                    PackageViewModel viewModel => viewModel,
                    PackageBase package => new PackageViewModel(package),
                    _ => throw new ArgumentException($"'{nameof(obj)}' is an invalid type: {obj.GetType().Name}"),
                };
            }

            if (pvm.Package.Status < PackageStatus.Details)
            {
                // Load all details
                await Refresh();
            }

            NavigationService.Navigate(pvm);
        }

        public async Task Refresh()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
            try
            {
                Package = await PackageService.GetPackageAsync(Package.Urn);
            }
            catch (SDK.Models.WebException ex)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                NavigationService.ShowHttpErrorPage(ex.StatusCode, ex.Message);
            }
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public IEnumerable<AbstractButtonViewModel> GetAdditionalCommands()
        {
            foreach (AbstractButton buttonModel in Package.GetAdditionalCommands())
                yield return new(buttonModel);
        }

        private List<DisplayInfo> _DisplayProperties;
        /// <summary>
        /// Gets the value of all properties with <see cref="DisplayAttribute"/> applied.
        /// </summary>
        public List<DisplayInfo> DisplayProperties
        {
            get
            {
                if (_DisplayProperties == null)
                {
                    _DisplayProperties = new List<DisplayInfo>();
                    if (Package == null)
                        return _DisplayProperties;

                    Type type = Package.GetType();
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                        // Filter out properties without the attribute, and ignore DisplayAdditionalInformationInfo
                        if (displayAttr == null || displayAttr.GetType() != typeof(DisplayAttribute))
                            continue;

                        object value = prop.GetValue(Package);
                        object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                        if (value == null || value.Equals(defaultValue) || (value is System.Collections.IList list && list.Count == 0))
                            continue;

                        var info = new DisplayInfo(displayAttr, value);
                        if (displayAttr.Rank >= _DisplayProperties.Count)
                            _DisplayProperties.Add(info);
                        else
                            _DisplayProperties.Insert(displayAttr.Rank, info);
                    }
                }
                return _DisplayProperties;
            }
            set => SetProperty(ref _DisplayProperties, value);
        }

        private List<DisplayAdditionalInformationInfo> _DisplayAdditionalInformationProperties;
        /// <summary>
        /// Gets the value of all properties with <see cref="DisplayAdditionalInformationAttribute"/> applied.
        /// </summary>
        public List<DisplayAdditionalInformationInfo> DisplayAdditionalInformationProperties
        {
            get
            {
                if (_DisplayAdditionalInformationProperties == null)
                {
                    _DisplayAdditionalInformationProperties = new List<DisplayAdditionalInformationInfo>();
                    if (Package == null)
                        return _DisplayAdditionalInformationProperties;

                    Type type = Package.GetType();
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        var displayAttr = prop.GetCustomAttribute<DisplayAdditionalInformationAttribute>();
                        // Filter out properties without the attribute
                        if (displayAttr == null)
                            continue;

                        object value = prop.GetValue(Package, null);
                        object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                        if (value == null || value.Equals(defaultValue) || (value is System.Collections.IList list && list.Count == 0))
                            continue;

                        var info = new DisplayAdditionalInformationInfo(displayAttr, value);
                        if (displayAttr.Rank >= _DisplayAdditionalInformationProperties.Count)
                            _DisplayAdditionalInformationProperties.Add(info);
                        else
                            _DisplayAdditionalInformationProperties.Insert(displayAttr.Rank, info);
                    }
                }
                return _DisplayAdditionalInformationProperties;
            }
            set => SetProperty(ref _DisplayAdditionalInformationProperties, value);
        }

        public static implicit operator PackageBase(PackageViewModel pvm) => pvm.Package;
        public static implicit operator PackageViewModel(PackageBase pb) => new(pb);

        public override string ToString() => Package.ToString();

        private void HandleFaultedTask(Task task)
        {
            if (!task.IsFaulted) return;
            Logger?.UnhandledException(task.Exception, Microsoft.Extensions.Logging.LogLevel.Warning);
        }

        private void UpdateImages()
        {
            AppIcon = null;
            HeroImage = null;
            Screenshots = null;

            // Cache images in the background
            var context = System.Threading.SynchronizationContext.Current;
            Package.GetAppIcon().ContinueWith(task =>
            {
                HandleFaultedTask(task);
                context.Post(img => AppIcon = (ImageBase)img, task.Result);
            });

            Package.GetHeroImage().ContinueWith(task =>
            {
                HandleFaultedTask(task);
                context.Post(img => HeroImage = (ImageBase)img, task.Result);
            });

            Package.GetScreenshots().ContinueWith(task =>
            {
                HandleFaultedTask(task);
                context.Post(img => Screenshots = (List<ImageBase>)img, task.Result);
            });
        }
    }
}
