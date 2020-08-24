using System;
using System.IO;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Lambdajection.Encryption
{
    public class DefaultDecryptionService : IDecryptionService
    {
        private readonly IAmazonKeyManagementService kmsClient;

        public DefaultDecryptionService(IAmazonKeyManagementService kmsClient)
        {
            this.kmsClient = kmsClient;
        }

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