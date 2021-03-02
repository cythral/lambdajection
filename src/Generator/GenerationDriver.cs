using System;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    [Generator]
    public class GenerationDriver : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var options = context.AnalyzerConfigOptions.GlobalOptions;
            options.TryGetValue("build_property.LambdajectionBuildTimeAssemblies", out var buildTimeAssembliesString);

            var buildTimeAssemblies = buildTimeAssembliesString?.Split(',') ?? Array.Empty<string>();
            foreach (var buildTimeAssembly in buildTimeAssemblies)
            {
                if (!string.IsNullOrEmpty(buildTimeAssembly))
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(buildTimeAssembly);
                }
            }

            new UnitGenerator(context).Generate();
        }
    }
}
