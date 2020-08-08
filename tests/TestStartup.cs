using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection
{
    public class TestStartup : ILambdaStartup
    {
        public IConfiguration Configuration { get; set; } = null!;

        public IServiceCollection Services { get; private set; } = null!;

        public ILoggingBuilder LoggingBuilder { get; private set; } = null!;

        public void ConfigureServices(IServiceCollection services)
        {
            Services = services;
        }

        public void ConfigureLogging(ILoggingBuilder builder)
        {
            LoggingBuilder = builder;
        }
    }
}