using System;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator
{
    public class GenerationSettings
    {
        public bool Nullable { get; init; }

        public bool GenerateEntrypoint { get; init; }

        public bool IncludeAmazonFactories { get; init; }

        public bool IncludeDefaultSerializer { get; init; }

        public static GenerationSettings FromContext(GeneratorExecutionContext context)
        {
            var referencedAssemblies = context.Compilation.ReferencedAssemblyNames;
            var includeAmazonFactories = referencedAssemblies.Any(assembly => assembly.Name == "AWSSDK.SecurityToken");
            var includeDefaultSerializer = referencedAssemblies.Any(assembly => assembly.Name == "Amazon.Lambda.Serialization.SystemTextJson");

            var options = context.AnalyzerConfigOptions.GlobalOptions;
            options.TryGetValue("build_property.GenerateLambdajectionEntrypoint", out var generateLambdajectionEntrypoint);
            options.TryGetValue("build_property.Nullable", out var nullable);

            generateLambdajectionEntrypoint ??= "false";
            nullable ??= "disable";

            return new GenerationSettings
            {
                Nullable = nullable.Equals("enable", StringComparison.OrdinalIgnoreCase),
                GenerateEntrypoint = generateLambdajectionEntrypoint.Equals("true", StringComparison.OrdinalIgnoreCase),
                IncludeAmazonFactories = includeAmazonFactories,
                IncludeDefaultSerializer = includeDefaultSerializer,
            };
        }
    }
}
