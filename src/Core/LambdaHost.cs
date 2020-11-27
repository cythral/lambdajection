using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0060, CA1801

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
        private TLambda? lambda;

        private IServiceScope? scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        public LambdaHost()
            : this(LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>.Build)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        /// <param name="build">The builder action to run on this lambda.</param>
        internal LambdaHost(Action<LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
        {
            build(this);
        }

        /// <summary>
        /// Gets the service provider to use for the lambda.
        /// </summary>
        /// <value>Provides services to the lambda.</value>
        public IServiceProvider ServiceProvider { get; internal set; } = default!;

        /// <summary>
        /// Gets a value indicating whether to run initialization services on the lambda.
        /// </summary>
        /// <value>Whether the lambda host should run its initialization services.</value>
        public bool RunInitializationServices { get; internal set; }

        /// <summary>
        /// Gets the function used to suppress finalizers.
        /// </summary>
        /// <value>Function to suppress the finalization of an object.</value>
        public Action<object> SuppressFinalize { get; internal set; } = GC.SuppressFinalize;

        /// <summary>
        /// Gets the context object used for the current invocation.
        /// </summary>
        /// <value>Context object used for the current lambda invocation.</value>
        public ILambdaContext? Context { get; private set; }

        /// <summary>
        /// Runs the lambda.
        /// </summary>
        /// <param name="parameter">The input parameter to pass to the lambda.</param>
        /// <param name="context">The context object to pass to the lambda.</param>
        /// <returns>The return value of the lambda.</returns>
        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            if (RunInitializationServices)
            {
                await Initialize();
            }

            scope = ServiceProvider.CreateScope();

            var provider = scope.ServiceProvider;
            var scopeContext = provider.GetRequiredService<LambdaScope>();
            scopeContext.LambdaContext = context;

            lambda = provider.GetRequiredService<TLambda>();
            return await lambda.Handle(parameter);
        }

        /// <summary>
        /// Disposes the Lambda Host and its subresources asynchronously.
        /// </summary>
        /// <returns>The disposal task.</returns>
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

        private async Task Initialize()
        {
            var services = ServiceProvider.GetServices<ILambdaInitializationService>();

            var initializeTasks = services.Select(service => service.Initialize());
            await Task.WhenAll(initializeTasks);

            var disposeTasks = services.Select(MaybeDispose);
            await Task.WhenAll(disposeTasks);
        }
    }
}
