using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Amazon.S3;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.AmazonFactories
{
    public class S3Utility
    {
        public IAwsFactory<IAmazonS3> Factory { get; set; }

        public S3Utility(IAwsFactory<IAmazonS3> s3Factory)
        {
            this.Factory = s3Factory;
        }
    }
}