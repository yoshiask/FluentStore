using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractUI.Models;
using System;

namespace FluentStore.SDK.Users
{
    public abstract class Account : IEquatable<Account>
    {
        private AbstractUICollection _manageAccountForm;

        public Urn Urn { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string Id => Urn.GetContent<NamespaceSpecificString>().UnEscapedValue;

        /// <inheritdoc cref="CreateManageAccountForm"/>
        public AbstractUICollection ManageAccountForm
        {
            get
            {
                if (_manageAccountForm == null)
                    _manageAccountForm = CreateManageAccountForm();
                return _manageAccountForm;
            }
            set => _manageAccountForm = value;
        }

        /// <summary>
        /// Gets the <see cref="AbstractUICollection"/> that represents a form to manage this account.
        /// </summary>
        protected abstract AbstractUICollection CreateManageAccountForm();

        public bool Equals(Account other) => Urn.Equals(other.Urn);

        public override bool Equals(object obj) => obj is Account other && this.Equals(other);

        public override int GetHashCode() => Urn.GetHashCode();
    }
}
