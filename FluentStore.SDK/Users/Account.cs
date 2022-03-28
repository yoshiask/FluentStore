using System;

namespace FluentStore.SDK.Users
{
    public abstract class Account : IEquatable<Account>
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public bool Equals(Account other) => Id.Equals(other.Id);

        public override bool Equals(object obj) => obj is Account other && this.Equals(other);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
