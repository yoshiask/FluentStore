﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentStore.SDK
{
    public abstract class PackageBase : ObservableObject, IEquatable<PackageBase>
    {
        public abstract string HandlerId { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether <see cref="GetCannotBeInstalledReason"/>
        /// and <see cref="CanBeInstalled"/> requires the package to be downloaded first.
        /// </summary>
        public virtual bool RequiresDownloadForCompatCheck => false;

        /// <summary>
        /// Gets a message describing why this package cannot be installed on this system.
        /// </summary>
        /// <returns><c>null</c> if it can be installed, a reason if it can't.</returns>
        public virtual Task<string> GetCannotBeInstalledReason() => null;

        /// <summary>
        /// Determines if this package can be installed on this system.
        /// </summary>
        public bool CanBeInstalled() => GetCannotBeInstalledReason() == null;

        public abstract Task<bool> InstallAsync();

        public abstract Task<bool> DownloadPackageAsync(string installerPath = null);

        public abstract Task<bool> IsPackageInstalledAsync();

        public virtual void OnDownloaded(StorageFile file) { }

        public virtual bool Equals(PackageBase other) => this.PackageId == other.PackageId;

        public override string ToString() => Title;

        /// <summary>
        /// The underlying model for this package class.
        /// </summary>
        private object _Model;
        public virtual object Model
        {
            get => _Model;
            set => SetProperty(ref _Model, value);
        }

        private PackageStatus _Status = PackageStatus.Unknown;
        public PackageStatus Status
        {
            get => _Status;
            set => SetProperty(ref _Status, value);
        }

        private StorageFile _InstallerFile;
        public StorageFile InstallerFile
        {
            get => _InstallerFile;
            set
            {
                OnDownloaded(value);
                SetProperty(ref _InstallerFile, value);
            }
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _PublisherId;
        public string PublisherId
        {
            get => _PublisherId;
            set => SetProperty(ref _PublisherId, value);
        }

        private string _PackageId = Guid.NewGuid().ToString();
        public string PackageId
        {
            get => _PackageId;
            set => SetProperty(ref _PackageId, value);
        }

        private string _DeveloperName;
        public string DeveloperName
        {
            get => _DeveloperName;
            set => SetProperty(ref _DeveloperName, value);
        }

        /// <summary>
        /// The date this specific package was released.
        /// </summary>
        private DateTimeOffset _ReleaseDate;
        public DateTimeOffset ReleaseDate
        {
            get => _ReleaseDate;
            set => SetProperty(ref _ReleaseDate, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        private string _Version;
        public string Version
        {
            get => _Version;
            set => SetProperty(ref _Version, value);
        }

        private double? _AverageRating;
        public double? AverageRating
        {
            get => _AverageRating;
            set => SetProperty(ref _AverageRating, value);
        }
        public bool HasAverageRating => AverageRating != null;

        private int? _RatingCount;
        public int? RatingCount
        {
            get => _RatingCount;
            set => SetProperty(ref _RatingCount, value);
        }
        public bool HasRatingCount => RatingCount != null;

        private double? _Price;
        public double? Price
        {
            get => _Price;
            set => SetProperty(ref _Price, value);
        }
        public bool HasPrice => Price != null;

        private string _DisplayPrice;
        public string DisplayPrice
        {
            get => _DisplayPrice;
            set => SetProperty(ref _DisplayPrice, value);
        }
        public bool HasDisplayPrice => DisplayPrice != null;

        private string _ShortTitle;
        public string ShortTitle
        {
            get => _ShortTitle ?? Title;
            set => _ShortTitle = value;
        }

        private string _Website;
        public string Website
        {
            get => _Website;
            set => SetProperty(ref _Website, value);
        }
        public bool HasWebsite => Website != null;

        private List<ImageBase> _Images = new List<ImageBase>();
        public List<ImageBase> Images
        {
            get => _Images;
            set => SetProperty(ref _Images, value);
        }
    }

    public abstract class PackageBase<TModel> : PackageBase
    {
        private TModel _Model;
        public new TModel Model
        {
            get => _Model;
            set => SetProperty(ref _Model, value);
        }
    }

    public enum PackageStatus
    {
        Unknown         = 0xFFFF,
        None            = 0,
        DownloadReady   = 1,
        Downloaded      = 2,
        Installed       = 3
    }
}
