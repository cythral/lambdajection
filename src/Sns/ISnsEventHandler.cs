using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Framework;

namespace Lambdajection.Sns
{
    /// <summary>
    /// Describes an Amazon Lambda function.
    /// </summary>
    /// <typeparam name="TLambdaParameter">The type of the lambda's input parameter.</typeparam>
    /// <typeparam name="TLambdaOutput">The type of the lambda's return value.</typeparam>
    public interface ISnsEventHandler<TLambdaParameter, TLambdaOutput>
    {
        /// <summary>
        /// The lambda's entrypoint.
        /// </summary>
        /// <param name="parameter">The lambda's input parameter.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The lambda's return value.</returns>
        Task<TLambdaOutput> Handle(SnsMessage<TLambdaParameter> parameter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs validations the lambda's input parameter.
        /// </summary>
        /// <param name="parameter">The lambda's input parameter.</param>
        [Generated("Lambdajection.Generator", "Lambdajection.Generator.ValidationsGenerator")]
        void Validate(SnsMessage<TLambdaParameter> parameter);
    }
}
