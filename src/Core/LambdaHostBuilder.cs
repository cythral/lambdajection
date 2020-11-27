using System;

using Amazon.Lambda.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    /// <summary>
    /// Helper class for building Lambda Hosts.
    /// </summary>
    /// <typeparam name="TLambda">The type of lambda to host.</typeparam>
    /// <typeparam name="TLambdaParameter">The type of the lambda's input parameter.</typeparam>
    /// <typeparam name="TLambdaOutput">The type of the lambda's return value.</typeparam>
    /// <typeparam name="TLambdaStartup">The type of startup class to use for hosting the lambda.</typeparam>
    /// <typeparam name="TLambdaConfigurator">The type of configurator class to use when building the lambda.</typeparam>
    /// <typeparam name="TLambdaConfigFactory">The type of config factory to use when building the lambda.</typeparam>
    internal static class LambdaHostBuilder<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
#pragma warning disable SA1401, SA1311, SA1304

        /// <summary>
        /// The service provider containing services to inject into the lambda.
        /// This gets reused between invocations.
        /// </summary>
        internal static IServiceProvider serviceProvider = BuildServiceProvider();

        /// <summary>
        /// Whether or not to run initialization services for the lambda.  Initialization
        /// services only run once.
        /// </summary>
        internal static bool runInitializationServices = true;

        /// <summary>
        /// The context for the current invocation.
        /// </summary>
        internal static ILambdaContext? context;

#pragma warning restore SA1401, SA1311, SA1304

        /// <summary>
        /// Builds a new lambda host.
        /// </summary>
        /// <param name="host">The host to build.</param>
        public static void Build(LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory> host)
        {
            host.ServiceProvider = serviceProvider;
            host.RunInitializationServices = runInitializationServices;
            runInitializationServices = false;
        }

        /// <summary>
        /// Builds a service provider for the lambda host.
        /// </summary>
        /// <returns>The built service provider.</returns>
        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = new TLambdaConfigFactory().Create();
            var serviceCollection = BuildServiceCollection(configuration);
            using var intermediateProvider = serviceCollection.BuildServiceProvider();
            _ = RunLambdaStartup(intermediateProvider, serviceCollection);
            _ = RunOptionsConfigurator(intermediateProvider, serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Builds a service collection for the lambda host.
        /// </summary>
        /// <param name="configuration">The configuration to inject into the service collection.</param>
        /// <returns>The built service collection.</returns>
        public static IServiceCollection BuildServiceCollection(IConfigurationRoot configuration)
        {
            return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<ILambdaStartup, TLambdaStartup>()
            .AddSingleton<ILambdaConfigurator, TLambdaConfigurator>()
            .AddScoped<TLambda>()
            .AddScoped<LambdaScope>()
            .AddScoped(provider =>
            {
                var scope = provider.GetRequiredService<LambdaScope>();
                return scope.LambdaContext ?? throw new Exception("The Lambda Context has not been registered.");
            });
        }

        /// <summary>
        /// Runs the lambda's startup class.
        /// </summary>
        /// <param name="provider">The intermediate service provider to retrieve the startup class from.</param>
        /// <param name="serviceCollection">The service collection to add services to.</param>
        /// <returns>The startup class that was built and run for the lambda.</returns>
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

        /// <summary>
        /// Runs the lambda's configurator to add dynamic services generated at compile-time.
        /// </summary>
        /// <param name="provider">The intermediate service provider to get the configurator from.</param>
        /// <param name="serviceCollection">The service collection to add services to.</param>
        /// <returns>The configurator that was built and run for the lambda.</returns>
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
