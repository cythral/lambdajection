using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.TestsWithoutFactories
{
    [Lambda(typeof(Startup))]
    public partial class TestLambda
    {
        private readonly S3Utility utility;

        public TestLambda(S3Utility utility)
        {
            this.utility = utility;
        }

        public Task<IAwsFactory<IAmazonS3>> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(utility.Factory);
        }
    }

    public class S3Utility
    {
        public IAwsFactory<IAmazonS3> Factory { get; set; }

        public S3Utility(IAwsFactory<IAmazonS3> s3Factory)
        {
            this.Factory = s3Factory;
        }
    }

    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection collection)
        {
            collection.UseAwsService<IAmazonS3>();
        }
    }
}

