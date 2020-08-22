using Amazon.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public static class LambdaServiceCollectionExtensions
    {

#pragma warning disable IDE0060
        public static void UseAwsService<TAwsService>(this IServiceCollection serviceCollection) where TAwsService : IAmazonService
        {

        }
#pragma warning restore IDE0060
    }
}
