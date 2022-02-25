using Garfoot.Utilities.FluentUrn;
using System;

namespace FluentStore.SDK.Users
{
    public abstract class Account : IEquatable<Account>
    {
        public Urn Urn { get; set; }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public bool Equals(Account other) => Urn.Equals(other.Urn);

        public override bool Equals(object obj) => obj is Account other ? this.Equals(other) : false;

        public override int GetHashCode() => Urn.GetHashCode();
    }
}
