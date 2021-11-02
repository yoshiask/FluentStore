using System.Collections.Generic;
using System.Linq;
using Windows.Security.Credentials;

namespace FluentStore.Services
{
    public class PasswordVaultService : IPasswordVaultService
    {
        /// <inheritdoc/>
        public void Add(string userName, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(CredentialBase.DEFAULT_RESOURCE, userName, password));
        }

        /// <inheritdoc/>
        public void Add(string userName, string password, string resource)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(resource, userName, password));
        }

        /// <inheritdoc/>
        public void Add(CredentialBase cred)
        {
            var vault = new PasswordVault();
            if (cred is Credential pCred)
            {
                vault.Add(pCred.ToPasswordCredential());
            }
            else
            {
                vault.Add(new PasswordCredential(cred.Resource, cred.UserName, cred.Password));
            }
        }

        /// <inheritdoc/>
        public IList<CredentialBase> FindAllByResource(string resource)
        {
            var vault = new PasswordVault();
            return vault.FindAllByResource(resource).Select(c => (CredentialBase)new Credential(c)).ToList();
        }

        /// <inheritdoc/>
        public IList<CredentialBase> FindAllByUserName(string userName)
        {
            var vault = new PasswordVault();
            return vault.FindAllByUserName(userName).Select(c => (CredentialBase)new Credential(c)).ToList();
        }

        /// <inheritdoc/>
        public void Remove(CredentialBase cred)
        {
            var vault = new PasswordVault();
            if (cred is Credential pCred)
            {
                vault.Remove(pCred.ToPasswordCredential());
            }
            else
            {
                vault.Remove(new PasswordCredential(cred.Resource, cred.UserName, cred.Password));
            }
        }

        /// <inheritdoc/>
        public CredentialBase Retreive(string resource, string userName)
        {
            var vault = new PasswordVault();
            return new Credential(vault.Retrieve(resource, userName));
        }

        /// <inheritdoc/>
        public IList<CredentialBase> RetreiveAll()
        {
            var vault = new PasswordVault();
            return vault.RetrieveAll().Select(c => (CredentialBase)new Credential(c)).ToList();
        }
    }

    public class Credential : CredentialBase
    {
        public Credential(PasswordCredential cred)
        {
            cred.RetrievePassword();

            UserName = cred.UserName;
            Password = cred.Password;
            Resource = cred.Resource;
        }

        public PasswordCredential ToPasswordCredential()
        {
            return new PasswordCredential(Resource, UserName, Password);
        }
    }
}
