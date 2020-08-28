using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes a lambda configurator.  A lambda configurator sets up the options to use and any Amazon Service Clients.
    /// </summary>
    public interface ILambdaConfigurator
    {
        /// <summary>
        /// Configures options to use for the lambda.
        /// </summary>
        /// <param name="configuration">The configuration for the lambda.</param>
        /// <param name="services">Collection of services used by the lambda's container.</param>
        void ConfigureOptions(IConfiguration configuration, IServiceCollection services);

        /// <summary>
        /// Configures aws services to use for the lambda.
        /// </summary>
        /// <param name="services">Collection of services used by the lambda's container.</param>
        void ConfigureAwsServices(IServiceCollection services);
    }
}
