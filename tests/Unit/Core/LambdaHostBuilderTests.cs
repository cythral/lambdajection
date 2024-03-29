using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.Core.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using TestLambdaHost = Lambdajection.Core.DefaultLambdaHost<
    Lambdajection.TestLambda,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;
using TestLambdaHostBuilder = Lambdajection.Core.LambdaHostBuilder<
    Lambdajection.TestLambda,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestLambdaMessage,
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

        [Test]
        public async Task BuildSetsTheLogger()
        {
            await using var host = new TestLambdaHost(lambdaHost => { });
            host.Logger.Should().BeNull();

            TestLambdaHostBuilder.Build(host);
            host.Logger.Should().NotBeNull();
        }

        [Test, Auto]
        public async Task BuildSetsTheServiceProviderIfItAlreadyWasBuilt(
            ILoggerFactory loggerFactory
        )
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton(loggerFactory)
            .BuildServiceProvider();

            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.ServiceProvider = serviceProvider;
            TestLambdaHostBuilder.Build(host);

            host.ServiceProvider.Should().BeSameAs(serviceProvider);
        }

        [Test, Auto]
        public async Task BuildSetsRunInitializationServicesToTrueTheFirstTime(
            ILoggerFactory loggerFactory
        )
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton(loggerFactory)
            .BuildServiceProvider();

            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.ServiceProvider = serviceProvider;
            TestLambdaHostBuilder.RunInitializationServices = true;
            TestLambdaHostBuilder.Build(host);

            host.RunInitializationServices.Should().BeTrue();
        }

        [Test, Auto]
        public async Task BuildSetsRunInitializationServicesToFalseTheSecondTime(
            ILoggerFactory loggerFactory
        )
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton(loggerFactory)
            .BuildServiceProvider();

            await using var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.ServiceProvider = serviceProvider;
            TestLambdaHostBuilder.RunInitializationServices = true;
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

        [Test]
        public void BuildServiceProviderReturnsServiceProviderWithSerializer()
        {
            var provider = TestLambdaHostBuilder.BuildServiceProvider();

            var serializer = provider.GetService<ISerializer>();
            serializer.Should().NotBeNull();
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
        public void RunLambdaStartupShouldAddDefaultWarningFilter(
            ServiceCollection serviceCollection,
            [Substitute] ILambdaStartup startup
        )
        {
            serviceCollection.AddSingleton(startup);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, serviceCollection);

            var options = serviceCollection.BuildServiceProvider().GetRequiredService<IOptions<LoggerFilterOptions>>();
            options.Value.Rules.Should().Contain(rule => rule.CategoryName == TestLambdaHostBuilder.LogCategory && rule.LogLevel == LogLevel.Warning);
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
