using Microsoft.Extensions.Configuration;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes a configuration factory for a Lambda.
    /// </summary>
    public interface ILambdaConfigFactory
    {
        /// <summary>
        /// Creates a configuration object.
        /// </summary>
        /// <returns>The resulting configuration object.</returns>
        IConfigurationRoot Create();
    }
}