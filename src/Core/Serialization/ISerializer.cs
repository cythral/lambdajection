using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core.Serialization
{
    /// <summary>
    /// Describes an object that serializes and deserializes objects between formats.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize an object to a stream.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of the object to serialize.</typeparam>
        /// <param name="stream">The stream to write data to.</param>
        /// <param name="serializable">The object to serialize.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A stream containing the serialized representation of <paramref name="serializable" />.</returns>
        Task Serialize<TSerializableType>(Stream stream, TSerializableType serializable, CancellationToken cancellationToken);

        /// <summary>
        /// Deserialize a stream to an object of type <typeparamref name="TResultType" />.
        /// </summary>
        /// <typeparam name="TResultType">The type to deserialize to.</typeparam>
        /// <param name="input">The stream input to deserialize.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The deserialized representation of the given stream.</returns>
        ValueTask<TResultType?> Deserialize<TResultType>(Stream input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize a stream to an object of type <typeparamref name="TResultType" />.
        /// </summary>
        /// <typeparam name="TResultType">The type to deserialize to.</typeparam>
        /// <param name="input">The input to deserialize.</param>
        /// <returns>The deserialized representation of the given stream.</returns>
        TResultType? Deserialize<TResultType>(string input);
    }
}
