using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Lambdajection.Encryption.Tests
{
    [Category("Unit")]
    public class DefaultDecryptionServiceTests
    {
        [Test, Auto]
        public async Task DecryptShouldDecryptTheCiphertext(
            [Frozen, Substitute] IAmazonKeyManagementService kmsClient,
            [Target] DefaultDecryptionService service
        )
        {
            var value = "ZW5jcnlwdGVkIHZhcmlhYmxlCg==";
            var expectedValue = "decrypted variable";

            kmsClient
            .DecryptAsync(Any<DecryptRequest>())
            .Returns(new DecryptResponse
            {
                Plaintext = await CreateStreamFromString(expectedValue),
            });

            var cancellationToken = new CancellationToken(false);
            var response = await service.Decrypt(value);

            response.Should().BeEquivalentTo(expectedValue);
            await kmsClient.Received().DecryptAsync(Is<DecryptRequest>(req => req.CiphertextBlob != null), Is(cancellationToken));
        }

        private static async Task<MemoryStream> CreateStreamFromString(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            var stream = new MemoryStream();
            await stream.WriteAsync(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
