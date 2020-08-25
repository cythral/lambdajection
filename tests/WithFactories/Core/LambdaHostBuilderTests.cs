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
                descriptor.Lifetime == ServiceLifetime.Transient
            );
        }

        [Test]
        public void BuildConfiguratorShouldConfigureOptions()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var serviceCollection = Substitute.For<IServiceCollection>();
            var configurator = TestLambdaHostBuilder.BuildOptionsConfigurator(configuration, serviceCollection);

            configurator.Configuration.Should().BeSameAs(configuration);
            configurator.ServicesSetByConfigureOptions.Should().BeSameAs(serviceCollection);
        }

        [Test]
        public void BuildConfiguratorShouldConfigureAwsServices()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var serviceCollection = Substitute.For<IServiceCollection>();
            var configurator = TestLambdaHostBuilder.BuildOptionsConfigurator(configuration, serviceCollection);

            configurator.ServicesSetByConfigureAwsServices.Should().BeSameAs(serviceCollection);
        }

        [Test]
        public void BuildLambdaStartupShouldAddConfiguration()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = Substitute.For<IServiceCollection>();
            var startup = TestLambdaHostBuilder.BuildLambdaStartup(configuration, collection);

            startup.Configuration.Should().BeSameAs(configuration);
        }

        [Test]
        public void BuildLambdaStartupShouldAddServiceCollection()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = Substitute.For<IServiceCollection>();
            var startup = TestLambdaHostBuilder.BuildLambdaStartup(configuration, collection);

            startup.Services.Should().BeSameAs(collection);
        }

        [Test]
        public void BuildLambdaStartupShouldAddLoggingToServiceCollection()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = new ServiceCollection();
            var startup = TestLambdaHostBuilder.BuildLambdaStartup(configuration, collection);

            collection.Should().Contain(descriptor => descriptor.ServiceType == typeof(ILoggerFactory));
        }

        [Test]
        public void BuildLambdaStartupShouldAddConsoleDestinationToLogging()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = new ServiceCollection();
            var startup = TestLambdaHostBuilder.BuildLambdaStartup(configuration, collection);

            collection.Should().Contain(descriptor => descriptor.ImplementationType == typeof(ConsoleLoggerProvider));
        }

        [Test]
        public void BuildLambdaStartupShouldCallConfigureLoggingOnStartup()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            var collection = new ServiceCollection();
            var startup = TestLambdaHostBuilder.BuildLambdaStartup(configuration, collection);

            startup.LoggingBuilder.Should().NotBeNull();
        }
    }
}
