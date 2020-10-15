using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Examples.CustomRuntime
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject services into the Lambda's container here
        }
    }
}
