using System.Collections.Generic;

namespace FluentStore.Services
{
    public interface IPasswordVaultService
    {

        /// <summary>
        /// Adds a credential to the locker.
        /// </summary>
        void Add(string userName, string password);

        /// <summary>
        /// Adds a credential to the locker.
        /// </summary>
        void Add(string userName, string password, string resource);

        /// <summary>
        /// Adds a credential to the locker.
        /// </summary>
        void Add(CredentialBase cred);

        /// <summary>
        /// Searches the locker for credentials matching the user name specified.
        /// </summary>
        IList<CredentialBase> FindAllByUserName(string userName);

        /// <summary>
        /// Searches the locker for credentials matching the resource specified.
        /// </summary>
        IList<CredentialBase> FindAllByResource(string resource);

        /// <summary>
        /// Removes a credential from the locker.
        /// </summary
        void Remove(CredentialBase cred);

        /// <summary>
        /// Reads a credential from the locker.
        /// </summary>
        CredentialBase Retreive(string resource, string userName);

        /// <summary>
        /// Reads all credentials in the locker.
        /// </summary>
        IList<CredentialBase> RetreiveAll();
    }

    public class CredentialBase
    {
        public const string DEFAULT_RESOURCE = "default";

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Resource { get; set; }

        public CredentialBase() { }

        public CredentialBase(string userName, string password)
        {
            UserName = userName;
            Password = password;
            Resource = DEFAULT_RESOURCE;
        }

        public CredentialBase(string userName, string password, string resource)
        {
            UserName = userName;
            Password = password;
            Resource = resource;
        }
    }
}
