using System;
using System.Reflection;

using Microsoft.CodeAnalysis;

#pragma warning disable CA1032, SA1600, CS8618, CS1591

namespace Lambdajection.Framework
{
    internal class GeneratedMethodInfo
    {
        public GeneratedAttribute GeneratedAttribute { get; set; }

        public IMethodSymbol GeneratedMethod { get; set; }

        public Location Location { get; set; }

#pragma warning disable IDE0046
        public IMemberGenerator GetGenerator(AnalyzerResults analyzerResults, GenerationContext context)
        {
            var assembly = Assembly.Load(GeneratedAttribute.AssemblyName);
            var type = assembly?.GetType(GeneratedAttribute.TypeName);

            if (type == null)
            {
                throw new GenerationFailureException
                {
                    Id = "LJ0004",
                    Title = "Member Generator Not Found",
                    Description = $"Lambdajection was unable to load member generator: {GeneratedAttribute.TypeName} from {GeneratedAttribute.AssemblyName}",
                    Location = Location,
                };
            }

            return (IMemberGenerator)Activator.CreateInstance(type, new object[] { analyzerResults, context });
        }
    }
}
