using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.ConfigFactory
{
    [Lambda(typeof(Startup), ConfigFactory = typeof(JsonConfigFactory))]
    public partial class Handler
    {
        private readonly IConfiguration config;

        public Handler(IConfiguration config)
        {
            this.config = config;
        }

        public Task<string> Handle(string _, CancellationToken cancellationToken = default)
        {
            var value = config.GetValue<string>("TestKey");
            return Task.FromResult(value);
        }
    }
}
