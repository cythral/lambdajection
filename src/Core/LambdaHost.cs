using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    public class LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaOptionsConfigurator>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
        where TLambdaOptionsConfigurator : ILambdaOptionsConfigurator, new()
    {
        public IServiceProvider ServiceProvider { get; internal set; } = null!;

        public LambdaHost() : this(LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaOptionsConfigurator>.Build)
        {
        }

        public LambdaHost(Action<LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaOptionsConfigurator>> build)
        {
            build(this);
        }

        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            using var scope = ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TLambda>();
            return await service.Handle(parameter, context);
        }
    }
}
