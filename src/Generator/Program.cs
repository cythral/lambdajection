﻿using System;
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

            if (additionalProbingPath != null)
            {
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
            }

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
