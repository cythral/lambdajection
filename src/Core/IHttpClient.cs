using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core
{
    /// <summary>
    /// An HTTP Client.
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Sends a PUT request with the specified payload and content type.
        /// </summary>
        /// <param name="requestUri">The URI to send a request to.</param>
        /// <param name="payload">The JSON body to send in the request.</param>
        /// <param name="contentType">The content type to use in the request.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <typeparam name="TPayload">The type of payload.</typeparam>
        /// <returns>Nothing.</returns>
        Task PutJson<TPayload>(Uri requestUri, TPayload payload, string? contentType = "application/json", CancellationToken cancellationToken = default);
    }
}
