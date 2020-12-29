using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using TestLambdaHost = Lambdajection.Core.LambdaHost<
    Lambdajection.TestLambda,
    object,
    object,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;

namespace Lambdajection.Core.Tests
{
    [Category("Unit")]
    public class LambdaHostTests
    {
        [Test, Auto]
        public async Task RunCreatesLambdaAndCallsHandle(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<object>(), Any<CancellationToken>()).Returns(expectedResponse);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
            });

            var cancellationToken = new CancellationToken(false);
            var response = await host.Run(request, context, cancellationToken);

            response.Should().Be(expectedResponse);
            await lambda.Received().Handle(Is(request), Is(cancellationToken));
        }

        [Test, Auto]
        public async Task RunRunsInitializationServices(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<object>(), Any<CancellationToken>()).Returns(expectedResponse);

            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            var cancellationToken = new CancellationToken(false);
            await host.Run(request, context, cancellationToken);

            await initializationService.Received().Initialize(Is(cancellationToken));
        }

        [Test, Auto]
        public async Task RunSetsLambdaContextOnScope(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<object>()).Returns(expectedResponse);

            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(request, context);

            scope.LambdaContext.Should().Be(context);
        }

        [Test, Auto]
        public async Task RunDisposesInitializationServices(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            var initializationService = Substitute.For<ILambdaInitializationService, IDisposable>();

            lambda.Handle(Any<object>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(request, context);

            initializationService.As<IDisposable>().Received().Dispose();
        }

        [Test, Auto]
        public async Task RunDisposesInitializationServicesAsync(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            var initializationService = Substitute.For<ILambdaInitializationService, IAsyncDisposable>();
            lambda.Handle(Any<object>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(request, context);

            await initializationService.As<IAsyncDisposable>().Received().DisposeAsync();
        }

        [Test, Auto]
        public async Task RunDoesNotRunInitializationServicesIfPropertySetToFalse(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<object>(), Any<CancellationToken>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            });

            var cancellationToken = new CancellationToken(false);
            await host.Run(request, context, cancellationToken);

            await initializationService.DidNotReceive().Initialize(Is(cancellationToken));
        }

        [Test, Auto]
        public async Task DisposeAsyncIsCalled(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] AsyncDisposableLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                await host.Run(request, context);
            }

            await lambda.Received().DisposeAsync();
        }

        [Test, Auto]
        public async Task DisposeIsCalled(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] Action<object> suppressor,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] DisposableLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                await host.Run(request, context);
            }

            lambda.Received().Dispose();
        }

        [Test, Auto]
        public async Task DisposeAsyncIsPreferred(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] MultiDisposableLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);
            collection.AddSingleton(scope);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                await host.Run(request, context);
            }

            await lambda.Received().DisposeAsync();
            lambda.DidNotReceive().Dispose();
        }

        [Test, Auto]
        public async Task FinalizationIsSuppressed(
            string expectedResponse,
            object request,
            ServiceCollection collection,
            [Substitute] Action<object> suppressor,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] MultiDisposableLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);
            collection.AddSingleton(scope);

            TestLambdaHost host;
            var provider = collection.BuildServiceProvider();
            await using (host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                await host.Run(request, context);
            }

            suppressor.Received()(Is(host));
        }
    }
}
