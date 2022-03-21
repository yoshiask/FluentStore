using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Images;
using FluentStore.SDK.Users;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public abstract class PackageHandlerBase : IEqualityComparer<PackageHandlerBase>
    {
        protected static readonly List<PackageBase> _emptyPackageList = new(0);
        protected AccountService AccSvc { get; } = Ioc.Default.GetService<AccountService>();

        /// <summary>
        /// A list of all namespaces this handler can handle.
        /// </summary>
        /// <remarks>
        /// Namespaces cannot be shared across handlers.
        /// </remarks>
        public abstract HashSet<string> HandledNamespaces { get; }

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
        public abstract Task<PackageBase> GetPackage(Urn packageUrn);

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
    }
}
