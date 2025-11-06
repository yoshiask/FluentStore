using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Scoop.Responses;
using System.Linq;
using System.Collections.Generic;

namespace Scoop.Converters;

internal class LicenseJsonConverter : JsonConverter<LicenseInfo>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(string) || base.CanConvert(typeToConvert);

    public override LicenseInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is JsonTokenType.String)
        {
            var singleString = reader.GetString();
            if (Uri.TryCreate(singleString, UriKind.Absolute, out var licenseUri))
            {
                return new()
                {
                    Url = licenseUri
                };
            }

            return new()
            {
                MultiLicenses = ParseIdentifiers(singleString!)
            };
        }

        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException($"Expected the start of an object.");

        Dictionary<string, string> properties = [];

        while (reader.TokenType is not JsonTokenType.EndObject)
        {
            if (!reader.Read() || reader.TokenType is not JsonTokenType.PropertyName)
                break;
            var propertyName = reader.GetString();

            if (!reader.Read() || reader.TokenType is not JsonTokenType.String)
                throw new JsonException("Expected property value");
            var propertyValue = reader.GetString();

            properties[propertyName!] = propertyValue!;
        }

        LicenseInfo license = new();

        if (properties.TryGetValue("identifier", out var identifierStr))
            license.MultiLicenses = ParseIdentifiers(identifierStr);

        if (properties.TryGetValue("url", out var urlStr) && Uri.TryCreate(urlStr, UriKind.Absolute, out var url))
            license.Url = url;

        return license;
    }

    public override void Write(Utf8JsonWriter writer, LicenseInfo value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static List<List<string>> ParseIdentifiers(string value)
    {
        // Apps can be dual-licensed, and their different files within them can have different licenses.
        // Dual-licenses are split by '|', and internal licenses by ','.

        return value
            .Split('|')
            .Select(d => d.Split(',').ToList())
            .ToList();
    }
}
