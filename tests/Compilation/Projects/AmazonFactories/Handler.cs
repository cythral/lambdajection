using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.AmazonFactories
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly S3Utility utility;

        public Handler(S3Utility utility)
        {
            this.utility = utility;
        }

        public Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("ok");
        }
    }
}

