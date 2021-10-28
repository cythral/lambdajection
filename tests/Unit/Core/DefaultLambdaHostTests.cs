using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using TestLambdaHost = Lambdajection.Core.DefaultLambdaHost<
    Lambdajection.TestLambda,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type

namespace Lambdajection.Core.Tests
{
    [Category("Unit")]
    public class DefaultLambdaHostTests
    {
        public static async Task<Stream> CreateStreamForRequest(TestLambdaMessage request)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request);
            stream.Position = 0;
            return stream;
        }

        [Test, Auto]
        public async Task RunCreatesLambdaAndCallsHandle(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<TestLambdaMessage>(), Any<CancellationToken>()).Returns(expectedResponse);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
            });

            var cancellationToken = new CancellationToken(false);
            var responseStream = await host.Run(inputStream, context, cancellationToken);
            var response = await JsonSerializer.DeserializeAsync<TestLambdaMessage>(responseStream);

            response.Should().NotBeNull();
            response!.Id.Should().Be(expectedResponse.Id);
            await lambda.Received().Handle(Is<TestLambdaMessage>(req => req.Id == request.Id), Is(cancellationToken));
        }

        [Test, Auto]
        public async Task RunRunsInitializationServices(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<TestLambdaMessage>(), Any<CancellationToken>()).Returns(expectedResponse);

            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            var cancellationToken = new CancellationToken(false);
            await host.Run(inputStream, context, cancellationToken);

            await initializationService.Received().Initialize(Is(cancellationToken));
        }

        [Test, Auto]
        public async Task RunSetsLambdaContextOnScope(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<TestLambdaMessage>()).Returns(expectedResponse);

            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(inputStream, context);

            scope.LambdaContext.Should().Be(context);
        }

        [Test, Auto]
        public async Task RunDisposesInitializationServices(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            var initializationService = Substitute.For<ILambdaInitializationService, IDisposable>();

            lambda.Handle(Any<TestLambdaMessage>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(inputStream, context);

            initializationService.As<IDisposable>().Received().Dispose();
        }

        [Test, Auto]
        public async Task RunDisposesInitializationServicesAsync(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            var initializationService = Substitute.For<ILambdaInitializationService, IAsyncDisposable>();
            lambda.Handle(Any<TestLambdaMessage>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            await host.Run(inputStream, context);

            await initializationService.As<IAsyncDisposable>().Received().DisposeAsync();
        }

        [Test, Auto]
        public async Task RunDoesNotRunInitializationServicesIfPropertySetToFalse(
            TestLambdaMessage expectedResponse,
            TestLambdaMessage request,
            ServiceCollection collection,
            [Substitute] ILambdaInitializationService initializationService,
            [Substitute] TestLambda lambda,
            [Substitute] LambdaScope scope,
            [Substitute] ILambdaContext context
        )
        {
            lambda.Handle(Any<TestLambdaMessage>(), Any<CancellationToken>()).Returns(expectedResponse);
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);
            collection.AddSingleton(scope);

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            });

            var cancellationToken = new CancellationToken(false);
            await host.Run(inputStream, context, cancellationToken);

            await initializationService.DidNotReceive().Initialize(Is(cancellationToken));
        }

        [Test, Auto]
        public async Task DisposeAsyncIsCalled(
            string expectedResponse,
            TestLambdaMessage request,
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

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                await host.Run(inputStream, context);
            }

            await lambda.Received().DisposeAsync();
        }

        [Test, Auto]
        public async Task DisposeIsCalled(
            string expectedResponse,
            TestLambdaMessage request,
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

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                await host.Run(inputStream, context);
            }

            lambda.Received().Dispose();
        }

        [Test, Auto]
        public async Task DisposeAsyncIsPreferred(
            string expectedResponse,
            TestLambdaMessage request,
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

            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                await host.Run(inputStream, context);
            }

            await lambda.Received().DisposeAsync();
            lambda.DidNotReceive().Dispose();
        }

        [Test, Auto]
        public async Task FinalizationIsSuppressed(
            string expectedResponse,
            TestLambdaMessage request,
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
            using var inputStream = await CreateStreamForRequest(request);
            var provider = collection.BuildServiceProvider();
            await using (host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                await host.Run(inputStream, context);
            }

            suppressor.Received()(Is(host));
        }
    }
}
