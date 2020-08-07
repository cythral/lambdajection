using System;
using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    public class LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
    {
        private IServiceProvider serviceProvider;

        public LambdaHost(IServiceProvider? serviceProvider = null)
        {
            if (serviceProvider == null)
            {
                var builder = new LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>();
                serviceProvider = builder.GetOrBuildServiceProvider();
            }

            this.serviceProvider = serviceProvider;
        }

        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TLambda>();
                return await service.Handle(parameter, context);
            }
        }
    }
}