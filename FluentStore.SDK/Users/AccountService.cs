using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK.Users
{
    public class AccountService
    {
        private IReadOnlySet<AccountHandlerBase> _AccountHandlers;
        /// <summary>
        /// A cache of all valid account handlers. The key is the name of the <see cref="Type"/>,
        /// and the value is an instance of the <see cref="AccountHandlerBase"/>
        /// </summary>
        public IReadOnlySet<AccountHandlerBase> AccountHandlers
        {
            get => _AccountHandlers;
            set
            {
                if (_AccountHandlers != null)
                    CommunityToolkit.Diagnostics.ThrowHelper.ThrowInvalidOperationException($"Cannot set {nameof(AccountHandlers)} more than once.");
                _AccountHandlers = value;
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
                    foreach (AccountHandlerBase handler in AccountHandlers)
                    {
                        foreach (string ns in handler.HandledNamespaces)
                            _NamespaceRegistry.Add(ns, i);
                        i++;
                    }
                }

                return _NamespaceRegistry;
            }
        }

        public async Task TrySlientSignInAsync()
        {
            foreach (var handler in AccountHandlers)
            {
                //if (!handler.IsEnabled) continue;

                await handler.TrySilentSignInAsync();
            }
        }

        public async Task RouteAuthActivation(Url url)
        {
            string ns = url.PathSegments.Last();
            var handler = GetHandlerForNamespace(ns);
            await handler.HandleAuthActivation(url);
        }

        /// <summary>
        /// Gets the handler registered for the given namespace.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="InvalidOperationException"/>
        public AccountHandlerBase GetHandlerForNamespace(string ns)
        {
            if (NamespaceRegistry.TryGetValue(ns, out var handlerIdx))
            {
                var handler = AccountHandlers.ElementAt(handlerIdx);
                return handler;
            }
            else
            {
                throw new NotSupportedException($"No package handler is registered for the namespace \"{ns}\".");
            }
        }
    }
}
