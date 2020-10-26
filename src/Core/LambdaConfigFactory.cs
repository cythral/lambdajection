using Microsoft.Extensions.Configuration;

namespace Lambdajection.Core
{
    /// <summary>
    /// The default lambda config factory.
    /// </summary>
    public class LambdaConfigFactory : ILambdaConfigFactory
    {
        /// <summary>
        /// Creates the default configuration object.
        /// </summary>
        /// <returns>The default configuration object.</returns>
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        }
    }
}
