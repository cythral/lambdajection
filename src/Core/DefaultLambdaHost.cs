using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

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
            Stream inputStream,
            CancellationToken cancellationToken = default
        )
        {
            var parameter = await Deserialize(inputStream, cancellationToken);
            Validate(parameter, cancellationToken);
            return await Invoke(parameter, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TLambdaParameter> Deserialize(Stream inputStream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Beginning deserialization of input parameter.");

            Stopwatch.Restart();
            var result = await Serializer.Deserialize<TLambdaParameter>(inputStream, cancellationToken);
            Stopwatch.Stop();

            Logger.LogInformation("Received input parameter: {input}", result);
            Logger.LogInformation("Finished deserialization of input parameter in {time} ms", Stopwatch.ElapsedMilliseconds);
            return result!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Validate(TLambdaParameter parameter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Beginning input parameter validation.");

            Stopwatch.Restart();
            Lambda.Validate(parameter);
            Stopwatch.Stop();

            Logger.LogInformation("Successfully validated input parameter in {time} ms", Stopwatch.ElapsedMilliseconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TLambdaOutput> Invoke(TLambdaParameter parameter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Beginning lambda execution.");

            Stopwatch.Restart();
            var result = await Lambda.Handle(parameter!, cancellationToken);
            Stopwatch.Stop();

            Logger.LogInformation("Received lambda output: {output}", result);
            Logger.LogInformation("Finished lambda execution in {time} ms", Stopwatch.ElapsedMilliseconds);
            return result;
        }
    }
}
