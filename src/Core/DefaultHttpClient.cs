using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core
{
    /// <inheritdoc />
    public class DefaultHttpClient : IHttpClient, IDisposable
    {
        private readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private HttpClient httpClient = new();

        private bool disposed;

        /// <inheritdoc />
        public async Task PutJson<TPayload>(
            Uri requestUri,
            TPayload payload,
            string? contentType = "application/json",
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jsonString = JsonSerializer.Serialize(payload, options);
            var content = new StringContent(jsonString);
            content.Headers.Remove("Content-Type");

            if (contentType != null)
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            var response = await httpClient.PutAsync(requestUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            httpClient.Dispose();
            httpClient = null!;

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
