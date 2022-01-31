using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Handlers;
using FluentStore.SDK.Models;
using FluentStore.Services;
using Flurl;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public class PackageService
    {
        private Dictionary<string, PackageHandlerBase> _PackageHandlers;
        /// <summary>
        /// A cache of all valid package handlers. The key is the name of the <see cref="Type"/>,
        /// and the value is an instance of the <see cref="PackageHandlerBase"/>
        /// </summary>
        public Dictionary<string, PackageHandlerBase> PackageHandlers
        {
            get
            {
                // Use reflection to create an instance of each handler and add it to the regsitry
                if (_PackageHandlers == null)
                {
                    ISettingsService Settings = Ioc.Default.GetRequiredService<ISettingsService>();
                    var emptyTypeList = Array.Empty<Type>();
                    var emptyObjectList = Array.Empty<object>();
                    _PackageHandlers = new Dictionary<string, PackageHandlerBase>();

                    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace == "FluentStore.SDK.Handlers" && t.IsClass && t.IsPublic))
                    {
                        var ctr = type.GetConstructor(emptyTypeList);
                        if (ctr == null)
                            continue;
                        try
                        {
                            var handler = (PackageHandlerBase)ctr.Invoke(emptyObjectList);
                            if (handler == null)
                                continue;

                            // Enable or disable according to user settings
                            handler.IsEnabled = Settings.GetPackageHandlerEnabledState(type.Name);

                            _PackageHandlers.Add(type.Name, handler);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(ex);
#endif
                            continue;
                        }
                    }
                }

                return _PackageHandlers;
            }
        }

        private Dictionary<string, int> _NamespaceRegistry;
        /// <summary>
        /// A mapping of known namespaces and the handlers that registered them.
        /// </summary>
        public Dictionary<string, int> NamespaceRegistry
        {
            get
            {
                if (_NamespaceRegistry == null)
                {
                    _NamespaceRegistry = new();
                    int i = 0;
                    foreach (PackageHandlerBase handler in PackageHandlers.Values)
                    {
                        foreach (string ns in handler.HandledNamespaces)
                            _NamespaceRegistry.Add(ns, i);
                        i++;
                    }
                }

                return _NamespaceRegistry;
            }
        }

        /// <summary>
        /// Gets a list of featured packages from each package handler.
        /// </summary>
        public async IAsyncEnumerable<HandlerPackageListPair> GetFeaturedPackagesAsync()
        {
            foreach (var handler in PackageHandlers.Values)
            {
                if (!handler.IsEnabled) continue;

                List<PackageBase> results;
                try
                {
                    results = await handler.GetFeaturedPackagesAsync();
                } catch { continue; }
                if (results.Count <= 0)
                    continue;
                yield return new HandlerPackageListPair(handler, results);
            }
        }

        /// <summary>
        /// Performs a search across all package handlers with the given query.
        /// </summary>
        public async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in PackageHandlers.Values)
            {
                if (!handler.IsEnabled) continue;

                List<PackageBase> results;
                try
                {
                    results = await handler.SearchAsync(query);
                } catch { continue; }
                // Filter results already in list
                packages.AddRange(results);
            }

            // Fuzzy search to resort by relevance
            var scorer = ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.StrategySensitive.TokenDifferenceScorer>();
            return Process.ExtractSorted(query, packages.Select(p => p.Title + " - " + p.DeveloperName), scorer: scorer)
                .Select(r => packages.ElementAt(r.Index)).ToList();
        }

        /// <summary>
        /// Gets search suggestions for the given query from all package handlers.
        /// </summary>
        public async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in PackageHandlers.Values)
            {
                if (!handler.IsEnabled) continue;

                List<PackageBase> results;
                try
                {
                    results = await handler.GetSearchSuggestionsAsync(query);
                }
                catch { continue; }
                // Filter results already in list
                packages.AddRange(results);
            }

            // Fuzzy search to resort by relevance
            var scorer = ScorerCache.Get<FuzzySharp.SimilarityRatio.Scorer.StrategySensitive.TokenDifferenceScorer>();
            return Process.ExtractSorted(query, packages.Select(p => p.Title + " - " + p.DeveloperName), scorer: scorer)
                .Select(r => packages.ElementAt(r.Index)).ToList();
        }

        /// <summary>
        /// Gets the package with the specified <paramref name="packageUrn"/>.
        /// </summary>
        public async Task<PackageBase> GetPackageAsync(Urn packageUrn)
        {
            string ns = packageUrn.NamespaceIdentifier;
            PackageHandlerBase handler = GetHandlerForNamespace(ns);
            return await handler.GetPackage(packageUrn);
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
            foreach (PackageHandlerBase handler in PackageHandlers.Values)
            {
                if (!handler.IsEnabled) continue;

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
        /// Gets the handler registered for the given namespace.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public PackageHandlerBase GetHandlerForNamespace(string ns)
        {
            if (NamespaceRegistry.TryGetValue(ns, out var handlerIdx))
            {
                var handler = PackageHandlers.Values.ElementAt(handlerIdx);
                if (!handler.IsEnabled)
                    throw new InvalidOperationException($"The {handler.DisplayName} package handler is disabled. Please go to settings and re-enable it.");
                return handler;
            }
            else
            {
                throw new NotSupportedException("No package handler is registered for the namespace \"" + ns + "\".");
            }
        }

        /// <summary>
        /// Gets the handler with the given [class] name.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public PackageHandlerBase GetHandlerByName(string name)
        {
            if (PackageHandlers.TryGetValue(name, out var handler))
            {
                return handler;
            }
            else
            {
                throw new NotSupportedException("A package handler with the name \"" + name + "\" was not registered.");
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
    }
}
