using System.Threading.Tasks;

using Amazon.Runtime;

namespace Lambdajection.Core
{
    /// <summary>
    /// Describes an Amazon Service Client factory which creates clients whose credentials can be the result
    /// of assuming a role.
    /// </summary>
    /// <typeparam name="TAmazonService">The interface type of the Amazon Service Client to create.</typeparam>
    public interface IAwsFactory<TAmazonService>
        where TAmazonService : IAmazonService
    {
        /// <summary>
        /// Creates an Amazon Service Client of type TAmazonService.
        /// </summary>
        /// <param name="roleArn">ARN of the role to assume for obtaining security credentials.</param>
        /// <returns>An Amazon Service Client with credentials that were the result of assuming the given role arn.</returns>
        Task<TAmazonService> Create(string? roleArn = null);
    }
}
