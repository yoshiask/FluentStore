namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Represents a collection that can be created, whose details and items are editable.
    /// </summary>
    public interface ICreatablePackageCollection : IPackageCollection, IEditablePackage, IEditablePackageCollection
    {
    }
}
