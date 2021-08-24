using FluentStore.SDK.Handlers;
using FluentStore.SDK.Models;
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
                    var emptyTypeList = new Type[0];
                    var emptyObjectList = new object[0];
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

        private Dictionary<string, PackageHandlerBase> _NamespaceRegistry;
        /// <summary>
        /// A mapping of known namespaces and the handlers that registered them.
        /// </summary>
        public Dictionary<string, PackageHandlerBase> NamespaceRegistry
        {
            get
            {
                if (_NamespaceRegistry == null)
                {
                    _NamespaceRegistry = new Dictionary<string, PackageHandlerBase>();
                    foreach (PackageHandlerBase handler in PackageHandlers.Values)
                        foreach (string ns in handler.HandledNamespaces)
                            _NamespaceRegistry.Add(ns, handler);
                }

                return _NamespaceRegistry;
            }
        }

        /// <summary>
        /// Gets a list of featured packages from each package handler.
        /// </summary>
        public async Task<List<HandlerPackageListPair>> GetFeaturedPackagesAsync()
        {
            var lists = new List<HandlerPackageListPair>();

            foreach (var handler in PackageHandlers.Values)
            {
                var results = await handler.GetFeaturedPackagesAsync();
                if (results.Count <= 0)
                    continue;
                lists.Add(new HandlerPackageListPair(handler, results));
            }

            return lists;
        }

        /// <summary>
        /// Performs a search across all package handlers with the given query.
        /// </summary>
        public async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in PackageHandlers.Values)
            {
                var results = await handler.SearchAsync(query);
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
                var results = await handler.GetSearchSuggestionsAsync(query);
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
            if (NamespaceRegistry.TryGetValue(ns, out var handler))
            {
                return await handler.GetPackage(packageUrn);
            }
            else
            {
                throw new NotSupportedException("No package handler is registered for the namespace \"" + ns + "\".");
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
            if (NamespaceRegistry.TryGetValue(ns, out var handler))
            {
                return handler.GetUrlFromPackage(package);
            }
            else
            {
                throw new NotSupportedException("No package handler is registered for the namespace \"" + ns + "\".");
            }
        }
    }
}
