using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

using Lambdajection.Attributes;

using Microsoft.Extensions.Options;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Lambdajection.Examples.EncryptedOptions
{
    [Lambda(Startup = typeof(Startup))]
    public partial class Handler
    {
        private readonly Options options;

        public Handler(IOptions<Options> options)
        {
            this.options = options.Value;
        }

        public Task<string> Handle(object request, ILambdaContext context)
        {
            return Task.FromResult(options.EncryptedValue); // Despite the name, this will be returned to you unencrypted.
        }
    }
}
