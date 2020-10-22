using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.ConfigFactory
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // do nothing
        }
    }
}