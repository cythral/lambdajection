
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    public interface ILambdaStartup
    {
        IConfiguration Configuration { get; set; }

        void ConfigureServices(IServiceCollection services);
        void ConfigureLogging(ILoggingBuilder logging) { }
    }
}