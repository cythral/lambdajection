using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Lambdajection.Encryption
{
    /// <summary>
    /// Default decryption service - uses KMS to decrypt values.
    /// </summary>
    public class DefaultDecryptionService : IDecryptionService
    {
        private readonly IAmazonKeyManagementService kmsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDecryptionService" /> class.
        /// </summary>
        /// <param name="kmsClient">The KMS Client to use when decrypting values.</param>
        public DefaultDecryptionService(IAmazonKeyManagementService kmsClient)
        {
            this.kmsClient = kmsClient;
        }

        /// <inheritdoc />
        public virtual async Task<string> Decrypt(string ciphertext, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var stream = new MemoryStream();
            var byteArray = Convert.FromBase64String(ciphertext);

            await stream.WriteAsync(byteArray, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var request = new DecryptRequest { CiphertextBlob = stream };
            var response = await kmsClient.DecryptAsync(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            using var reader = new StreamReader(response.Plaintext);
            return await reader.ReadToEndAsync();
        }
    }
}
