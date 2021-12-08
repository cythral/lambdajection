using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.CustomResource;
using Lambdajection.Sns;

using Microsoft.Extensions.Options;

namespace Lambdajection.Examples.SnsHandler
{
    [SnsEventHandler(typeof(Startup))]
    public partial class Handler
    {
        private readonly IHttpClient httpClient;

        public Handler(
            IHttpClient httpClient
        )
        {
            this.httpClient = httpClient;
        }

        public async Task<string> Handle(SnsMessage<CustomResourceRequest<Response>> request, CancellationToken cancellationToken = default)
        {
            var response = new CustomResourceResponse<Response>
            {
                Status = CustomResourceResponseStatus.Success,
                RequestId = request.Message.RequestId,
                StackId = request.Message.StackId,
                Data = new Response(),
                LogicalResourceId = request.Message.LogicalResourceId,
                PhysicalResourceId = string.IsNullOrEmpty(request.Message.PhysicalResourceId) ? Guid.NewGuid().ToString() : request.Message.PhysicalResourceId,
            };

            await httpClient.PutJson(
                requestUri: request.Message.ResponseURL!,
                payload: response,
                contentType: null,
                cancellationToken: cancellationToken
            );

            return "OK";
        }
    }
}
