using System.Security.Cryptography;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Examples.CustomResource
{
    public class Startup : ILambdaStartup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RNGCryptoServiceProvider>();
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Lambdajection", LogLevel.Information);
        }
    }
}
