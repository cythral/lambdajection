using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0060, CA1801

namespace Lambdajection.Core
{
    /// <summary>
    /// Abstract IoC container / host used for lambdas of type TLambda.
    /// </summary>
    /// <typeparam name="TLambda">The type of lambda to host.</typeparam>
    /// <typeparam name="TLambdaParameter">The type of the lambda's input parameter.</typeparam>
    /// <typeparam name="TLambdaOutput">The type of the lambda's return value.</typeparam>
    /// <typeparam name="TLambdaStartup">The type to use for lambda startup (sets up services).</typeparam>
    /// <typeparam name="TLambdaConfigurator">The type to use for the lambda configurator (sets up options and aws services).</typeparam>
    /// <typeparam name="TLambdaConfigFactory">The type to use for the lambda's config factory.</typeparam>
    public abstract class LambdaHostBase<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : IAsyncDisposable
        where TLambda : class
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaHostBase{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        public LambdaHostBase()
            : this(LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>.Build)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaHostBase{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        /// <param name="build">The builder action to run on this lambda.</param>
        internal LambdaHostBase(Action<LambdaHostBase<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
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
        public ILambdaContext Context { get; private set; } = null!;

        /// <summary>
        /// Gets the lambda to be invoked.
        /// </summary>
        /// <value>The lambda to be invoked.</value>
        protected internal TLambda Lambda { get; internal set; } = null!;

        /// <summary>
        /// Gets the service scope capable of retrieving scoped services.
        /// </summary>
        /// <value>The service scope.</value>
        protected internal IServiceScope Scope { get; internal set; } = null!;

        /// <summary>
        /// Runs the lambda host:
        /// - Runs initialization services.
        /// - Invokes the lambda.
        /// </summary>
        /// <param name="parameter">The input parameter to pass to the lambda.</param>
        /// <param name="context">The context object to pass to the lambda.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The return value of the lambda.</returns>
        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context, CancellationToken cancellationToken = default)
        {
            if (RunInitializationServices)
            {
                await Initialize(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            Scope = ServiceProvider.CreateScope();

            var provider = Scope.ServiceProvider;
            var scopeContext = provider.GetRequiredService<LambdaScope>();
            scopeContext.LambdaContext = context;

            Lambda = provider.GetRequiredService<TLambda>();
            return await InvokeLambda(parameter, cancellationToken);
        }

        /// <summary>
        /// Invokes the Lambda.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the lambda to invoke it with.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The return value of the lambda.</returns>
        public abstract Task<TLambdaOutput> InvokeLambda(TLambdaParameter parameter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disposes the Lambda Host and its subresources asynchronously.
        /// </summary>
        /// <returns>The disposal task.</returns>
        public async ValueTask DisposeAsync()
        {
            await MaybeDispose(Scope);
            await MaybeDispose(Lambda);

            Lambda = null!;
            Scope = null!;

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

        private async Task Initialize(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var services = ServiceProvider.GetServices<ILambdaInitializationService>();

            var initializeTasks = services.Select(service => service.Initialize(cancellationToken));
            await Task.WhenAll(initializeTasks);

            var disposeTasks = services.Select(MaybeDispose);
            await Task.WhenAll(disposeTasks);
        }
    }
}
