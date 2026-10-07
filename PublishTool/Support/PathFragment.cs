namespace PublishTool.Support;

internal sealed partial class PathFragment : List<string>
{
    public static PathFragment operator /(PathFragment path, string segment) => [.. path, segment];

    public static string operator /(PathRoot root, PathFragment path) => root.Value + root.Separator + string.Join(root.Separator, path);

    public static implicit operator PathFragment(string segment) => [segment];
}

internal sealed class PathRoot(string root, char separator)
{
    public string Value { get; } = root;

    public char Separator { get; } = separator;

    public override string ToString() => Value;

    public static string operator /(PathRoot root, string segment) => root.Value + root.Separator + segment;

    public static implicit operator PathRoot(string root) => new(root, Path.DirectorySeparatorChar);

    public static implicit operator PathRoot(Uri root) => new(root.ToString()[..^1], '/');
}
