using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.Framework;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NSubstitute;

using NUnit.Framework;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lambdajection.Generator.Tests
{
    [Category("Unit")]
    public class UsingsGeneratorTests
    {
        [Test, Auto]
        public void Generate_ShouldEmitUsingOnlyOnce_IfIsDefault_AndAddedAtRuntime(
            string usingToEmit,
            [Substitute] SyntaxTree syntaxTree
        )
        {
            var defaultUsings = new[] { usingToEmit };
            var generator = new UsingsGenerator(defaultUsings);

            var root = CompilationUnit().WithUsings(List<UsingDirectiveSyntax>());
            var context = new GenerationContext { SyntaxTree = syntaxTree };

            syntaxTree.GetRoot().Returns(root);
            context.Usings.Add(usingToEmit);

            var result = generator.Generate(context);
            result.Should().ContainSingle(directive => directive.Name.ToFullString() == usingToEmit);
        }

        [Test, Auto]
        public void Generate_ShouldEmitUsingOnlyOnce_IfAlreadyInSourceDocument_AndAddedAtRuntime(
            string usingToEmit,
            [Substitute] SyntaxTree syntaxTree
        )
        {
            var generator = new UsingsGenerator();
            var usings = new[] { UsingDirective(ParseName(usingToEmit)) };
            var root = CompilationUnit().WithUsings(List(usings));
            var context = new GenerationContext { SyntaxTree = syntaxTree };

            syntaxTree.GetRoot().Returns(root);
            context.Usings.Add(usingToEmit);

            var result = generator.Generate(context);
            result.Should().ContainSingle(directive => directive.Name.ToFullString() == usingToEmit);
        }

        [Test, Auto]
        public void Generate_ShouldEmitUsingOnlyOnce_IfIsDefault_AndAlreadyInSourceDocument(
            string usingToEmit,
            [Substitute] SyntaxTree syntaxTree
        )
        {
            var defaultUsings = new[] { usingToEmit };
            var usings = new[] { UsingDirective(ParseName(usingToEmit)) };
            var generator = new UsingsGenerator(defaultUsings);
            var root = CompilationUnit().WithUsings(List(usings));
            var context = new GenerationContext { SyntaxTree = syntaxTree };

            syntaxTree.GetRoot().Returns(root);

            var result = generator.Generate(context);
            result.Should().ContainSingle(directive => directive.Name.ToFullString() == usingToEmit);
        }
    }
}
