
namespace FluentStore.SDK;

public interface IHasAccessibleDescription
{
    /// <summary>
    /// Gets a text description of this object that can be used
    /// for accessibility purposes, such as screen readers.
    /// </summary>
    string ToAccessibleDescription();
}

public static class AccessibleDescriptionExtensions
{
    public static string GetAccessibleDescription(this object obj)
    {
        return obj is IHasAccessibleDescription acc
            ? acc.ToAccessibleDescription()
            : obj?.ToString();
    }
}
