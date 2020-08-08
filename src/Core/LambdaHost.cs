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
        public IServiceProvider ServiceProvider { get; internal set; } = null!;

        public LambdaHost() : this(LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>.Build)
        {
        }

        public LambdaHost(Action<LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>> build)
        {
            build(this);
        }

        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TLambda>();
                return await service.Handle(parameter, context);
            }
        }
    }
}