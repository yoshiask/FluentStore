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

        

        /// <summary>
        /// Attempts to get an authenticated handler with the given ID.
        /// </summary>
        /// <param name="id">The ID to look up.</param>
        /// <param name="handler">The registered handler.</param>
        /// <returns>
        /// <see langword="true"/> if a handler with the given ID is logged in.
        /// </returns>
        public bool TryGetAuthenticatedHandler(string id, [NotNullWhen(true)] out AccountHandlerBase handler)
        {
            try
            {
                handler = GetHandler(id);
                return handler.IsLoggedIn;
            }
            catch
            {
                handler = null;
                return false;
            }
        }
    }
}
