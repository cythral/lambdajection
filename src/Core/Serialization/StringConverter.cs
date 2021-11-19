using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable SA1313

namespace Lambdajection.Core.Serialization
{
    /// <summary>
    /// Converts a JSON string to a type.
    /// </summary>
    public class StringConverter : JsonConverter<object>
    {
        private readonly Type typeToConvert;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter" /> class.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        public StringConverter(Type typeToConvert)
        {
            this.typeToConvert = typeToConvert;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return this.typeToConvert == typeToConvert;
        }

        /// <inheritdoc />
        /// <remarks>Cannot use the given typeToConvert because it will always be System.Object.</remarks>
        public override object? Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (stringValue == null)
            {
                return null;
            }

            var isJsonLike = stringValue.StartsWith('{') || stringValue.StartsWith('[');
            var deserializable = isJsonLike
                ? stringValue
                : $@"""{JsonEncodedText.Encode(stringValue)}""";

            return SystemTextJsonSerializer.Deserialize(deserializable, typeToConvert, options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            var stringValue = SystemTextJsonSerializer.Serialize(value!, typeToConvert, options);
            writer.WriteStringValue(stringValue);
        }
    }
}
