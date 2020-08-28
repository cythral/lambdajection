using System.Threading.Tasks;

using Amazon.Lambda.Core;

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
        /// <param name="context">The lambda's context object.</param>
        /// <returns>The lambda's return value.</returns>
        Task<TLambdaOutput> Handle(TLambdaParameter parameter, ILambdaContext context);
    }
}
