using FluentStore.SDK.Images;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public abstract class PackageHandlerBase : IEqualityComparer<PackageHandlerBase>
    {
        protected static readonly List<PackageBase> _emptyPackageList = new(0);

        /// <summary>
        /// Initializes a new instance of <see cref="PackageHandlerBase"/>.
        /// </summary>
        /// <param name="passwordVaultService">
        /// The <see cref="IPasswordVaultService"/> to be used when
        /// instantiating a new <see cref="AccountHandlerBase"/>.
        /// </param>
        public PackageHandlerBase(IPasswordVaultService passwordVaultService)
        {

        }

        /// <summary>
        /// A list of all namespaces this handler can handle.
        /// </summary>
        /// <remarks>
        /// Namespaces cannot be shared across handlers.
        /// </remarks>
        public abstract HashSet<string> HandledNamespaces { get; }

        /// <summary>
        /// Whether this package handler is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        private ImageBase _Image;
        /// <summary>
        /// An image that represents this handler.
        /// </summary>
        /// <remarks>
        /// Caches the result of <see cref="GetImage"/>.
        /// </remarks>
        public ImageBase Image
        {
            get
            {
                if (_Image == null)
                    _Image = GetImage();
                return _Image;
            }
        }

        /// <summary>
        /// The current account handler for this package handler.
        /// </summary>
        public AccountHandlerBase AccountHandler { get; protected set; }

        /// <summary>
        /// The display name of this handler.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets an image that represents this handler.
        /// </summary>
        public abstract ImageBase GetImage();

        /// <summary>
        /// Gets a list of featured packages.
        /// </summary>
        public abstract Task<List<PackageBase>> GetFeaturedPackagesAsync();

        /// <summary>
        /// Performs a search using the given query.
        /// </summary>
        public abstract Task<List<PackageBase>> SearchAsync(string query);

        /// <summary>
        /// Gets search suggestions for the given query.
        /// </summary>
        public abstract Task<List<PackageBase>> GetSearchSuggestionsAsync(string query);

        /// <summary>
        /// Gets the package with the specified <paramref name="packageUrn"/>.
        /// </summary>
        /// <param name="packageUrn">
        /// The URN of the package to get.
        /// </param>
        /// <param name="targetStatus">
        /// Specifies how much package information to load.
        /// <see cref="PackageStatus.BasicDetails"/> and <see cref="PackageStatus.Details"/>
        /// are the only valid options.
        /// </param>
        public abstract Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus targetStatus = PackageStatus.Details);

        /// <summary>
        /// Gets the package associated with the specified URL.
        /// </summary>
        public abstract Task<PackageBase> GetPackageFromUrl(Url url);

        /// <summary>
        /// Gets the URL of the package on the source website.
        /// </summary>
        public abstract Url GetUrlFromPackage(PackageBase package);

        /// <summary>
        /// Gets a list of package collections.
        /// </summary>
        /// <remarks>
        /// Typically, this method will return a list of <see cref="Packages.GenericPackageCollection{TModel}"/>,
        /// but this is not a requirement and technically any package is allowed.
        /// </remarks>
        public virtual Task<List<PackageBase>> GetCollectionsAsync() => Task.FromResult(_emptyPackageList);

        public bool Equals(PackageHandlerBase x, PackageHandlerBase y) => x.GetType() == y.GetType();

        public int GetHashCode(PackageHandlerBase obj) => obj.GetType().GetHashCode();

        #region Package editing

        /// <summary>
        /// Determines whether the specified package can be edited.
        /// </summary>
        /// <param name="package">
        /// The package to check.
        /// </param>
        public virtual bool CanEditPackage(PackageBase package) => false;

        /// <summary>
        /// Determines whether packages can be added or removed from the specified
        /// package collection.
        /// </summary>
        /// <param name="package">
        /// The collection to check.
        /// </param>
        public virtual bool CanEditCollection(PackageBase package) => false;

        /// <summary>
        /// Attempts to save changes made to the package.
        /// </summary>
        /// <param name="package">
        /// The package with changes to apply.
        /// </param>
        /// <returns>
        /// Whether the package was saved successfully.
        /// </returns>
        public virtual Task<bool> SavePackageAsync(PackageBase package) => Task.FromResult(false);

        #endregion

        #region Package deleting

        /// <summary>
        /// Determines whether the specified package can be deleted.
        /// </summary>
        /// <param name="package">
        /// The package to check.
        /// </param>
        public virtual bool CanDeletePackage(PackageBase package) => false;

        /// <summary>
        /// Attempts to delete the package.
        /// </summary>
        /// <param name="package">
        /// The package to delete.
        /// </param>
        /// <returns>
        /// Whether the package was deleted successfully.
        /// </returns>
        public virtual Task<bool> DeletePackageAsync(PackageBase package) => Task.FromResult(false);

        #endregion

        #region Package creation

        /// <summary>
        /// Determines whether new packages can be created.
        /// </summary>
        public virtual bool CanCreatePackage() => false;

        /// <summary>
        /// Creates a new package.
        /// </summary>
        /// <returns>
        /// The new package if <see cref="CanCreatePackage"/> is <see langword="true"/>,
        /// <see langword="null"/> if <see langword="false"/>.
        /// </returns>
        public virtual Task<PackageBase> CreatePackage() => Task.FromResult<PackageBase>(null);

        #endregion
    }
}
