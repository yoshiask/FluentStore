using System.Collections.Generic;

namespace Winstall.Models;

internal interface IPageProps
{
    
}

public sealed class IndexPageProps : IPageProps
{
    public IReadOnlyList<PopularApp> Popular { get; set; }
    public IReadOnlyList<App> Apps { get; set; }
    public IReadOnlyList<Pack> Recommended { get; set; }
}

public sealed class PacksPageProps : IPageProps
{
    public IReadOnlyList<Pack> Packs { get; set; }
}

public sealed class AppPageProps : IPageProps
{
    public App App { get; set; }
}

public sealed class PackPageProps : IPageProps
{
    public Pack Pack { get; set; }
    public Creator Creator { get; set; }
}