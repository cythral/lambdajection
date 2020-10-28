using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lambdajection.CompilationTests.NoHandleMethod
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly Options exampleOptions;

        public Handler(IOptions<Options> exampleOptions)
        {
            this.exampleOptions = exampleOptions.Value;
        }
    }
}
