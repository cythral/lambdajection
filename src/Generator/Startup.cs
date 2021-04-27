using Lambdajection.Framework.Utils;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Generator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<UsingsGenerator>();
            services.AddSingleton<TypeUtils>();
            services.AddSingleton<UnitGenerator>();
            services.AddSingleton<IamAccessAnalyzer>();
        }
    }
}
