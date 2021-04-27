using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Framework;
using Lambdajection.Framework.Utils;
using Lambdajection.Generator.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    internal class UnitGenerator
    {
        private readonly ProgramContext programContext;
        private readonly UsingsGenerator usingsGenerator;
        private readonly TypeUtils typeUtils;

        public UnitGenerator(
            ProgramContext programContext,
            UsingsGenerator usingsGenerator,
            TypeUtils typeUtils
        )
        {
            this.programContext = programContext;
            this.usingsGenerator = usingsGenerator;
            this.typeUtils = typeUtils;
        }

        public void Generate()
        {
            try
            {
                var settings = GenerationSettings.FromContext(programContext.GeneratorExecutionContext);
                var syntaxTrees = programContext.GeneratorExecutionContext.Compilation.SyntaxTrees;
                var generations = from tree in syntaxTrees.AsParallel()
                                  let semanticModel = programContext.GeneratorExecutionContext.Compilation.GetSemanticModel(tree)

                                  from node in tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                                  from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                                  where IsAssignableTo(attr.AttributeClass, nameof(LambdaAttribute))

                                  let metadataAttributes = attr.AttributeClass?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                                  let startupType = GetAttributeArgument<INamedTypeSymbol>(attr, "Startup")!
                                  let generationContext = new GenerationContext
                                  {
                                      SourceGeneratorContext = programContext.GeneratorExecutionContext,
                                      Declaration = node,
                                      SyntaxTree = tree,
                                      Compilation = programContext.GeneratorExecutionContext.Compilation,
                                      SemanticModel = semanticModel,
                                      AttributeData = attr,
                                      LambdaHostAttribute = GetMetadataAttribute<LambdaHostAttribute>(metadataAttributes),
                                      LambdaInterfaceAttribute = GetMetadataAttribute<LambdaInterfaceAttribute>(metadataAttributes),
                                      CancellationToken = programContext.GeneratorExecutionContext.CancellationToken,
                                      StartupType = startupType,
                                      SerializerType = GetAttributeArgument<INamedTypeSymbol>(attr, "Serializer"),
                                      ConfigFactoryType = GetAttributeArgument<INamedTypeSymbol>(attr, "ConfigFactory"),
                                      RunnerMethodName = GetAttributeArgument<string>(attr, "RunnerMethod") ?? "Run",
                                      StartupTypeName = startupType.Name,
                                      StartupTypeDisplayName = startupType.ToDisplayString(),
                                      Settings = settings,
                                  }

                                  let unit = GenerateUnit(generationContext)
                                  let document = unit.NormalizeWhitespace().GetText(Encoding.UTF8)
                                  let name = node.Identifier.Text
                                  select (name, document);

                foreach (var (name, document) in generations)
                {
                    programContext.GeneratorExecutionContext.AddSource(name, document);
                }
            }
            catch (AggregateException e)
            {
                var failureExceptions = e.InnerExceptions.OfType<GenerationFailureException>();
                var nonFailureExceptions = e.InnerExceptions.Where(e => e is not GenerationFailureException);

                foreach (var failure in failureExceptions)
                {
                    programContext.GeneratorExecutionContext.ReportDiagnostic(failure.Diagnostic);
                }

                using var source = CancellationTokenSource.CreateLinkedTokenSource(programContext.GeneratorExecutionContext.CancellationToken);
                source.Cancel();

                if (nonFailureExceptions.Any())
                {
                    throw new AggregateException(nonFailureExceptions);
                }
            }
        }

        public TAttribute GetMetadataAttribute<TAttribute>(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass != null && typeUtils.IsSymbolEqualToType(attr.AttributeClass, typeof(TAttribute)))
                {
                    return AttributeFactory.Create<TAttribute>(attr);
                }
            }

            throw new GenerationFailureException
            {
                Id = "LJ0005",
                Title = "Unable to get Framework Metadata",
                Description = $"An error occurred while attempting to fetch metadata from {typeof(TAttribute).Name}",
                Location = Location.None,
            };
        }

        public static T? GetAttributeArgument<T>(AttributeData attributeData, string argName)
        {
            if (argName == "Startup")
            {
                return (T?)attributeData.ConstructorArguments[0].Value;
            }

            var query = from arg in attributeData.NamedArguments
                        where arg.Key == argName
                        select (T?)arg.Value.Value;

            return query.FirstOrDefault();
        }

        public static bool IsAssignableTo(INamedTypeSymbol? symbol, string assignableTo)
        {
            var baseType = symbol;

            while (baseType != null)
            {
                if (baseType.Name == assignableTo)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private CompilationUnitSyntax GenerateUnit(GenerationContext context)
        {
            var namespaceNode = (NamespaceDeclarationSyntax?)context.Declaration.Parent;
            var unitRoot = context.SyntaxTree.GetCompilationUnitRoot();
            var existingUsings = unitRoot.Usings.Select(x => x.WithoutTrivia().Name.ToString());
            var members = GenerateMembers(context);
            var usings = usingsGenerator.Generate(context);

            if (namespaceNode != null)
            {
                var namespacedMembers = NamespaceDeclaration(namespaceNode.Name, List<ExternAliasDirectiveSyntax>(), List<UsingDirectiveSyntax>(), members);
                members = List(new MemberDeclarationSyntax[] { namespacedMembers });
            }

            var codes = new ExpressionSyntax[] { ParseExpression("CA2007"), ParseExpression("CA1812"), ParseExpression("CS1591") };
            var ignoreWarningsTrivia = Trivia(PragmaWarningDirectiveTrivia(Token(DisableKeyword), SeparatedList(codes), true));
            return CompilationUnit(unitRoot.Externs, List(usings), unitRoot.AttributeLists, members)
                .WithLeadingTrivia(TriviaList(ignoreWarningsTrivia));
        }

        private SyntaxList<MemberDeclarationSyntax> GenerateMembers(GenerationContext generationContext)
        {
            var declaration = generationContext.Declaration;
            var namespaceName = declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0).Name;
            var className = declaration.Identifier.ValueText;
            var results = new InterfaceImplementationAnalyzer(typeUtils).Analyze(declaration, generationContext);

            var constructorArgs = from tree in generationContext.SourceGeneratorContext.Compilation.SyntaxTrees
                                  from constructor in tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                                  from parameter in constructor.ParameterList.Parameters
                                  select parameter;

            if (!generationContext.Settings.IncludeAmazonFactories && constructorArgs.Any())
            {
                foreach (var arg in constructorArgs)
                {
                    if (arg?.Type == null)
                    {
                        continue;
                    }

                    var semanticModel = generationContext.SourceGeneratorContext.Compilation.GetSemanticModel(arg.SyntaxTree);
                    var typeDefinition = semanticModel.GetTypeInfo(arg.Type).Type?.OriginalDefinition;
                    var qualifiedName = typeDefinition?.ContainingNamespace + "." + typeDefinition?.MetadataName + ", " + typeDefinition?.ContainingAssembly;

                    if (qualifiedName != typeof(IAwsFactory<>).AssemblyQualifiedName)
                    {
                        continue;
                    }

                    throw new GenerationFailureException
                    {
                        Id = "LJ0002",
                        Title = "Factories Not Enabled",
                        Description = "Add AWSSDK.SecurityToken as a dependency of your project to use AWS Factories.",
                        Location = Location.Create(declaration.SyntaxTree, declaration.Span),
                    };
                }
            }

            var scanner = new LambdaCompilationScanner(generationContext.SourceGeneratorContext.Compilation, generationContext.SourceGeneratorContext.Compilation.SyntaxTrees, $"{namespaceName}.{className}", generationContext.StartupTypeDisplayName);
            var scanResults = scanner.Scan();

            IEnumerable<MemberDeclarationSyntax> GenerateMembers()
            {
                var generator = new LambdaGenerator(results, generationContext, className!, scanResults, programContext);
                yield return generator.Generate();
            }

            var result = List(GenerateMembers());
            return result;
        }
    }
}
