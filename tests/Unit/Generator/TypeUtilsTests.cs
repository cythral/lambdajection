using System.Linq;

using Amazon.Runtime;

using AutoFixture;

// using AutoFixture.AutoNSubstitute;
using FluentAssertions;

using Lambdajection.Generator.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// using NSubstitute;
using NUnit.Framework;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lambdajection.Generator.Tests
{
    [Category("Unit")]
    public class TypeUtilsTests
    {
        [TestFixture, Category("Unit")]
        public class IsSymbolEqualToTypeTests
        {
            private const string Template = @"
using System;
using Amazon.Runtime;

class TypeUtilsTests
{
    public static IAmazonService Service { get; }
    public static TypeUtilsTests TypeUtilsTestsInstance { get; }
    public static Lambdajection.Generator.TypeUtilsTests TypeUtilsTestsInstance2 { get; }

    public static void Main(string[] args)
    {
    }
}

namespace TestNamespace
{
    class TypeUtilsTests
    {
    }
}

namespace Lambdajection.Generator
{
    class TypeUtilsTests
    {
    }
}
            ";

            public static void Customize(Fixture fixture)
            {
                var amazonReference = MetadataReference.CreateFromFile(typeof(IAmazonService).Assembly.Location);
                var systemReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("test")
                    .AddSyntaxTrees(ParseSyntaxTree(Template))
                    .AddReferences(amazonReference)
                    .AddReferences(systemReference);

                var tree = compilation.SyntaxTrees.First();
                var semanticModel = compilation.GetSemanticModel(tree);
                var assembly = compilation.GenerateAssembly();

                fixture.Register(() => semanticModel);
                fixture.Register(() => tree);
                fixture.Register(() => compilation);
                fixture.Register(() => assembly);
            }

            [Test, Auto(customizer: typeof(IsSymbolEqualToTypeTests))]
            public void ShouldReturnTrueIfSymbolRepresentsType(
                SyntaxTree tree,
                SemanticModel semanticModel,
                [Target] TypeUtils typeUtils
            )
            {
                var serviceProp = (from prop in tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>()
                                   where prop.Identifier.Text == "Service"
                                   select prop).First();

                var serviceType = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(serviceProp)!.Type!;

                var result = typeUtils.IsSymbolEqualToType(serviceType!, typeof(IAmazonService));
                result.Should().BeTrue();
            }

            [Test, Auto(customizer: typeof(IsSymbolEqualToTypeTests))]
            public void ShouldReturnFalseIfNameDoesntMatch(
                SyntaxTree tree,
                SemanticModel semanticModel,
                [Target] TypeUtils typeUtils
            )
            {
                var instanceProp = (from prop in tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>()
                                    where prop.Identifier.Text == "TypeUtilsTestsInstance"
                                    select prop).First();

                var instanceType = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(instanceProp)!.Type!;

                var result = typeUtils.IsSymbolEqualToType(instanceType!, typeof(IAmazonService));
                result.Should().BeFalse();
            }

            [Test, Auto(customizer: typeof(IsSymbolEqualToTypeTests))]
            public void ShouldReturnFalseIfNamespaceDoesntMatch(
                GenerateAssemblyResult result,
                SyntaxTree tree,
                SemanticModel semanticModel,
                [Target] TypeUtils typeUtils
            )
            {
                var (assembly, _) = result;
                var instanceProp = (from prop in tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>()
                                    where prop.Identifier.Text == "TypeUtilsTestsInstance"
                                    select prop).First();

                var instanceType = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(instanceProp)!.Type!;
                var namespacedType = assembly.GetType("TestNamespace.TypeUtilsTests", true)!;

                var isEqual = typeUtils.IsSymbolEqualToType(instanceType!, namespacedType);
                isEqual.Should().BeFalse();
            }

            [Test, Auto(customizer: typeof(IsSymbolEqualToTypeTests))]
            public void ShouldReturnFalseIfAssemblyDoesntMatch(
                GenerateAssemblyResult result,
                SyntaxTree tree,
                SemanticModel semanticModel,
                [Target] TypeUtils typeUtils
            )
            {
                var (assembly, _) = result;
                var instanceProp = (from prop in tree.GetRoot().DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>()
                                    where prop.Identifier.Text == "TypeUtilsTestsInstance2"
                                    select prop).First();

                var instanceType = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(instanceProp)!.Type!;

                var isEqual = typeUtils.IsSymbolEqualToType(instanceType!, typeof(TypeUtilsTests));
                isEqual.Should().BeFalse();
            }
        }
    }
}
