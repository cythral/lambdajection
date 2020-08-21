using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection
{
    public class TestOptionsConfigurator : ILambdaOptionsConfigurator
    {
        public IConfiguration Configuration { get; set; } = null!;

        public IServiceCollection Services { get; private set; } = null!;

        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            Configuration = configuration;
            Services = services;
        }
    }
}