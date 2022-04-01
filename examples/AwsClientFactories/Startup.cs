using Amazon.S3;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Examples.AwsClientFactories
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
            services.UseAwsService<IAmazonS3>();
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Lambdajection", LogLevel.Information);
        }
    }
}
