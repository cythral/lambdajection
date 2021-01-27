using System.Threading.Tasks;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.MissingLambdaInterface
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
