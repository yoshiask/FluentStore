using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using FluentStore.SDK.Downloads;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace FluentStore.SDK.Plugins.NuGet;

public class AbstractStoragePackageSearchResourceV3 : PackageSearchResource
{
    private readonly IEnumerable<Uri> _endpoints;
    private V3SearchResults _index;

    public AbstractStoragePackageSearchResourceV3(IEnumerable<Uri> endpoints)
    {
        _endpoints = endpoints;
    }

    public override async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string searchTerm, SearchFilter filters,
        int skip, int take, ILogger log, CancellationToken token = default)
    {
        Guard.IsGreaterThanOrEqualTo(take, 0);

        bool PackageFilter(PackageSearchMetadata p) =>
            p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm) || p.Tags.Contains(searchTerm);

        var index = await GetIndexAsync(token);
        return index.Data.Where(PackageFilter).Skip(skip).Take(take);
    }

    private async Task<V3SearchResults> GetIndexAsync(CancellationToken token)
    {
        if (_index is null)
        {
            var serializer = JsonSerializer.Create(JsonExtensions.ObjectSerializationSettings);
            serializer.Converters.Add(new V3SearchResultsConverter());

            _index = new();
            foreach (var endpoint in _endpoints)
            {
                var file = AbstractStorageHelper.GetFileFromUrl(endpoint.ToString());

                await using var stream = await file.SafeOpenStreamAsync(token: token);
                using var streamReader = new StreamReader(stream);
                await using var jsonReader = new JsonTextReader(streamReader);

                var index = serializer.Deserialize<V3SearchResults>(jsonReader);
                _index.TotalHits += index.TotalHits;
                _index.Data.AddRange(index.Data);
            }
        }

        return _index;
    }
}

internal class V3SearchResults
{
    [JsonProperty("totalHits")]
    public long TotalHits { get; set; }

    [JsonProperty("data")]
    public List<PackageSearchMetadata> Data { get; private set; } = new();
}

internal class V3SearchResultsConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(V3SearchResults);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (objectType != typeof(V3SearchResults))
        {
            throw new InvalidOperationException();
        }

        if (reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonException("Expected StartObject, found " + reader.TokenType);
        }

        var searchResults = new V3SearchResults();

        var finished = false;

        while (!finished)
        {
            reader.Read();

            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    switch ((string)reader.Value)
                    {
                        case "totalHits":
                            if (long.TryParse(reader.ReadAsString(), out var totalHits))
                            {
                                searchResults.TotalHits = totalHits;
                            }
                            else
                            {
                                throw new JsonException("totalHits should be a long integer");
                            }

                            break;

                        case "data":
                            reader.Read();

                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                throw new JsonException("data should be an array");
                            }

                            reader.Read();

                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                var searchResult = serializer.Deserialize<PackageSearchMetadata>(reader);
                                searchResults.Data.Add(searchResult);
                                reader.Read();
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }

                    break;

                case JsonToken.EndObject:
                    finished = true;
                    break;

                default:
                    throw new JsonException();
            }
        }

        return searchResults;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}