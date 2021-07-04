using FluentStore.SDK.Handlers;
using FluentStore.SDK.Messages;
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
                        _PackageHandlers.Add(type.Name, handler);
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
            foreach (var handler in PackageHandlers.Values)
            {
                var results = await handler.SearchAsync(query);
                // Filter results already in list
                foreach (var result in results)
                {
                    if (packages.ContainsKey(result.PackageId))
                        packages[result.PackageId].Add(result);
                    else
                        packages.Add(result.PackageId, new List<PackageBase> { result });
                }
            }
            return packages;
        }

        public async Task<PackageCollection> GetSearchSuggestionsAsync(string query)
        {
            var packages = new PackageCollection();
            foreach (var handler in PackageHandlers.Values)
            {
                var results = await handler.GetSearchSuggestionsAsync(query);
                // Filter results already in list
                foreach (var result in results)
                {
                    if (packages.ContainsKey(result.PackageId))
                        packages[result.PackageId].Add(result);
                    else
                        packages.Add(result.PackageId, new List<PackageBase> { result });
                }
            }
            return packages;
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
