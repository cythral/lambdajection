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
    public partial class DisposableHandler : IDisposable
    {
        public static bool DisposeWasCalled { get; set; } = false;

        public Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("ok");
        }

        public void Dispose()
        {
            DisposeWasCalled = true;
            GC.SuppressFinalize(this);
        }
    }
}
