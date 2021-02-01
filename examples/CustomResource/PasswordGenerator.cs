using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.CustomResource;

using Microsoft.Extensions.Options;

namespace Lambdajection.Examples.CustomResource
{
    [CustomResourceProvider(typeof(Startup))]
    public partial class PasswordGenerator
    {
        private readonly Options options;
        private readonly RNGCryptoServiceProvider cryptoServiceProvider;

        public PasswordGenerator(
            RNGCryptoServiceProvider cryptoServiceProvider,
            IOptions<Options> options
        )
        {
            this.options = options.Value;
            this.cryptoServiceProvider = cryptoServiceProvider;
        }

        private string GeneratePassword(uint length)
        {
            var bytes = new byte[length];
            cryptoServiceProvider.GetBytes(bytes);

            var chars = bytes.Select(@byte => $"{@byte:X2}");
            return string.Join(string.Empty, chars);
        }

        public Task<Response> Create(CustomResourceRequest<Request> request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var password = GeneratePassword(request.ResourceProperties?.Length ?? options.DefaultLength);
            var response = new Response(password);
            return Task.FromResult(response);
        }

        public Task<Response> Update(CustomResourceRequest<Request> request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var password = GeneratePassword(request.ResourceProperties?.Length ?? options.DefaultLength);
            var response = new Response(password);
            return Task.FromResult(response);
        }

        public Task<Response> Delete(CustomResourceRequest<Request> request, CancellationToken cancellationToken = default)
        {
            var response = new Response(string.Empty);
            return Task.FromResult(response);
        }
    }
}
