using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chocolatey.Models;
using NuGet.Versioning;

namespace Chocolatey;

public interface IChocoSearchService
{
    Task<IReadOnlyList<Package>> SearchAsync(string query, string targetFramework = "", bool includePrerelease = false, int top = 30, int skip = 0);

    Task<Package> GetPackageAsync(string id, NuGetVersion version);

    Task<string> GetPackagePropertyAsync(string id, NuGetVersion version, string propertyName);
}

public static class ChocoSearchServiceExtensions
{
    public static async Task<T> GetPackagePropertyAsync<T>(this IChocoSearchService service, string id,
        NuGetVersion version, string propertyName, Parse<T> parse)
    {
        string str = await service.GetPackagePropertyAsync(id, version, propertyName);
        return parse(str);
    }

    public static async Task<DateTimeOffset> GetPackageDatePropertyAsync(this IChocoSearchService service, string id,
        NuGetVersion version, string propertyName)
    {
        return await service.GetPackagePropertyAsync(id, version, propertyName, DateTimeOffset.Parse);
    }
    public static async Task<bool> GetPackageBooleanPropertyAsync(this IChocoSearchService service, string id,
        NuGetVersion version, string propertyName)
    {
        return await service.GetPackagePropertyAsync(id, version, propertyName, bool.Parse);
    }
    public static async Task<int> GetPackageInt32PropertyAsync(this IChocoSearchService service, string id,
        NuGetVersion version, string propertyName)
    {
        return await service.GetPackagePropertyAsync(id, version, propertyName, int.Parse);
    }
    public static async Task<long> GetPackageInt64PropertyAsync(this IChocoSearchService service, string id,
        NuGetVersion version, string propertyName)
    {
        return await service.GetPackagePropertyAsync(id, version, propertyName, long.Parse);
    }
}
