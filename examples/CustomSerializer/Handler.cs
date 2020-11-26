using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Amazon.Lambda.ApplicationLoadBalancerEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomSerializer
{
    [Lambda(typeof(Startup), Serializer = typeof(CamelCaseLambdaJsonSerializer))]
    public partial class Handler : IAsyncDisposable
    {
        private readonly EmbeddedResourceReader reader;

        private bool disposed;

        public Handler(EmbeddedResourceReader reader)
        {
            this.reader = reader;
        }

        public async Task<ApplicationLoadBalancerResponse> Handle(ApplicationLoadBalancerRequest request, ILambdaContext context)
        {
            var path = Regex.Replace(request.Path, @"^\/", "");
            var contents = await reader.ReadAsString(path);

#pragma warning disable IDE0046
            if (contents == null)
            {
#pragma warning restore IDE0046

                return new ApplicationLoadBalancerResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    StatusDescription = $"{(int)HttpStatusCode.NotFound} Not Found",
                    Headers = new Dictionary<string, string> { ["content-type"] = "text/plain" },
                    Body = "Not Found.  Please check your spelling and try again.",
                    IsBase64Encoded = false,
                };
            }

            return new ApplicationLoadBalancerResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                StatusDescription = $"{(int)HttpStatusCode.OK} OK",
                Headers = new Dictionary<string, string> { ["content-type"] = "text/html" },
                Body = contents,
                IsBase64Encoded = false,
            };
        }

        #region Disposable Methods

        public async ValueTask DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            await reader.DisposeOpenContexts();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
