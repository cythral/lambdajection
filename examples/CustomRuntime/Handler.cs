using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomRuntime
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        public async Task<string> Handle(object request, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult("Hello World!");
        }
    }
}
