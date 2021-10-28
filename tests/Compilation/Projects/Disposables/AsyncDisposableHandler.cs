using System;
using System.Threading;
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
    public partial class AsyncDisposableHandler : IAsyncDisposable
    {
        public static bool DisposeAsyncWasCalled { get; set; } = false;

        public Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("ok");
        }

        public ValueTask DisposeAsync()
        {
            DisposeAsyncWasCalled = true;

#pragma warning disable CA1816
            GC.SuppressFinalize(this);
#pragma warning restore CA1816

            return new ValueTask();
        }
    }
}
