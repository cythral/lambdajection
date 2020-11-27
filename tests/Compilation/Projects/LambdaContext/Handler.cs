using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.LambdaContext
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly ILambdaContext context;

        public Handler(ILambdaContext context)
        {
            this.context = context;
        }

        public Task<ILambdaContext> Handle(string request)
        {
            return Task.FromResult(context);
        }
    }
}

