using Flurl;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <exception cref="NotSupportedException">When no package handler is registered for the namespace.</exception>
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

        /// <summary>
        /// Gets the handler of type <typeparamref name="THandler"/> registered for the given namespace.
        /// </summary>
        /// <exception cref="NotSupportedException">When no package handler is registered for the namespace.</exception>
        /// <exception cref="InvalidOperationException"/>
        public THandler GetHandlerForNamespace<THandler>(string ns) where THandler : AccountHandlerBase
        {
            if (NamespaceRegistry.TryGetValue(ns, out var handlerIdx))
            {
                var handler = AccountHandlers.ElementAt(handlerIdx);
                if (handler is THandler tHandler)
                    return tHandler;
            }

            throw new NotSupportedException($"No package handler is registered for the namespace \"{ns}\".");
        }

        /// <summary>
        /// Gets the first registered handler of type <typeparamref name="THandler"/>
        /// </summary>
        /// <exception cref="NotSupportedException">When no package handler is registered for the namespace.</exception>
        /// <exception cref="InvalidOperationException"/>
        public THandler GetHandlerForNamespace<THandler>() where THandler : AccountHandlerBase
        {
            var tHandler = AccountHandlers.OfType<THandler>().FirstOrDefault();
            if (tHandler != null)
                return tHandler;

            throw new NotSupportedException($"No package handler is registered for the type \"{typeof(THandler)}\".");
        }

        /// <summary>
        /// Attempts to get an authenticated handler registered for the namespace.
        /// </summary>
        /// <param name="ns">The namespace to look up.</param>
        /// <param name="handler">The registered handler.</param>
        /// <returns>
        /// <see langword="true"/> if the namespace is handled, and the associated
        /// handler is logged in.
        /// </returns>
        public bool TryGetAuthenticatedHandlerForNamespace(string ns, [NotNullWhen(true)] out AccountHandlerBase handler)
        {
            if (NamespaceRegistry.TryGetValue(ns, out var handlerIdx))
            {
                handler = AccountHandlers.ElementAt(handlerIdx);
                return handler.IsLoggedIn;
            }
            else
            {
                handler = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to get an authenticated handler of the specified type.
        /// </summary>
        /// <typeparam name="THandler">The handler type to get.</typeparam>
        /// <param name="handler">The registered handler.</param>
        /// <returns>
        /// <see langword="true"/> if a handler with the specified type is
        /// registered and logged in.
        /// </returns>
        public bool TryGetAuthenticatedHandler<THandler>([NotNullWhen(true)] out THandler handler) where THandler : AccountHandlerBase
        {
            foreach (AccountHandlerBase genericHandler in AccountHandlers)
            {
                if (genericHandler.IsLoggedIn && genericHandler is THandler tHandler)
                {
                    handler = tHandler;
                    return true;
                }
            }

            handler = null;
            return false;
        }
    }
}
