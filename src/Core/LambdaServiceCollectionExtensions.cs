using Amazon.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public static class LambdaServiceCollectionExtensions
    {
        public static void UseAwsService<TAwsService>(this IServiceCollection serviceCollection) where TAwsService : IAmazonService
        {

        }
    }
}
