using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Encryption;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Lambdajection.CompilationTests.Configuration
{
    public class Startup : ILambdaStartup
    {
        public static IDecryptionService DecryptionService { get; } = Substitute.For<IDecryptionService>();

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            DecryptionService.Decrypt(Arg.Any<string>()).Returns(x => "[decrypted] " + x.ArgAt<string>(0));

            services.AddSingleton(DecryptionService);
        }
    }
}