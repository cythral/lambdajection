using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Examples.CustomRuntime
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject services into the Lambda's container here
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Lambdajection", LogLevel.Information);
        }
    }
}
