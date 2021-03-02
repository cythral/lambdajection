using System.Collections.Generic;

using Microsoft.CodeAnalysis;

#pragma warning disable CA1032, SA1600, CS8618, CS1591

namespace Lambdajection.Framework
{
    internal class AnalyzerResults
    {
        public INamedTypeSymbol? InputType { get; set; }

        public string? InputTypeName { get; set; }

        public INamedTypeSymbol? InputEncapsulationType { get; set; }

        public string? InputEncapsulationTypeName { get; set; }

        public string? OutputTypeName { get; set; }

        public IEnumerable<GeneratedMethodInfo> GeneratedMethods { get; set; }

        public INamedTypeSymbol? FlattenedInputType => InputEncapsulationType != null && InputType != null
            ? InputEncapsulationType.ConstructedFrom.Construct(InputType)
            : InputType;
    }
}
