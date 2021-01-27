using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.CustomResource;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.CustomResources
{
    [CustomResourceProvider(typeof(Startup))]
    public partial class Handler
    {
        public Handler()
        {
        }

        private void FailIfRequested(CustomResourceRequest<ResourceProperties> request)
        {
            if (request.ResourceProperties.ShouldFail)
            {
                throw new Exception(request.ResourceProperties.ErrorMessage);
            }
        }

        public Task<ResponseData> Create(CustomResourceRequest<ResourceProperties> request, CancellationToken cancellationToken = default)
        {
            FailIfRequested(request);
            return Task.FromResult(new ResponseData
            {
                Id = request.ResourceProperties.Name,
                MethodCalled = "Create",
            });
        }

        public Task<ResponseData> Update(CustomResourceRequest<ResourceProperties> request, CancellationToken cancellationToken = default)
        {
            FailIfRequested(request);
            return Task.FromResult(new ResponseData
            {
                Id = request.ResourceProperties.Name,
                MethodCalled = "Update",
            });
        }

        public Task<ResponseData> Delete(CustomResourceRequest<ResourceProperties> request, CancellationToken cancellationToken = default)
        {
            FailIfRequested(request);
            return Task.FromResult(new ResponseData
            {
                Id = request.ResourceProperties.Name,
                MethodCalled = "Delete",
            });
        }
    }
}

