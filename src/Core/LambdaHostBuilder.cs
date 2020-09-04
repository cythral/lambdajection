using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    internal static class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
    {
        internal static IServiceProvider serviceProvider = BuildServiceProvider();

        public static void Build(LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator> host)
        {
            host.ServiceProvider = serviceProvider;
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();
            var serviceCollection = BuildServiceCollection(configuration);
            using var intermediateProvider = serviceCollection.BuildServiceProvider();
            RunLambdaStartup(intermediateProvider, serviceCollection);
            RunOptionsConfigurator(intermediateProvider, serviceCollection);
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
            serviceCollection.AddSingleton<ILambdaStartup, TLambdaStartup>();
            serviceCollection.AddSingleton<ILambdaConfigurator, TLambdaConfigurator>();
            serviceCollection.AddScoped<TLambda>();
            return serviceCollection;
        }

        public static ILambdaStartup RunLambdaStartup(IServiceProvider provider, IServiceCollection serviceCollection)
        {
            var startup = provider.GetRequiredService<ILambdaStartup>();
            startup.ConfigureServices(serviceCollection);
            serviceCollection.AddLogging(logging =>
            {
                logging.AddConsole();
                startup.ConfigureLogging(logging);
            });

            return startup;
        }

        public static ILambdaConfigurator RunOptionsConfigurator(IServiceProvider provider, IServiceCollection serviceCollection)
        {
            var configurator = provider.GetRequiredService<ILambdaConfigurator>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            configurator.ConfigureAwsServices(serviceCollection);
            configurator.ConfigureOptions(configuration, serviceCollection);
            return configurator;
        }
    }
}
