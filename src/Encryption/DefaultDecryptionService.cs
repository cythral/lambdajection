using System;
using System.IO;
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

        /// <summary>
        /// Decrypts a value and returns it as a plaintext string.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <returns>The plaintext decrypted value.</returns>
        public virtual async Task<string> Decrypt(string ciphertext)
        {
            using var stream = new MemoryStream();
            var byteArray = Convert.FromBase64String(ciphertext);
            await stream.WriteAsync(byteArray);

            var request = new DecryptRequest { CiphertextBlob = stream };
            var response = await kmsClient.DecryptAsync(request);

            using var reader = new StreamReader(response.Plaintext);
            return await reader.ReadToEndAsync();
        }
    }
}
