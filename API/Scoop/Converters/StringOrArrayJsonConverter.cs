using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scoop.Converters;

internal class StringOrArrayJsonConverter : JsonConverter<List<string>>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(string) || base.CanConvert(typeToConvert);

    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return [];

        var singleStr = reader.GetString();
        if (singleStr is not null)
            return [singleStr];

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected the start of a string array.");

        List<string> list = [];

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var str = JsonSerializer.Deserialize<string>(ref reader, options)
                ?? throw new JsonException($"Unexpected null value could not be converted to List<string>.");

            list.Add(str);
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            writer.WriteStringValue(value[0]);
            return;
        }

        writer.WriteStartArray();

        foreach (var str in value)
            writer.WriteStringValue(str);

        writer.WriteEndArray();
    }
}
