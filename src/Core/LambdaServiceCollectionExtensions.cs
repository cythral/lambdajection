using Amazon.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    /// <summary>
    /// IServiceCollection extensions for Lambdajection.
    /// </summary>
    public static class LambdaServiceCollectionExtensions
    {
#pragma warning disable IDE0060

        /// <summary>
        /// Adds an Aws Service to the IoC container.  If AWSSDK.SecurityToken is a dependency of your project, then an
        /// <see cref="IAwsFactory{TAmazonService}" /> is also added to your lambda's container.
        /// </summary>
        /// <param name="serviceCollection">Services to be injected into the lambda's container.</param>
        /// <typeparam name="TAwsService">The interface type of the Amazon Service Client to use.</typeparam>
        public static void UseAwsService<TAwsService>(this IServiceCollection serviceCollection)
            where TAwsService : IAmazonService
        {
            // logic handled by generator
        }

#pragma warning restore IDE0060
    }
}
