using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes an Amazon Lambda function.
    /// </summary>
    /// <typeparam name="TLambdaParameter">The type of the lambda's input parameter.</typeparam>
    /// <typeparam name="TLambdaOutput">The type of the lambda's return value.</typeparam>
    public interface ILambda<TLambdaParameter, TLambdaOutput>
    {
        /// <summary>
        /// The lambda's entrypoint.
        /// </summary>
        /// <param name="parameter">The lambda's input parameter.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The lambda's return value.</returns>
        Task<TLambdaOutput> Handle(TLambdaParameter parameter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs validations the lambda's input parameter.
        /// </summary>
        /// <param name="parameter">The lambda's input parameter.</param>
        [CompilerGenerated]
        void Validate(TLambdaParameter parameter);
    }
}
