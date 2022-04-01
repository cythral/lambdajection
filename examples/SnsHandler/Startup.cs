using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Examples.SnsHandler
{
    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpClient, DefaultHttpClient>();
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Lambdajection", LogLevel.Information);
        }
    }
}
