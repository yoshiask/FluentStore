using FluentStore.SDK.Handlers;
using FluentStore.SDK.Messages;
using Flurl;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.Messaging;
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

                            foreach (string ns in handler.HandledNamespaces)
                                _PackageHandlers.Add(ns, handler);
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

        /// <summary>
        /// Performs a search across all package handlers with the given query.
        /// </summary>
        public async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            foreach (var handler in PackageHandlers.Values.Distinct())
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
        /// Gets the package with the specific <paramref name="packageUrn"/>.
        /// </summary>
        public async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            string ns = packageUrn.NamespaceIdentifier;
            if (PackageHandlers.TryGetValue(ns, out var handler))
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
        public async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            PackageBase package = null;
            foreach (PackageHandlerBase handler in PackageHandlers.Values.Distinct())
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
        public Url GetUrlForPackage(PackageBase package)
        {
            string ns = package.Urn.NamespaceIdentifier;
            if (PackageHandlers.TryGetValue(ns, out var handler))
            {
                return handler.GetUrlFromPackage(package);
            }
            else
            {
                throw new NotSupportedException("No package handler is registered for the namespace \"" + ns + "\".");
            }
        }


        // Callbacks

        /// <summary>
        /// Do not call this method. It is only here as a reference for all available messages.
        /// </summary>
        [Obsolete]
        private void RegisterMessageHandlers()
        {
            WeakReferenceMessenger.Default.Register<PackageFetchStartedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageFetchFailedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageFetchCompletedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageDownloadStartedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageDownloadProgressMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageDownloadCompletedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageDownloadCompletedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageInstallStartedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageInstallProgressMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageInstallFailedMessage>(this, (r, m) =>
            {
                
            });

            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                
            });
        }
    }
}
