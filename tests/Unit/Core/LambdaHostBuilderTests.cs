using System;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using NSubstitute;

using NUnit.Framework;

using TestLambdaHost = Lambdajection.Core.DefaultLambdaHost<
    Lambdajection.TestLambda,
    object,
    object,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;
using TestLambdaHostBuilder = Lambdajection.Core.LambdaHostBuilder<
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
    public class LambdaHostBuilderTests
    {
        [Test]
        public async Task BuildSetsTheServiceProvider()
        {
            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.Build(host);
            host.ServiceProvider.Should().NotBeNull();
        }

        [Test, Auto]
        public async Task BuildSetsTheServiceProviderIfItAlreadyWasBuilt(
            [Substitute] IServiceProvider serviceProvider
        )
        {
            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.serviceProvider = serviceProvider;
            TestLambdaHostBuilder.Build(host);

            host.ServiceProvider.Should().BeSameAs(serviceProvider);
        }

        [Test, Auto]
        public async Task BuildSetsRunInitializationServicesToTrueTheFirstTime(
            [Substitute] IServiceProvider serviceProvider
        )
        {
            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.serviceProvider = serviceProvider;
            TestLambdaHostBuilder.runInitializationServices = true;
            TestLambdaHostBuilder.Build(host);

            host.RunInitializationServices.Should().BeTrue();
        }

        [Test, Auto]
        public async Task BuildSetsRunInitializationServicesToFalseTheSecondTime(
            [Substitute] IServiceProvider serviceProvider
        )
        {
            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.serviceProvider = serviceProvider;
            TestLambdaHostBuilder.runInitializationServices = true;
            TestLambdaHostBuilder.Build(host);
            TestLambdaHostBuilder.Build(host);

            host.RunInitializationServices.Should().BeFalse();
        }

        [Test]
        public void BuildServiceProviderReturnsServiceProviderWithConfiguration()
        {
            var provider = TestLambdaHostBuilder.BuildServiceProvider();

            var configuration = provider.GetService<IConfiguration>();
            configuration.Should().NotBeNull();
        }

        [Test]
        public void BuildServiceProviderReturnsServiceProviderWithLogger()
        {
            var provider = TestLambdaHostBuilder.BuildServiceProvider();

            var configuration = provider.GetService<ILogger<TestLambda>>();
            configuration.Should().NotBeNull();
        }

        [Test]
        public void BuildServiceProviderReturnsServiceProviderWithLambda()
        {
            var provider = TestLambdaHostBuilder.BuildServiceProvider();

            var configuration = provider.GetService<TestLambda>();
            configuration.Should().NotBeNull();
        }

        [Test, Auto]
        public void BuildServiceCollectionShouldAddConfiguration(
            [Substitute] IConfigurationRoot configuration
        )
        {
            var collection = TestLambdaHostBuilder.BuildServiceCollection(configuration);

            collection.Should().Contain(descriptor =>
                descriptor.ServiceType == typeof(IConfiguration) &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ImplementationInstance == configuration
            );
        }

        [Test, Auto]
        public void BuildServiceCollectionShouldAddLambda(
            [Substitute] IConfigurationRoot configuration
        )
        {
            var collection = TestLambdaHostBuilder.BuildServiceCollection(configuration);

            collection.Should().Contain(descriptor =>
                descriptor.ServiceType == typeof(TestLambda) &&
                descriptor.Lifetime == ServiceLifetime.Scoped
            );
        }

        [Test, Auto]
        public void BuildConfiguratorShouldConfigureOptions(
            ServiceCollection serviceCollection,
            [Substitute] IConfigurationRoot configuration,
            [Substitute] ILambdaConfigurator configurator
        )
        {
            serviceCollection.AddSingleton(configurator);
            serviceCollection.AddSingleton<IConfiguration>(configuration);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunOptionsConfigurator(provider, serviceCollection);

            configurator.Received().ConfigureOptions(Arg.Is(configuration), Arg.Is(serviceCollection));
        }

        [Test, Auto]
        public void BuildConfiguratorShouldConfigureAwsServices(
            ServiceCollection serviceCollection,
            [Substitute] IConfigurationRoot configuration,
            [Substitute] ILambdaConfigurator configurator
        )
        {
            serviceCollection.AddSingleton(configurator);
            serviceCollection.AddSingleton<IConfiguration>(configuration);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunOptionsConfigurator(provider, serviceCollection);

            configurator.Received().ConfigureAwsServices(Arg.Is(serviceCollection));
        }

        [Test, Auto]
        public void RunLambdaStartupShouldAddServiceCollection(
            ServiceCollection serviceCollection,
            [Substitute] ILambdaStartup startup
        )
        {
            serviceCollection.AddSingleton(startup);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, serviceCollection);

            startup.Received().ConfigureServices(Arg.Is(serviceCollection));
        }

        [Test, Auto]
        public void RunLambdaStartupShouldAddLoggingToServiceCollection(
            ServiceCollection serviceCollection,
            [Substitute] ILambdaStartup startup
        )
        {
            serviceCollection.AddSingleton(startup);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, serviceCollection);

            serviceCollection.Should().Contain(descriptor => descriptor.ServiceType == typeof(ILoggerFactory));
        }

        [Test, Auto]
        public void RunLambdaStartupShouldAddConsoleDestinationToLogging(
            ServiceCollection serviceCollection,
            [Substitute] ILambdaStartup startup
        )
        {
            serviceCollection.AddSingleton(startup);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, serviceCollection);

            serviceCollection.Should().Contain(descriptor => descriptor.ImplementationType == typeof(ConsoleLoggerProvider));
        }

        [Test, Auto]
        public void RunLambdaStartupShouldCallConfigureLoggingOnStartup(
            ServiceCollection serviceCollection,
            [Substitute] ILambdaStartup startup
        )
        {
            serviceCollection.AddSingleton(startup);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, serviceCollection);

            startup.Received().ConfigureLogging(Arg.Any<ILoggingBuilder>());
        }
    }
}
