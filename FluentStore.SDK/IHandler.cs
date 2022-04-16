using FluentStore.SDK.Users;

namespace FluentStore.SDK
{
    public interface IHandler
    {
        /// <summary>
        /// The <see cref="PackageService"/> for the current application.
        /// </summary>
        public PackageService PkgSvc { get; internal set; }

        /// <summary>
        /// The <see cref="AccountService"/> for the current application.
        /// </summary>
        public AccountService AccSvc { get; internal set; }

        /// <summary>
        /// Sets the services.
        /// </summary>
        internal void SetServices(PackageService pkgSvc, AccountService accSvc)
        {
            PkgSvc = pkgSvc;
            AccSvc = accSvc;
        }

        /// <summary>
        /// Called after all plugins are loaded.
        /// </summary>
        internal void OnLoaded();
    }
}
