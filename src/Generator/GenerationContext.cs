using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public class GenerationContext
    {
        public ClassDeclarationSyntax Declaration { get; init; }

        public SyntaxTree SyntaxTree { get; init; }

        public SemanticModel SemanticModel { get; init; }

        public AttributeData AttributeData { get; init; }

        public GeneratorExecutionContext SourceGeneratorContext { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public INamedTypeSymbol StartupType { get; init; }

        public INamedTypeSymbol? SerializerType { get; set; }

        public INamedTypeSymbol? ConfigFactoryType { get; set; }

        public string StartupTypeName { get; set; }

        public string StartupTypeDisplayName { get; set; }

        public string RunnerMethodName { get; set; }

        public HashSet<string> Usings { get; } = new HashSet<string>();

        public GenerationSettings Settings { get; init; }
    }
}
