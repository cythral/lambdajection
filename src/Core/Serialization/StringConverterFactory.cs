using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lambdajection.Core.Serialization
{
    /// <summary>
    /// Converts a JSON string to a type.
    /// </summary>
    public class StringConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        /// <inheritdoc />
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new StringConverter(typeToConvert);
        }
    }
}
