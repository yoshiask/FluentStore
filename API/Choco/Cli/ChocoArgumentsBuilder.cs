using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NuGet.Versioning;

namespace Chocolatey.Cli;

internal class ChocoArgumentsBuilder
{
    public const string CHOCO_EXE = "choco";

    public NuGetVersion? Version { get; set; }
    public bool Yes { get; set; }
    public bool LimitOutput { get; set; }
    public bool NoOp { get; set; }

    public void Build(List<string> args) => args.AddRange(Build());

    [Pure]
    public IEnumerable<string> Build()
    {
        if (Version is not null)
            yield return $"--version=\"'{Version}'\"";

        if (Yes)
            yield return "-y";

        if (LimitOutput)
            yield return "--limit-output";

        if (NoOp)
            yield return "--noop";
    }
}