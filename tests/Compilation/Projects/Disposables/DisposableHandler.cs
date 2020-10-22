using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lambdajection.CompilationTests.Disposables
{
    [Lambda(typeof(Startup))]
    public partial class DisposableHandler : IDisposable
    {
        public bool DisposeWasCalled = false;

        public Task<DisposableHandler> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(this);
        }

        public void Dispose()
        {
            DisposeWasCalled = true;
            GC.SuppressFinalize(this);
        }
    }
}