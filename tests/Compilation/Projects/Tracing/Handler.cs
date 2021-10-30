using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

namespace Lambdajection.CompilationTests.Tracing
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly IAWSXRayRecorder recorder;

        public Handler(
            IAWSXRayRecorder recorder
        )
        {
            this.recorder = recorder;
        }

        public Task<bool> Handle(string request, CancellationToken cancellationToken = default)
        {
            var client = new ConcreteAmazonClient();
            return Task.FromResult(client.IsTracingEnabled());
        }
    }
}
