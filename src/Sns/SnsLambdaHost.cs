using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;
using Lambdajection.Core.Exceptions;

using Microsoft.Extensions.Logging;

namespace Lambdajection.Sns
{
    /// <inheritdoc />
    public class SnsLambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : LambdaHostBase<TLambda, SnsMessage<TLambdaParameter>, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
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
        internal SnsLambdaHost(Action<LambdaHostBase<TLambda, SnsMessage<TLambdaParameter>, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
            : base(build)
        {
        }

        /// <inheritdoc />
        public override async Task<TLambdaOutput> InvokeLambda(
            Stream inputStream,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snsEvent = await Deserialize(inputStream, cancellationToken);
            var message = snsEvent.Records[0].Sns;
            Validate(message);

            var response = await Lambda.Handle(message, cancellationToken);
            Logger.LogInformation("SNS Response: {response}", response);
            return response;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<SnsEvent<TLambdaParameter>> Deserialize(Stream inputStream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Beginning deserialization of sns request.");
            Stopwatch.Restart();

            var snsEvent = await Serializer.Deserialize<SnsEvent<TLambdaParameter>>(inputStream, cancellationToken)
                ?? throw new InvalidLambdaParameterException();

            Stopwatch.Stop();
            Logger.LogInformation("Received sns request: {request}", snsEvent);
            Logger.LogInformation("Finished deserializing sns request in {time} ms", Stopwatch.ElapsedMilliseconds);
            return snsEvent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Validate(SnsMessage<TLambdaParameter> message)
        {
            Logger.LogInformation("Beginning sns request validation.");
            Stopwatch.Restart();

            Lambda.Validate(message);

            Stopwatch.Stop();
            Logger.LogInformation("Finished request validation in {time} ms", Stopwatch.ElapsedMilliseconds);
        }
    }
}
