using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Lambdajection.CompilationTests.IamPermissions
{
    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Utility>();

            var s3Substitute = Substitute.For<IAmazonS3>();
            services.AddSingleton(s3Substitute);
        }
    }
}
