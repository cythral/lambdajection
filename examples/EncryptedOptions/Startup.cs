using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Examples.EncryptedOptions
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
            // You can add your own decryption service here.  The default decryption service uses KMS to decrypt values.
            // During testing, you can replace this value with a substitute:

            // var decryptionService = Substitute.For<IDecryptionService>();
            // services.AddSingleton(decryptionService);
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Lambdajection", LogLevel.Information);
        }
    }
}
