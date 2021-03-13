using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    [Generator]
    public class Program : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var buildTimeAssemblies = GetBuildTimeAssemblies(context);
            foreach (var buildTimeAssembly in buildTimeAssemblies)
            {
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(buildTimeAssembly);
                }
                catch (FileLoadException)
                {
                }
            }

            Run(context);
        }

        private IEnumerable<string> GetBuildTimeAssemblies(GeneratorExecutionContext context)
        {
            var options = context.AnalyzerConfigOptions.GlobalOptions;

            options.TryGetValue("build_property.LambdajectionBuildTimeAssemblies", out var buildTimeAssembliesString);
            buildTimeAssembliesString ??= string.Empty;

            return from assemblyName in buildTimeAssembliesString.Split(',')
                   where assemblyName != string.Empty
                   select assemblyName;
        }

        private void Run(GeneratorExecutionContext context)
        {
            Host.CreateDefaultBuilder()
                .ConfigureServices((builderContext, services) =>
                {
                    services.AddSingleton(new ProgramContext { GeneratorExecutionContext = context });
                    services.AddSingleton<IHost, GeneratorHost>();
                    new Startup().ConfigureServices(services);
                })
                .Build()
                .RunAsync(context.CancellationToken)
                .GetAwaiter()
                .GetResult();
        }
    }
}
