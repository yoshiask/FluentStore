using System.Text;

namespace PublishTool.Support;

internal sealed partial class PathFragment : List<string>
{
    public static PathFragment operator /(PathFragment path, string segment) => [.. path, segment];

    public static PathFragment operator /(PathFragment left, PathFragment right) => [..left, ..right];

    public static string operator /(PathRoot root, PathFragment path)
    {
        StringBuilder sb = new(root.Value);

        var previousEndedWithSeparator = root.Value[^1] == root.Separator;

        foreach (var segment in path)
        {
            var currentStartsWithSeparator = segment[0] == root.Separator;
            if (!currentStartsWithSeparator && !previousEndedWithSeparator)
                sb.Append(root.Separator);

            sb.Append(segment);
            previousEndedWithSeparator = segment[^1] == root.Separator;
        }
        
        return sb.ToString();
    }

    public static implicit operator PathFragment(string segment) => [segment];
}

internal sealed class PathRoot(string root, char separator)
{
    public string Value { get; } = root;

    public char Separator { get; } = separator;

    public override string ToString() => Value;

    public static string operator /(PathRoot root, string segment)
    {
        StringBuilder sb = new(root.Value);

        if (root.Value[^1] != root.Separator && segment[0] != root.Separator)
            sb.Append(root.Separator);
        sb.Append(segment);

        return sb.ToString();
    }

    public static implicit operator PathRoot(string root) => new(root, Path.DirectorySeparatorChar);

    public static implicit operator PathRoot(Uri root) => new(root.ToString()[..^1], '/');
}
