using System;

using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core
{
    /// <inheritdoc />
    public sealed class DefaultLambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : LambdaHostBase<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        public DefaultLambdaHost()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        /// <param name="build">The builder action to run on the lambda.</param>
        internal DefaultLambdaHost(Action<LambdaHostBase<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
            : base(build)
        {
        }

        /// <inheritdoc />
        public override async Task<TLambdaOutput> InvokeLambda(
            TLambdaParameter parameter,
            CancellationToken cancellationToken = default
        )
        {
            Lambda.Validate(parameter);
            return await Lambda.Handle(parameter, cancellationToken);
        }
    }
}
