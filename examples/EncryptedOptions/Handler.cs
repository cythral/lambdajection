using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

using Microsoft.Extensions.Options;

namespace Lambdajection.Examples.EncryptedOptions
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly Options options;

        public Handler(IOptions<Options> options)
        {
            this.options = options.Value;
        }

        public Task<string> Handle(object request, ILambdaContext context)
        {
            return Task.FromResult(options.EncryptedValue); // despite the name, this value will have already been decrypted for you.
        }
    }
}
