using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

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
        [Test]
        public async Task RunCreatesLambdaAndCallsHandle()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Any<object>(), Any<ILambdaContext>()).Returns(expectedResponse);

            var collection = new ServiceCollection();
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();
            var response = await host.Run(request, context);

            response.Should().Be(expectedResponse);
            await lambda.Received().Handle(Is(request), Is(context));
        }

        [Test]
        public async Task RunRunsInitializationServices()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Any<object>(), Any<ILambdaContext>()).Returns(expectedResponse);

            var initializationService = Substitute.For<ILambdaInitializationService>();

            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();

            await host.Run(request, context);

            await initializationService.Received().Initialize();
        }

        [Test]
        public async Task RunDisposesInitializationServices()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Any<object>(), Any<ILambdaContext>()).Returns(expectedResponse);

            var initializationService = Substitute.For<ILambdaInitializationService, IDisposable>();

            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();

            await host.Run(request, context);

            initializationService.As<IDisposable>().Received().Dispose();
        }

        [Test]
        public async Task RunDisposesInitializationServicesAsync()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Any<object>(), Any<ILambdaContext>()).Returns(expectedResponse);

            var initializationService = Substitute.For<ILambdaInitializationService, IAsyncDisposable>();

            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = true;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();

            await host.Run(request, context);

            await initializationService.As<IAsyncDisposable>().Received().DisposeAsync();
        }

        [Test]
        public async Task RunDoesNotRunInitializationServicesIfPropertySetToFalse()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Any<object>(), Any<ILambdaContext>()).Returns(expectedResponse);

            var initializationService = Substitute.For<ILambdaInitializationService>();

            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            await using var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();

            await host.Run(request, context);

            await initializationService.DidNotReceive().Initialize();
        }

        [Test]
        public async Task DisposeAsyncIsCalled()
        {
            var lambda = Substitute.For<AsyncDisposableLambda>();
            var initializationService = Substitute.For<ILambdaInitializationService>();
            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                var request = new object();
                var context = Substitute.For<ILambdaContext>();
                await host.Run(request, context);
            }

            await lambda.Received().DisposeAsync();
        }

        [Test]
        public async Task DisposeIsCalled()
        {
            var lambda = Substitute.For<DisposableLambda>();
            var suppressor = Substitute.For<Action<object>>();
            var initializationService = Substitute.For<ILambdaInitializationService>();
            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                var request = new object();
                var context = Substitute.For<ILambdaContext>();
                await host.Run(request, context);
            }

            lambda.Received().Dispose();
        }

        [Test]
        public async Task DisposeAsyncIsPreferred()
        {
            var lambda = Substitute.For<MultiDisposableLambda>();
            var initializationService = Substitute.For<ILambdaInitializationService>();
            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);

            var provider = collection.BuildServiceProvider();
            await using (var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
            }))
            {
                var request = new object();
                var context = Substitute.For<ILambdaContext>();
                await host.Run(request, context);
            }

            await lambda.Received().DisposeAsync();
            lambda.DidNotReceive().Dispose();
        }

        [Test]
        public async Task FinalizationIsSuppressed()
        {
            var lambda = Substitute.For<MultiDisposableLambda>();
            var suppressor = Substitute.For<Action<object>>();
            var initializationService = Substitute.For<ILambdaInitializationService>();
            var collection = new ServiceCollection();
            collection.AddSingleton(initializationService);
            collection.AddSingleton<TestLambda>(lambda);

            TestLambdaHost host;
            var provider = collection.BuildServiceProvider();
            await using (host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
                lambdaHost.RunInitializationServices = false;
                lambdaHost.SuppressFinalize = suppressor;
            }))
            {
                var request = new object();
                var context = Substitute.For<ILambdaContext>();
                await host.Run(request, context);
            }

            suppressor.Received()(Is(host));
        }
    }
}
