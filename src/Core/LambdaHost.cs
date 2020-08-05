using System;
using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Core
{
    public class LambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup>
        where TLambda : class, ILambda<TLambdaParameter, TLambdaOutput>
        where TLambdaStartup : ILambdaStartup, new()
    {
        private static IServiceProvider serviceProvider;

        static LambdaHost()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddTransient<TLambda>();

            var startup = new TLambdaStartup();
            startup.Configuration = configuration;
            startup.ConfigureServices(serviceCollection);
            serviceCollection.AddLogging(logging =>
            {
                logging.AddConsole();
                startup.ConfigureLogging(logging);
            });

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public LambdaHost() { }

        public async Task<TLambdaOutput> Run(TLambdaParameter parameter, ILambdaContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TLambda>();
                return await service.Handle(parameter, context);
            }
        }
    }
}