using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            var options = context.AnalyzerConfigOptions.GlobalOptions;
            options.TryGetValue("build_property.LambdajectionAdditionalProbingPath", out var additionalProbingPath);

            AssemblyLoadContext.Default.Resolving += (_, name) =>
            {
                var matchingFiles = from file in Directory.GetFiles(additionalProbingPath, $"{name.Name}.dll", SearchOption.AllDirectories)
                                    where file.Contains("netstandard") || file.Contains("net5.0")
                                    select file;

                foreach (var matchingFile in matchingFiles)
                {
                    try
                    {
                        return Assembly.LoadFile(matchingFile);
                    }
                    catch (Exception)
                    {
                    }
                }

                return null;
            };

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
