using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    internal class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
    {
        internal static IServiceProvider? serviceProvider;

        public static void Build(LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup> host)
        {
            serviceProvider = serviceProvider ?? BuildServiceProvider();
            host.ServiceProvider = serviceProvider;
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();
            var serviceCollection = BuildServiceCollection(configuration);
            var startup = BuildLambdaStartup(configuration, serviceCollection);
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

        public static TLambdaStartup BuildLambdaStartup(IConfigurationRoot configuration, IServiceCollection serviceCollection)
        {
            var startup = new TLambdaStartup();
            startup.Configuration = configuration;
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