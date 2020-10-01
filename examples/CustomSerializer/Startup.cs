using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Examples.CustomSerializer
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<EmbeddedResourceReader>();
        }
    }
}
