using OwlCore.AbstractUI.Models;
using System;

namespace FluentStore.SDK.Users
{
    public abstract class Account : IEquatable<Account>
    {
        private AbstractUICollection _manageAccountForm;

        public string Id { get; set; }

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

        public bool Equals(Account other) => Id.Equals(other.Id);

        public override bool Equals(object obj) => obj is Account other && this.Equals(other);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
