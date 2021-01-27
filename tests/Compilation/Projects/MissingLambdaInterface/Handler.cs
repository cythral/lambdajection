using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.MissingLambdaInterface
{
    [MissingLambdaInterface(typeof(Startup))]
    public partial class Handler
    {
        public Handler()
        {
        }

        public Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request);
        }
    }
}
