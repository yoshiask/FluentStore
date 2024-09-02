using FluentStore.SDK.Models;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public class PackageService
    {
        private readonly HashSet<PackageHandlerBase> _packageHandlers = [];
        private readonly Dictionary<string, int> _namespaceRegistry = [];
        private int _nextFreePackageHandlerIndex = 0;

        /// <summary>
        /// A cache of all valid package handlers.
        /// </summary>
        public IReadOnlySet<PackageHandlerBase> PackageHandlers => _packageHandlers;

        /// <summary>
        /// A mapping of known namespaces and the handlers that registered them.
        /// </summary>
        public IReadOnlyDictionary<string, int> NamespaceRegistry => _namespaceRegistry;

        public void RegisterPackageHandler(PackageHandlerBase handler)
        {
            _packageHandlers.Add(handler);

            foreach (string ns in handler.HandledNamespaces)
                _namespaceRegistry.Add(ns, _nextFreePackageHandlerIndex);

            _nextFreePackageHandlerIndex++;
        }

        /// <summary>
        /// Gets a list of featured packages from each package handler.
        /// </summary>
        public async IAsyncEnumerable<HandlerPackageListPair> GetFeaturedPackagesAsync()
        {
            foreach (var handler in GetEnabledPackageHandlers())
            {
                List<PackageBase> results;
                try
                {
                    results = await handler.GetFeaturedPackagesAsync().ToListAsync();
                }
                catch { continue; }

                if (results.Count <= 0)
                    continue;
                yield return new HandlerPackageListPair(handler, results);
            }
        }

        /// <summary>
        /// Performs a search across all package handlers with the given query.
        /// </summary>
        public async Task<IEnumerable<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in GetEnabledPackageHandlers())
            {
                List<PackageBase> results;
                try
                {
                    results = await handler.SearchAsync(query).ToListAsync();
                }
                catch { continue; }

                packages.AddRange(results);
            }

            // Fuzzy search to resort by relevance
            return SortPackages(query, packages);
        }

        /// <summary>
        /// Gets search suggestions for the given query from all package handlers.
        /// </summary>
        public async Task<IEnumerable<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in GetEnabledPackageHandlers())
            {
                List<PackageBase> results;
                try
                {
                    results = await handler.GetSearchSuggestionsAsync(query).ToListAsync();
                }
                catch { continue; }

                packages.AddRange(results);
            }

            // Fuzzy search to resort by relevance
            return SortPackages(query, packages);
        }

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
        public async Task<PackageBase> GetPackageAsync(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            string ns = packageUrn.NamespaceIdentifier;
            PackageHandlerBase handler = GetHandlerForNamespace(ns);
            return await handler.GetPackage(packageUrn, status);
        }

        /// <summary>
        /// Attempts to get the package with the specified <paramref name="packageUrn"/>.
        /// </summary>
        /// <returns>The package if successful, <see langword="null"/> if an exception is thrown.</returns>
        public async Task<PackageBase?> TryGetPackageAsync(Urn packageUrn)
        {
            try
            {
                return await GetPackageAsync(packageUrn);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the package associated with the specified URL.
        /// </summary>
        public async Task<PackageBase> GetPackageFromUrlAsync(Url url)
        {
            PackageBase package = null;
            foreach (var handler in GetEnabledPackageHandlers())
            {
                package = await handler.GetPackageFromUrl(url);
                if (package != null)
                    break;
            }

            return package;
        }

        /// <summary>
        /// Gets the URL of the package on the source website.
        /// </summary>
        public Url GetUrlForPackageAsync(PackageBase package)
        {
            string ns = package.Urn.NamespaceIdentifier;
            return GetHandlerForNamespace(ns).GetUrlFromPackage(package);
        }

        /// <summary>
        /// Gets a list of package collections.
        /// </summary>
        /// <remarks>
        /// Typically, this method will return a list of <see cref="Packages.GenericPackageCollection{TModel}"/>,
        /// but this is not a requirement and technically any package is allowed.
        /// </remarks>
        public async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            foreach (var handler in GetEnabledPackageHandlers())
            {
                IAsyncEnumerable<PackageBase> results;
                try
                {
                    results = handler.GetCollectionsAsync();
                }
                catch { continue; }

                await foreach (var result in results)
                    yield return result;
            }
        }

        /// <summary>
        /// Gets the handler registered for the given namespace.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public PackageHandlerBase GetHandlerForNamespace(string ns)
        {
            if (NamespaceRegistry.TryGetValue(ns, out var handlerIdx))
            {
                var handler = PackageHandlers.ElementAt(handlerIdx);
                if (!handler.IsEnabled())
                    throw new InvalidOperationException($"The {handler.DisplayName} package handler is disabled. Please go to settings and re-enable it.");
                return handler;
            }
            else
            {
                throw new NotSupportedException("No package handler is registered for the namespace \"" + ns + "\".");
            }
        }

        /// <inheritdoc cref="GetHandlerForNamespace(string)"/>
        public PackageHandlerBase GetHandlerForNamespace(Urn packageUrn) => GetHandlerForNamespace(packageUrn.NamespaceIdentifier);


        /// <inheritdoc cref="GetHandlerImage(string)"/>
        public Images.ImageBase GetHandlerImage(Urn packageUrn) => GetHandlerImage(packageUrn.NamespaceIdentifier);

        /// <summary>
        /// Gets an image that represents the handler registered for the given namespace.
        /// </summary>
        public Images.ImageBase GetHandlerImage(string ns) => GetHandlerForNamespace(ns).Image;


        /// <inheritdoc cref="GetHandlerDisplayName(string)"/>
        public string GetHandlerDisplayName(Urn packageUrn) => GetHandlerDisplayName(packageUrn.NamespaceIdentifier);

        /// <summary>
        /// Gets the display name for the handler registered for the given namespace.
        /// </summary>
        public string GetHandlerDisplayName(string ns) => GetHandlerForNamespace(ns).DisplayName;

        private static IEnumerable<PackageBase> SortPackages(string query, IList<PackageBase> packages)
        {
            return FuzzySharp.Process
                .ExtractSorted(query, packages.Select(p => p.Title))
                .Select(r => packages[r.Index]);
        }

        #region Account handling

        /// <summary>
        /// Attempts a silent sign-in using saved credentials
        /// on all available account handlers.
        /// </summary>
        public async Task TrySlientSignInAsync()
        {
            foreach (var handler in GetEnabledPackageHandlers())
            {
                if (handler.AccountHandler is null) continue;

                await handler.AccountHandler.TrySilentSignInAsync();
            }
        }

        /// <summary>
        /// Passes control to the appropriate <see cref="Users.AccountHandlerBase"/>
        /// after the app is activated with the <c>auth</c> protocol.
        /// </summary>
        /// <param name="url">
        /// The <see cref="Url"/> the app was activated with.
        /// </param>
        public async Task RouteAuthActivation(Url url)
        {
            string id = url.PathSegments.Last();
            var handler = GetAccountHandler(id);
            await handler.HandleAuthActivation(url);
        }

        /// <summary>
        /// Gets the account handler with the given ID.
        /// </summary>
        /// <exception cref="NotSupportedException">When no account handler matches the given ID.</exception>
        /// <exception cref="InvalidOperationException"/>
        public Users.AccountHandlerBase GetAccountHandler(string id)
        {
            var handler = GetAccountHandlers(true)
                .FirstOrDefault(ah => ah.Id == id);

            if (handler != null)
                return handler;

            throw new NotSupportedException($"Could not find an account handler with ID \"{id}\".");
        }

        /// <summary>
        /// Gets all available account handlers.
        /// </summary>
        /// <param name="includeDisabled">
        /// Whether to include disabled account handlers.
        /// </param>
        public IEnumerable<Users.AccountHandlerBase> GetAccountHandlers(bool includeDisabled = false)
        {
            return GetEnabledPackageHandlers()
                .Select(ph => ph.AccountHandler)
                .Where(ah => ah != null && (ah.IsEnabled || includeDisabled));
        }

        #endregion

        public IEnumerable<PackageHandlerBase> GetEnabledPackageHandlers() =>
            PackageHandlers.Where(h => h.IsEnabled());
    }
}
