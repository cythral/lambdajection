using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Converter to serialize/deserialize the status field on
    /// Custom Resource Responses.
    /// </summary>
    public class CustomResourceResponseStatusConverter : JsonConverter<CustomResourceResponseStatus>
    {
        /// <summary>
        /// Reads a string and converts it to a CustomResourceResponseStatus.
        /// </summary>
        /// <param name="reader">The JSON reader to read values from.</param>
        /// <param name="typeToConvert">The type to convert to.</param>
        /// <param name="options">The JSON Serializer Options to use.</param>
        /// <returns>A CustomResourceResponseStatus value.</returns>
        public override CustomResourceResponseStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()?.ToLower();
            return value switch
            {
                "success" => CustomResourceResponseStatus.Success,
                "failed" => CustomResourceResponseStatus.Failed,
                _ => throw new NotSupportedException($"Unknown response status: {value}"),
            };
        }

        /// <summary>
        /// Writes a custom resource response status value to a string.
        /// </summary>
        /// <param name="writer">The JSON writer to use.</param>
        /// <param name="value">The custom resource response status value to convert to a string.</param>
        /// <param name="options">The JSON Serializer Options to use.</param>
        public override void Write(Utf8JsonWriter writer, CustomResourceResponseStatus value, JsonSerializerOptions options)
        {
            var stringValue = value.ToString().ToUpper();
            writer.WriteStringValue(stringValue);
        }
    }
}
