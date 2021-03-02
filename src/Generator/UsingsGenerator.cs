using System.Collections.Generic;

using Lambdajection.Framework;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lambdajection.Generator
{
    internal class UsingsGenerator
    {
        private static readonly string[] DefaultUsings = new string[]
        {
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "System.IO",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Extensions",
            "Microsoft.Extensions.Configuration",
            "Amazon.Lambda.Core",
            "Lambdajection.Core",
        };

        private readonly string[] defaultUsings;

        public UsingsGenerator()
            : this(DefaultUsings)
        {
        }

        internal UsingsGenerator(string[] defaultUsings)
        {
            this.defaultUsings = defaultUsings;
        }

        public IEnumerable<UsingDirectiveSyntax> Generate(GenerationContext context)
        {
            var usingsToEmit = new HashSet<string>(defaultUsings);
            usingsToEmit.UnionWith(context.Usings);

            var unitRoot = context.SyntaxTree.GetCompilationUnitRoot();

            foreach (var @using in unitRoot.Usings)
            {
                usingsToEmit.Remove(@using.WithoutTrivia().Name.ToFullString());
                yield return @using.WithoutTrivia();
            }

            foreach (var @using in usingsToEmit)
            {
                var name = ParseName(@using);
                yield return UsingDirective(name);
            }
        }
    }
}
