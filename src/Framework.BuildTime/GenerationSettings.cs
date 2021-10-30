using System;
using System.Linq;

using Microsoft.CodeAnalysis;

#pragma warning disable SA1600, CS8618

namespace Lambdajection.Framework
{
    internal class GenerationSettings
    {
        public bool Nullable { get; init; }

        public bool GenerateEntrypoint { get; init; }

        public bool IncludeAmazonFactories { get; init; }

        public bool IncludeDefaultSerializer { get; init; }

        public string[] BuildTimeAssemblies { get; init; }

        public string AssemblyName { get; init; }

        public string OutputPath { get; init; }

        public string TargetFrameworkVersion { get; init; }

        public bool EnableTracing { get; init; }

        public static GenerationSettings FromContext(GeneratorExecutionContext context)
        {
            var referencedAssemblies = context.Compilation.ReferencedAssemblyNames;
            var includeAmazonFactories = referencedAssemblies.Any(assembly => assembly.Name == "AWSSDK.SecurityToken");
            var includeDefaultSerializer = referencedAssemblies.Any(assembly => assembly.Name == "Amazon.Lambda.Serialization.SystemTextJson");

            var options = context.AnalyzerConfigOptions.GlobalOptions;
            options.TryGetValue("build_property.GenerateLambdajectionEntrypoint", out var generateLambdajectionEntrypoint);
            options.TryGetValue("build_property.Nullable", out var nullable);
            options.TryGetValue("build_property.LambdajectionBuildTimeAssemblies", out var buildTimeAssemblies);
            options.TryGetValue("build_property.AssemblyName", out var assemblyName);
            options.TryGetValue("build_property.OutputPath", out var outputPath);
            options.TryGetValue("build_property.TargetFrameworkVersion", out var targetFrameworkVersion);
            options.TryGetValue("build_property.StackDescription", out var stackDescription);
            options.TryGetValue("build_property.EnableLambdajectionTracing", out var enableTracing);

            generateLambdajectionEntrypoint ??= "false";
            nullable ??= "disable";
            enableTracing ??= "false";

            return new GenerationSettings
            {
                Nullable = nullable.Equals("enable", StringComparison.OrdinalIgnoreCase),
                GenerateEntrypoint = generateLambdajectionEntrypoint.Equals("true", StringComparison.OrdinalIgnoreCase),
                IncludeAmazonFactories = includeAmazonFactories,
                IncludeDefaultSerializer = includeDefaultSerializer,
                BuildTimeAssemblies = buildTimeAssemblies?.Split(";") ?? Array.Empty<string>(),
                AssemblyName = assemblyName ?? "Unknown",
                OutputPath = outputPath?.TrimEnd('/') ?? "Unknown",
                TargetFrameworkVersion = targetFrameworkVersion?.Replace("v", string.Empty) ?? "Unknown",
                EnableTracing = enableTracing.Equals("true", StringComparison.OrdinalIgnoreCase),
            };
        }
    }
}
