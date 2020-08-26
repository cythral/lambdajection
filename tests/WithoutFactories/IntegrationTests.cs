using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Lambdajection.TestsWithoutFactories
{
    [Lambda(Startup = typeof(Startup))]
    public partial class TestLambda
    {
        private readonly IAwsFactory<IAmazonS3> s3Factory;

        public TestLambda(IAwsFactory<IAmazonS3> s3Factory)
        {
            this.s3Factory = s3Factory;
        }

        public Task<IAwsFactory<IAmazonS3>> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(s3Factory);
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

    public class IntegrationTests
    {
        [Test]
        public void RunningTheLambdaShouldResultInException()
        {
            Assert.That(async () => await TestLambda.Run("", null), Throws.Exception);
        }
    }
}

