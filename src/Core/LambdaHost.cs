using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.Core
{
    /// <summary>
    /// IoC container / host used for lambdas of type TLambda.
    /// </summary>
    /// <typeparam name="TLambda">The type of lambda to host.</typeparam>
    /// <typeparam name="TLambdaParameter">The type of the lambda's input parameter.</typeparam>
    /// <typeparam name="TLambdaOutput">The type of the lambda's return value.</typeparam>
    /// <typeparam name="TLambdaStartup">The type to use for lambda startup (sets up services).</typeparam>
    /// <typeparam name="TLambdaConfigurator">The type to use for the lambda configurator (sets up options and aws services).</typeparam>
    /// <typeparam name="TLambdaConfigFactory">The type to use for the lambda's config factory.</typeparam>
    public class LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <value>Provides services to the lambda.</value>
        public IServiceProvider ServiceProvider { get; internal set; } = null!;

        private bool initialized = false;


        /// <summary>
        /// Constructs a new Lambda Host / IoC Container with the default host builder function.
        /// </summary>
        public LambdaHost() : this(LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>.Build)
        {
        }

        /// <summary>
        /// Constructs a new Lambda Host / IoC Container with the given host builder function.
        /// </summary>
        /// <param name="build"></param>
        internal LambdaHost(Action<LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
        {
            build(this);
        }

        /// <summary>
        /// Runs the lambda.
        /// </summary>
        /// <param name="parameter">The input parameter to pass to the lambda.</param>
        /// <param name="context">The context object to pass to the lambda.</param>
        /// <returns>The return value of the lambda.</returns>
        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            if (!initialized) await Initialize();

            using var scope = ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TLambda>();
            return await service.Handle(parameter, context);
        }

        private async Task Initialize()
        {
            var tasks = ServiceProvider
                .GetServices<ILambdaInitializationService>()
                .Select(service => service.Initialize());

            await Task.WhenAll(tasks);
            initialized = true;
        }
    }
}
