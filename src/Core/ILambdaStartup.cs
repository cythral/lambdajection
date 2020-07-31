using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public interface ILambdaStartup
    {
        IConfiguration Configuration { get; set; }
        void ConfigureServices(IServiceCollection services);
    }
}