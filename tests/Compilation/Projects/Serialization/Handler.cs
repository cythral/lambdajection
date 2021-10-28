using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.Serialization
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        public Task<string> Handle(Request request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Id);
        }
    }
}

