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
    public sealed class LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : IAsyncDisposable
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <value>Provides services to the lambda.</value>
        public IServiceProvider ServiceProvider { get; internal set; } = default!;

        /// <value>Whether the lambda host should run its initialization services.</value>
        public bool RunInitializationServices { get; internal set; }

        /// <value>Function to suppress the finalization of an object.</value>
        public Action<object> SuppressFinalize { get; internal set; } = GC.SuppressFinalize;

        private TLambda? lambda;

        private IServiceScope? scope;


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
            Console.WriteLine(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Building host");
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
            Console.WriteLine(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Running initialization services");
            if (RunInitializationServices) await Initialize();

            Console.WriteLine(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Getting lambda from container");
            scope = ServiceProvider.CreateScope();
            lambda = scope.ServiceProvider.GetRequiredService<TLambda>();

            Console.WriteLine(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Running lambda handler");
            return await lambda.Handle(parameter, context);
        }

        private async Task Initialize()
        {
            var services = ServiceProvider.GetServices<ILambdaInitializationService>();

            var initializeTasks = services.Select(service => service.Initialize());
            await Task.WhenAll(initializeTasks);

            var disposeTasks = services.Select(MaybeDispose);
            await Task.WhenAll(disposeTasks);
        }

        /// <summary>
        /// Disposes the Lambda Host and its subresources asynchronously
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await MaybeDispose(scope);
            await MaybeDispose(lambda);

            lambda = null;
            scope = null;

            SuppressFinalize(this);
        }

        private static async Task MaybeDispose(object? obj)
        {
            if (obj is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
                return;
            }

            (obj as IDisposable)?.Dispose();
        }
    }
}
