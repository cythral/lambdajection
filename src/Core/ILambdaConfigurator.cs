using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public interface ILambdaConfigurator
    {
        void ConfigureOptions(IConfiguration configuration, IServiceCollection services);
        void ConfigureAwsServices(IServiceCollection services);
    }
}
