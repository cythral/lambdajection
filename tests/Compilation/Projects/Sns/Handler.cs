using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Sns;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.Sns
{
    [SnsEventHandler(typeof(Startup))]
    public partial class Handler
    {
        public Task<string> Handle(SnsMessage<CloudFormationStackEvent> request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Message.StackId);
        }
    }
}

