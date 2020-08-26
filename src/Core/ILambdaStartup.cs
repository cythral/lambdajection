using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes a startup class for a lambda which configures services and logging used
    /// in the lambda's IoC container.
    /// </summary>
    public interface ILambdaStartup
    {
        /// <summary>
        /// Configures services to be injected into the lambda's IoC container.
        /// </summary>
        /// <param name="services">Collection of services that are injected into the lambda's IoC container.</param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Configures the logging to be used in the lambda's IoC container.
        /// </summary>
        /// <param name="logging">Object used to build a lambda logger.</param>
        void ConfigureLogging(ILoggingBuilder logging) { }
    }
}
