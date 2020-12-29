using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.CustomRunnerMethod
{
    [Lambda(typeof(Startup), RunnerMethod = "Process")]
    public partial class Handler
    {
        public Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("Success");
        }
    }
}

