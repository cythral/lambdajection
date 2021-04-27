using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.IamPermissions
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly Utility utility;
        private readonly IAmazonS3 s3Client;

        public Handler(Utility utility, IAmazonS3 s3Client)
        {
            this.utility = utility;
            this.s3Client = s3Client;
        }

        public async Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            utility.AbitraryOperation();
            await s3Client.GetObjectAsync("test", "test");
            return (string)null!;
        }
    }
}

