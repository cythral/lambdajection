using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

using Lambdajection.Attributes;
using Lambdajection.Core;

namespace Lambdajection.Examples.AwsClientFactories
{
    [Lambda(typeof(Startup))]
    public partial class Handler : IDisposable
    {
        private readonly IAwsFactory<IAmazonS3> s3Factory;

        private IAmazonS3? s3Client;

        private bool disposed;

        public Handler(IAwsFactory<IAmazonS3> s3Factory)
        {
            this.s3Factory = s3Factory;
        }

        public async Task<string> Handle(Request request, CancellationToken cancellationToken = default)
        {
            s3Client = await s3Factory.Create(request.RoleArn, cancellationToken);

            await s3Client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = request.BucketName,
                    Key = request.FileName,
                    ContentBody = request.Contents,
                },
                cancellationToken
            );

            return $"Successfully written to file {request.FileName} in bucket {request.BucketName}";
        }

        #region Disposable Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            s3Client?.Dispose();
            s3Client = null;
            disposed = true;
        }

        #endregion
    }
}
