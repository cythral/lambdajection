using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

using Lambdajection.Attributes;
using Lambdajection.Core;

namespace Lambdajection.Examples.AwsClientFactories
{
    [Lambda(Startup = typeof(Startup))]
    public partial class Handler
    {
        private readonly IAwsFactory<IAmazonS3> s3Factory;

        public Handler(IAwsFactory<IAmazonS3> s3Factory)
        {
            this.s3Factory = s3Factory;
        }

        public async Task<string> Handle(Request request, ILambdaContext context)
        {
            var s3Client = await s3Factory.Create(request.RoleArn);

            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.FileName,
                ContentBody = request.Contents,
            });

            return $"Successfully written to file {request.FileName} in bucket {request.BucketName}";
        }
    }
}
