using System.Text.Json;

namespace Lambdajection.Core.Serialization
{
    /// <summary>
    /// Wrapper around JsonSerializerOptions to allow a default set of options to be used
    /// in case the user does not specify one.
    /// </summary>
    public class JsonSerializerSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerSettings" /> class.
        /// </summary>
        /// <param name="options">The json serializer options to use.</param>
        public JsonSerializerSettings(JsonSerializerOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Gets the json serializer options.
        /// </summary>
        public JsonSerializerOptions Options { get; }
    }
}
