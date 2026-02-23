using System.Collections.Generic;
using System.Text.Json.Serialization;
using Scoop.Converters;

namespace Scoop.Responses;

[JsonConverter(typeof(StringOrArrayJsonConverter))]
public class StringOrArray : List<string>
{
    public StringOrArray() : base() { }

    public StringOrArray(IEnumerable<string> values) : base(values) { }

    public StringOrArray(string value) : base([value]) { }
}
