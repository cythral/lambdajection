using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public interface ILambdaOptionsConfigurator
    {
        void ConfigureOptions(IConfiguration configuration, IServiceCollection services);
    }
}
