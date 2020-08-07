using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    public class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
    {
        private static IServiceProvider? instance;

        public IServiceProvider GetOrBuildServiceProvider()
        {
            if (instance == null)
            {
                var builder = new LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>();
                instance = builder.BuildServiceProvider();
            }

            return instance;
        }

        public IServiceProvider BuildServiceProvider()
        {
            var configuration = BuildConfiguration();
            var serviceCollection = BuildServiceCollection(configuration);
            var startup = BuildLambdaStartup(configuration, serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        public IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        }

        public IServiceCollection BuildServiceCollection(IConfigurationRoot configuration)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddTransient<TLambda>();
            return serviceCollection;
        }

        public TLambdaStartup BuildLambdaStartup(IConfigurationRoot configuration, IServiceCollection serviceCollection)
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