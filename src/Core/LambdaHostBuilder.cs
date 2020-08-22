using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    internal static class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
        where TLambdaConfigurator : ILambdaConfigurator, new()
    {
        internal static IServiceProvider? serviceProvider;

        public static void Build(LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator> host)
        {
            serviceProvider ??= BuildServiceProvider();
            host.ServiceProvider = serviceProvider;
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();
            var serviceCollection = BuildServiceCollection(configuration);
            BuildLambdaStartup(configuration, serviceCollection);
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

        public static TLambdaConfigurator BuildOptionsConfigurator(IConfigurationRoot configuration, IServiceCollection serviceCollection)
        {
            var configurator = new TLambdaConfigurator();
            configurator.ConfigureAwsServices(serviceCollection);
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
