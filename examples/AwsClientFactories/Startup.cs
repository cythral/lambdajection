using Amazon.S3;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Examples.AwsClientFactories
{
    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; set; } = null!;

        public void ConfigureServices(IServiceCollection services)
        {
            services.UseAwsService<IAmazonS3>();
        }
    }
}
