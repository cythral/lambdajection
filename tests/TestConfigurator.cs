using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection
{
    public class TestConfigurator : ILambdaConfigurator
    {
        public IConfiguration Configuration { get; set; } = null!;

        public IServiceCollection ServicesSetByConfigureOptions { get; private set; } = null!;

        public IServiceCollection ServicesSetByConfigureAwsServices { get; private set; } = null!;



        public void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            Configuration = configuration;
            ServicesSetByConfigureOptions = services;
        }

        public void ConfigureAwsServices(IServiceCollection services)
        {
            ServicesSetByConfigureAwsServices = services;
        }
    }
}
