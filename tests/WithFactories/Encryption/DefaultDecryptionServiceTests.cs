using System.IO;
using System.Text;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

namespace Lambdajection.Encryption.Tests
{
    [Category("Unit")]
    public class DefaultDecryptionServiceTests
    {

        private static async Task<MemoryStream> CreateStreamFromString(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            var stream = new MemoryStream();
            await stream.WriteAsync(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        [Test]
        public async Task DecryptShouldDecryptTheCiphertext()
        {
            var kmsClient = Substitute.For<IAmazonKeyManagementService>();
            var value = "ZW5jcnlwdGVkIHZhcmlhYmxlCg==";
            var expectedValue = "decrypted variable";

            kmsClient
            .DecryptAsync(Arg.Any<DecryptRequest>())
            .Returns(new DecryptResponse
            {
                Plaintext = await CreateStreamFromString(expectedValue)
            });

            var facade = new DefaultDecryptionService(kmsClient);
            var response = await facade.Decrypt(value);

            response.Should().BeEquivalentTo(expectedValue);
            await kmsClient.Received().DecryptAsync(Arg.Is<DecryptRequest>(req => req.CiphertextBlob != null));
        }
    }
}
