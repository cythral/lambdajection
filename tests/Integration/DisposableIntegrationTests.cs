using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Lambdajection.Tests.Integration.Disposable
{

    #region Setup

    [Lambda(typeof(Startup))]
    public sealed partial class DisposableLambda : IDisposable
    {
#pragma warning disable CA2211
        public static bool DisposeWasCalled;
#pragma warning restore CA2211

        public Task<string> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(request);
        }

        public void Dispose()
        {
            DisposeWasCalled = true;
            GC.SuppressFinalize(this);
        }
    }

    [Lambda(typeof(Startup))]
    public sealed partial class AsyncDisposableLambda : IAsyncDisposable
    {
#pragma warning disable CA2211
        public static bool DisposeAsyncWasCalled;
#pragma warning restore CA2211

        public Task<string> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(request);
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

    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }
    }

    #endregion

    #region Tests

    [Category("Integration")]
    public class DisposableIntegrationTests
    {
        [Test]
        public async Task DisposeShouldBeCalled()
        {
            await DisposableLambda.Run("", null);
            DisposableLambda.DisposeWasCalled.Should().BeTrue();
        }

        [Test]
        public async Task DisposeAsyncShouldBeCalled()
        {
            await AsyncDisposableLambda.Run("", null);
            AsyncDisposableLambda.DisposeAsyncWasCalled.Should().BeTrue();
        }
    }

    #endregion
}
