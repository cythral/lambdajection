using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;
using Lambdajection.Core.Exceptions;

namespace Lambdajection.Sns
{
    /// <inheritdoc />
    public class SnsLambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : LambdaHostBase<TLambda, SnsMessage<TLambdaParameter>, TLambdaOutput[], TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        where TLambda : class, ISnsEventHandler<TLambdaParameter, TLambdaOutput>
        where TLambdaParameter : class
        where TLambdaOutput : class
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        public SnsLambdaHost()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnsLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        /// <param name="build">The builder action to run on the lambda.</param>
        internal SnsLambdaHost(Action<LambdaHostBase<TLambda, SnsMessage<TLambdaParameter>, TLambdaOutput[], TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
            : base(build)
        {
        }

        /// <inheritdoc />
        public override async Task<TLambdaOutput[]> InvokeLambda(
            Stream inputStream,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snsEvent = await Serializer.Deserialize<SnsEvent<TLambdaParameter>>(inputStream, cancellationToken)
                ?? throw new InvalidLambdaParameterException();

            var tasks = from record in snsEvent.Records select HandleMessage(record.Sns, cancellationToken);
            return await Task.WhenAll(tasks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TLambdaOutput> HandleMessage(SnsMessage<TLambdaParameter> message, CancellationToken cancellationToken = default)
        {
            Lambda.Validate(message);
            return await Lambda.Handle(message, cancellationToken);
        }
    }
}
