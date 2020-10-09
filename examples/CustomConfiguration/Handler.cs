using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

using Microsoft.Extensions.Options;

namespace Lambdajection.Examples.ConfigFactory
{
    [Lambda(typeof(Startup), ConfigFactory = typeof(ConfigFactory))]
    public partial class Handler
    {
        private readonly Config config;

        public Handler(IOptions<Config> config)
        {
            this.config = config.Value;
        }

        public Task<string> Handle(object request, ILambdaContext context)
        {
            return Task.FromResult(config.Foo);
        }
    }
}
