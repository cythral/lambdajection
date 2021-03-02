using System;

using Microsoft.CodeAnalysis;

#pragma warning disable CA1032, SA1600, CS8618, CS1591

namespace Lambdajection.Framework
{
    internal class GenerationFailureException : Exception
    {
        public string Id { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }

        public string Category { get; init; } = "Lambdajection";

        public Location Location { get; init; }

        public Diagnostic Diagnostic =>
            Diagnostic.Create(new DiagnosticDescriptor(Id, Title, Description, Category, DiagnosticSeverity.Error, true), Location);
    }
}
