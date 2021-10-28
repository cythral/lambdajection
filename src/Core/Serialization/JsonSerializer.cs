using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

namespace Lambdajection.Core.Serialization
{
    /// <summary>
    /// Serializer that serializes to/from JSON.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer" /> class.
        /// </summary>
        /// <param name="settings">Options to pass to the <see cref="System.Text.Json.JsonSerializer" />'s methods.</param>
        /// <param name="converters">Converters to add to the serializer options.</param>
        public JsonSerializer(
            JsonSerializerSettings settings,
            IEnumerable<JsonConverter> converters
        )
        {
            options = settings.Options;

            foreach (var converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        /// <inheritdoc />
        public async Task Serialize<TSerializableType>(Stream stream, TSerializableType serializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SystemTextJsonSerializer.SerializeAsync(stream, serializable, options, cancellationToken);
            stream.Position = 0;
        }

        /// <inheritdoc />
        public TResultType? Deserialize<TResultType>(string input)
        {
            return SystemTextJsonSerializer.Deserialize<TResultType>(input, options);
        }

        /// <inheritdoc />
        public ValueTask<TResultType?> Deserialize<TResultType>(Stream deserializable, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SystemTextJsonSerializer.DeserializeAsync<TResultType>(deserializable, options, cancellationToken);
        }
    }
}
