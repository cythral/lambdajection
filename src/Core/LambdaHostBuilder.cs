using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lambdajection.Core
{
    internal static class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaOptionsConfigurator>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
        where TLambdaOptionsConfigurator : ILambdaOptionsConfigurator, new()
    {
        internal static IServiceProvider? serviceProvider;

        public static void Build(LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaOptionsConfigurator> host)
        {
            serviceProvider = serviceProvider ?? BuildServiceProvider();
            host.ServiceProvider = serviceProvider;
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();
            var serviceCollection = BuildServiceCollection(configuration);
            var startup = BuildLambdaStartup(configuration, serviceCollection);
            BuildOptionsConfigurator(configuration, serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        public static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        }

        public static IServiceCollection BuildServiceCollection(IConfigurationRoot configuration)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddTransient<TLambda>();
            return serviceCollection;
        }

        public static TLambdaOptionsConfigurator BuildOptionsConfigurator(IConfigurationRoot configuration, IServiceCollection serviceCollection)
        {
            var configurator = new TLambdaOptionsConfigurator();
            configurator.ConfigureOptions(configuration, serviceCollection);
            return configurator;
        }

        public static TLambdaStartup BuildLambdaStartup(IConfigurationRoot configuration, IServiceCollection serviceCollection)
        {
            var startup = new TLambdaStartup
            {
                Configuration = configuration,
            };

            startup.ConfigureServices(serviceCollection);

            serviceCollection.AddLogging(logging =>
            {
                logging.AddConsole();
                startup.ConfigureLogging(logging);
            });

            return startup;
        }
    }
}