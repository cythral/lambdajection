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
        Task Initialize();
    }
}