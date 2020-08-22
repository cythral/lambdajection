using System.Threading.Tasks;

using Amazon.Runtime;

namespace Lambdajection.Core
{
    public interface IAwsFactory<TAmazonService> where TAmazonService : IAmazonService
    {
        Task<TAmazonService> Create(string? roleArn = null);
    }
}
