using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scoop.Responses;

namespace Scoop.Converters;

internal class StringOrArrayJsonConverter : JsonConverter<StringOrArray>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(string) || base.CanConvert(typeToConvert);

    public override StringOrArray? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return [];

        if (reader.TokenType is JsonTokenType.String)
            return [reader.GetString()];

        if (reader.TokenType is not JsonTokenType.StartArray)
            throw new JsonException($"Expected the start of a string array.");

        StringOrArray list = [];

        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
        {
            var str = JsonSerializer.Deserialize<string>(ref reader, options)
                ?? throw new JsonException($"Unexpected null value could not be converted to List<string>.");

            list.Add(str);
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, StringOrArray value, JsonSerializerOptions options)
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
