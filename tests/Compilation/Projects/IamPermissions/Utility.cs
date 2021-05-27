using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Framework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.IamPermissions
{
    public class Utility
    {
        private readonly IAmazonS3 s3Client;

        public Utility(
            IAmazonS3 s3Client
        )
        {
            this.s3Client = s3Client;
        }

        [RequiresIamPermission("ec2:ArbitraryOperation1")]
        [RequiresIamPermission("ec2:ArbitraryOperation2")]
        public void ArbitraryOperation()
        {
        }

        public async Task GetObject()
        {
            await s3Client.GetObjectAsync("test", "test");
        }
    }
}
