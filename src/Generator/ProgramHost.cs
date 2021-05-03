using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lambdajection.Generator
{
    public class ProgramHost
    {
        public void Run(GeneratorExecutionContext context)
        {
            Host.CreateDefaultBuilder()
                .ConfigureServices((builderContext, services) =>
                {
                    services.AddSingleton(new ProgramContext { GeneratorExecutionContext = context });
                    services.AddSingleton<IHost, GeneratorHost>();
                    new Startup().ConfigureServices(services);

                    services.AddLogging(options =>
                    {
                        options.ClearProviders();
                        options.AddConsole();
                    });
                })
                .Build()
                .RunAsync(context.CancellationToken)
                .GetAwaiter()
                .GetResult();
        }
    }
}
