using System;
using System.Linq;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using NSubstitute;

using NUnit.Framework;

using TestLambdaHost = Lambdajection.Core.LambdaHost<
    Lambdajection.TestLambda,
    object,
    object,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator
>;

using TestLambdaHostBuilder = Lambdajection.Core.LambdaHostBuilder<
    Lambdajection.TestLambda,
    object,
    object,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator
>;

namespace Lambdajection.Core.Tests
{
    public class LambdaHostBuilderTests
    {
        [Test]
        public void BuildSetsTheServiceProvider()
        {
            var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            TestLambdaHostBuilder.Build(host);
            host.ServiceProvider.Should().NotBeNull();
        }

        [Test]
        public void BuildSetsTheServiceProviderIfItAlreadyWasBuilt()
        {
            var host = new TestLambdaHost(lambdaHost => { });
            host.ServiceProvider.Should().BeNull();

            var serviceProvider = Substitute.For<IServiceProvider>();
            TestLambdaHostBuilder.serviceProvider = serviceProvider;
            TestLambdaHostBuilder.Build(host);

            host.ServiceProvider.Should().BeSameAs(serviceProvider);
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
        public void BuildConfigurationShouldAddJsonFile()
        {
            var configuration = TestLambdaHostBuilder.BuildConfiguration();

            configuration.Providers.Should().Contain(provider => provider.GetType() == typeof(JsonConfigurationProvider));
        }

        [Test]
        public void JsonConfigurationShouldBeOptional()
        {
            var configuration = TestLambdaHostBuilder.BuildConfiguration();
            var providerQuery = from p in configuration.Providers where p.GetType() == typeof(JsonConfigurationProvider) select (JsonConfigurationProvider)p;
            var provider = providerQuery.First();

            provider.Source.Optional.Should().BeTrue();
        }

        [Test]
        public void BuildConfigurationShouldAddEnvironmentVariables()
        {
            var configuration = TestLambdaHostBuilder.BuildConfiguration();
            configuration.Providers.Should().Contain(provider => provider.GetType() == typeof(EnvironmentVariablesConfigurationProvider));
        }

        [Test]
        public void BuildServiceCollectionShouldAddConfiguration()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = TestLambdaHostBuilder.BuildServiceCollection(configuration);

            collection.Should().Contain(descriptor =>
                descriptor.ServiceType == typeof(IConfiguration) &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ImplementationInstance == configuration
            );
        }

        [Test]
        public void BuildServiceCollectionShouldAddLambda()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = TestLambdaHostBuilder.BuildServiceCollection(configuration);

            collection.Should().Contain(descriptor =>
                descriptor.ServiceType == typeof(TestLambda) &&
                descriptor.Lifetime == ServiceLifetime.Scoped
            );
        }

        [Test]
        public void BuildConfiguratorShouldConfigureOptions()
        {
            var configuration = Substitute.For<IConfiguration>();
            var configurator = Substitute.For<ILambdaConfigurator>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configurator);
            serviceCollection.AddSingleton(configuration);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunOptionsConfigurator(provider, serviceCollection);

            configurator.Received().ConfigureOptions(Arg.Is(configuration), Arg.Is(serviceCollection));
        }

        [Test]
        public void BuildConfiguratorShouldConfigureAwsServices()
        {
            var configuration = Substitute.For<IConfiguration>();
            var configurator = Substitute.For<ILambdaConfigurator>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configurator);
            serviceCollection.AddSingleton(configuration);

            var provider = serviceCollection.BuildServiceProvider();
            TestLambdaHostBuilder.RunOptionsConfigurator(provider, serviceCollection);

            configurator.Received().ConfigureAwsServices(Arg.Is(serviceCollection));
        }

        [Test]
        public void RunLambdaStartupShouldAddServiceCollection()
        {
            var startup = Substitute.For<ILambdaStartup>();
            var collection = new ServiceCollection();
            collection.AddSingleton(startup);

            var provider = collection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, collection);

            startup.Received().ConfigureServices(Arg.Is(collection));
        }

        [Test]
        public void RunLambdaStartupShouldAddLoggingToServiceCollection()
        {
            var startup = Substitute.For<ILambdaStartup>();
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = new ServiceCollection();
            collection.AddSingleton(startup);

            var provider = collection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, collection);

            collection.Should().Contain(descriptor => descriptor.ServiceType == typeof(ILoggerFactory));
        }

        [Test]
        public void RunLambdaStartupShouldAddConsoleDestinationToLogging()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var startup = Substitute.For<ILambdaStartup>();
            var collection = new ServiceCollection();
            collection.AddSingleton(startup);

            var provider = collection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, collection);

            collection.Should().Contain(descriptor => descriptor.ImplementationType == typeof(ConsoleLoggerProvider));
        }

        [Test]
        public void RunLambdaStartupShouldCallConfigureLoggingOnStartup()
        {
            var collection = new ServiceCollection();
            var startup = Substitute.For<ILambdaStartup>();
            collection.AddSingleton(startup);

            var provider = collection.BuildServiceProvider();
            TestLambdaHostBuilder.RunLambdaStartup(provider, collection);

            startup.Received().ConfigureLogging(Arg.Any<ILoggingBuilder>());
        }
    }
}
