using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes a lambda initialization service, which gets run immediately before the lambda is.
    /// </summary>
    public interface ILambdaInitializationService
    {
        /// <summary>
        /// Runs the initialization service's instructions.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel operation.</param>
        /// <returns>The initialization task.</returns>
        Task Initialize(CancellationToken cancellationToken = default);
    }
}
