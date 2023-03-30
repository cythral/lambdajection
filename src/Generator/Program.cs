using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;

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

            // Generation will be impossible without the build time assemblies,
            // aka generator dependencies.
            if (!buildTimeAssemblies.Any())
            {
                return;
            }

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

            // Dependencies other than the ones provided explicitly as build time assemblies,
            // the generator may use other dependencies in the restore packages path.
            // If we cannot access those, there is no point attempting generation.
            if (string.IsNullOrEmpty(additionalProbingPath))
            {
                return;
            }

            AssemblyLoadContext.Default.Resolving += (_, name) =>
            {
                var matchingFiles = from file in Directory.GetFiles(additionalProbingPath, $"{name.Name}.dll", SearchOption.AllDirectories)
                                    where file.Contains("netstandard") || file.Contains("net5.0") || file.Contains("net6.0") || file.Contains("net7.0")
                                    select file;

                foreach (var matchingFile in matchingFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(matchingFile);
                        if (assembly.GetName().Version >= name.Version)
                        {
                            return assembly;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                return null;
            };

            var host = new ProgramHost();
            host.Run(context);
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
    }
}
