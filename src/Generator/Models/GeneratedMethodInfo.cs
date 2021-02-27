using System;
using System.Reflection;

using Lambdajection.Framework;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator.Models
{
    internal class GeneratedMethodInfo
    {
        internal GeneratedAttribute GeneratedAttribute { get; set; }

        internal IMethodSymbol GeneratedMethod { get; set; }

        internal Location Location { get; set; }

#pragma warning disable IDE0046
        internal IMemberGenerator GetGenerator(InterfaceImplementationAnalyzer.Results analyzerResults, GenerationContext context)
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
