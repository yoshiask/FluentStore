using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using NuGet.Versioning;

namespace FluentStore.SDK.Plugins;

public class ServiceIndexIpnsResourceV3 : ServiceIndexResourceV3
{
    private static readonly IReadOnlyList<ServiceIndexEntry> _emptyEntries = new List<ServiceIndexEntry>();
    private readonly IDictionary<string, List<ServiceIndexEntry>> _index;

    public ServiceIndexIpnsResourceV3(JObject index, DateTime requestTime) : base(index, requestTime)
    {
        _index = MakeLookup(index);
    }

    public override IReadOnlyList<ServiceIndexEntry> Entries =>
        _index.SelectMany(e => e.Value).ToList();

    public override IReadOnlyList<ServiceIndexEntry> GetServiceEntries(NuGetVersion clientVersion,
        params string[] orderedTypes)
    {
        if (clientVersion == null)
        {
            throw new ArgumentNullException(nameof(clientVersion));
        }

        foreach (var type in orderedTypes)
        {
            if (!_index.TryGetValue(type, out var entries))
                continue;
            
            var compatible = GetBestVersionMatchForType(clientVersion, entries);
            if (compatible.Count > 0)
                return compatible;
        }

        return _emptyEntries;
    }

    private IReadOnlyList<ServiceIndexEntry> GetBestVersionMatchForType(NuGetVersion clientVersion,
        List<ServiceIndexEntry> entries)
    {
        var bestMatch = entries.FirstOrDefault(e => e.ClientVersion <= clientVersion);

        return bestMatch == null
            // No compatible version
            ? _emptyEntries
            // Find all entries with the same version.
            : entries.Where(e => e.ClientVersion == bestMatch.ClientVersion).ToList();
    }

    private static IDictionary<string, List<ServiceIndexEntry>> MakeLookup(JObject index)
    {
        var result = new Dictionary<string, List<ServiceIndexEntry>>(StringComparer.Ordinal);

        if (index.TryGetValue("resources", out var resources))
        {
            foreach (var resource in resources)
            {
                var id = GetValues(resource["@id"]).SingleOrDefault();
                
                // Skip missing @ids
                if (string.IsNullOrEmpty(id))
                    continue;
                
                // Convert gateway URLs to IPNS URIs
                var schemeIdx = id.IndexOf("://", StringComparison.Ordinal);
                Guard.IsGreaterThanOrEqualTo(schemeIdx, 0);
                id = "ipns" + id[schemeIdx..];

                // Skip invalid or missing @ids
                if (!Uri.TryCreate(id, UriKind.Absolute, out var uri))
                    continue;

                var types = GetValues(resource["@type"]).ToArray();
                var clientVersionToken = resource["clientVersion"];

                var clientVersions = new List<SemanticVersion>();

                if (clientVersionToken == null)
                {
                    // For non-versioned services assume all clients are compatible
                    clientVersions.Add(new SemanticVersion(0, 0, 0));
                }
                else
                {
                    // Parse supported versions
                    foreach (var versionString in GetValues(clientVersionToken))
                        if (SemanticVersion.TryParse(versionString, out var version))
                            clientVersions.Add(version);
                }

                // Create service entries
                foreach (var type in types)
                {
                    foreach (var version in clientVersions)
                    {
                        if (!result.TryGetValue(type, out var entries))
                        {
                            entries = new List<ServiceIndexEntry>();
                            result.Add(type, entries);
                        }

                        entries.Add(new ServiceIndexEntry(uri, type, version));
                    }
                }
            }
        }

        // Order versions desc for faster lookup later.
        foreach (var type in result.Keys.ToArray())
        {
            result[type] = result[type].OrderByDescending(e => e.ClientVersion).ToList();
        }

        return result;
    }

    /// <summary>
    /// Read string values from an array or string.
    /// Returns an empty enumerable if the value is null.
    /// </summary>
    private static IEnumerable<string> GetValues(JToken token)
    {
        if (token?.Type == JTokenType.Array)
        {
            foreach (var entry in token)
            {
                if (entry.Type == JTokenType.String)
                {
                    yield return entry.ToObject<string>();
                }
            }
        }
        else if (token?.Type == JTokenType.String)
        {
            yield return token.ToObject<string>();
        }
    }
}