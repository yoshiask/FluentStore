using FluentStore.SDK.Handlers;
using FluentStore.SDK.Messages;
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
                        var handler = (PackageHandlerBase)ctr.Invoke(emptyObjectList);

                        foreach (string ns in handler.HandledNamespaces)
                            _PackageHandlers.Add(ns, handler);
                    }
                }

                return _PackageHandlers;
            }
        }

        /// <summary>
        /// Performs a search across all package handlers with the given query.
        /// </summary>
        public async Task<PackageCollection> SearchAsync(string query)
        {
            var packages = new PackageCollection();
            foreach (var handler in PackageHandlers.Values.Distinct())
            {
                var results = await handler.SearchAsync(query);
                // Filter results already in list
                packages.AddPackages(results);
            }
            return packages;
        }

        /// <summary>
        /// Gets search suggestions for the given query from all package handlers.
        /// </summary>
        public async Task<PackageCollection> GetSearchSuggestionsAsync(string query)
        {
            var packages = new PackageCollection();
            foreach (var handler in PackageHandlers.Values)
            {
                var results = await handler.GetSearchSuggestionsAsync(query);
                // Filter results already in list
                packages.AddPackages(results);
            }
            return packages;
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
