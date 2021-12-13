using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.Enums
{
    public class ImageTypeStringConverter : JsonConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageTypeStringConverter"/> class.
        /// </summary>
        public ImageTypeStringConverter()
        {
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;
            string enumName = Enum.GetName(e.GetType(), value);

            if (enumName != null)
            {
                // enum value has no name so write number
                writer.WriteValue(value);
            }
            else
            {
                writer.WriteValue(enumName);
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        return Parse(reader.Value?.ToString());

                    case JsonToken.Integer:
                        return (ImageType)reader.Value;

                    case JsonToken.Null:
                    default:
                        return ImageType.Unspecified;
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{nameof(ImageType)}'.", ex);
            }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ImageType);
        }

        public static ImageType Parse(string enumText)
        {
            if (Enum.TryParse<ImageType>(enumText, true, out var imageType))
                return imageType;
            else
                return ImageType.Unspecified;
        }
    }
}
