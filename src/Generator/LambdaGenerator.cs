using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Cythral.CodeGeneration.Roslyn;

using Lambdajection.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lambdajection.Generator
{
    public class LambdaGenerator : IRichCodeGenerator
    {
        private readonly AttributeData attributeData;
        private readonly INamedTypeSymbol startupType;
        private readonly string startupTypeName;
        private readonly string[] usings = new string[]
        {
            "System.Threading.Tasks",
            "System.IO",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Configuration",
            "Amazon.Lambda.Core",
            "Lambdajection.Core"
        };

        public LambdaGenerator(AttributeData attributeData)
        {
            this.attributeData = attributeData;
            this.startupType = (from arg in attributeData.NamedArguments
                                where arg.Key == "Startup"
                                select (INamedTypeSymbol)arg.Value.Value!).FirstOrDefault();

            this.startupTypeName = this.startupType.Name;
        }

        public IEnumerable<UsingDirectiveSyntax> GenerateUsings(IEnumerable<UsingDirectiveSyntax> exclusions)
        {
            var usingsWithoutExclusions = new List<string>(usings);

            foreach (var exclusion in exclusions)
            {
                usingsWithoutExclusions.Remove(exclusion.Name.ToString());
            }

            foreach (var use in usingsWithoutExclusions)
            {
                yield return UsingDirective(ParseName(use));
            }
        }

        public async Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken = default)
        {
            var processingNode = (ClassDeclarationSyntax)context.ProcessingNode;
            var namespaceNode = (NamespaceDeclarationSyntax)processingNode.Parent;

            var members = await GenerateAsync(context, progress, cancellationToken);
            var namespacedMembers = NamespaceDeclaration(namespaceNode.Name, List<ExternAliasDirectiveSyntax>(), List<UsingDirectiveSyntax>(), members);
            var namespacedMembersList = new MemberDeclarationSyntax[] { namespacedMembers };

            return new RichGenerationResult
            {
                Usings = List(GenerateUsings(context.CompilationUnitUsings)),
                Members = List(namespacedMembersList),
            };
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken = default)
        {
            var declaration = (ClassDeclarationSyntax)context.ProcessingNode;
            var namespaceName = declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0).Name;
            var className = declaration.Identifier.ValueText;
            var handleMember = (from member in declaration!.Members
                                where (member as MethodDeclarationSyntax)?.Identifier.ValueText == "Handle"
                                select (MethodDeclarationSyntax)member).FirstOrDefault();

            if (handleMember == null)
            {
                var descriptor = new DiagnosticDescriptor("LJ0001", "Handle Method Not Implemented", "Implement the Handle method to provide Lambda Function Handler code.", "Lambdajection", DiagnosticSeverity.Error, true);
                var diagnostic = Diagnostic.Create(descriptor, Location.Create(declaration.SyntaxTree, declaration.Span));
                progress.Report(diagnostic);

                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                source.Cancel();

                throw new Exception("Lambda must implement handle method");
            }

            var optionClasses = from tree in context.Compilation.SyntaxTrees
                                let semanticModel = context.Compilation.GetSemanticModel(tree)

                                from node in tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                                from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()

                                where attr.AttributeClass?.Name == nameof(LambdaOptionsAttribute) &&
                                    ((INamedTypeSymbol)attr.ConstructorArguments[0].Value!).ToDisplayString() == $"{namespaceName}.{className}"


                                select ((string)attr.ConstructorArguments[1].Value!, node);

            IEnumerable<MemberDeclarationSyntax> GenerateMembers()
            {
                yield return GenerateLambda(className!, handleMember!, optionClasses!);
            }

            var result = List(GenerateMembers());
            return Task.FromResult(result);
        }

        public ClassDeclarationSyntax GenerateLambda(string className, MethodDeclarationSyntax handleMethod, IEnumerable<(string, ClassDeclarationSyntax)> optionClasses)
        {
            var inputParameter = handleMethod.ParameterList.Parameters[0];
            var contextParameter = handleMethod.ParameterList.Parameters[1];
            var inputType = inputParameter?.Type?.ToString() ?? "";
            var returnType = handleMethod.ReturnType.ChildNodes().ElementAt(0).ChildNodes().ElementAt(0);
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"ILambda<{inputType},{returnType}>")) };

            MemberDeclarationSyntax GenerateRunMethod()
            {
                var modifiers = new SyntaxToken[]
                {
                    Token(PublicKeyword),
                    Token(StaticKeyword),
                    Token(AsyncKeyword),
                };

                return MethodDeclaration(ParseTypeName($"Task<{returnType}>"), "Run")
                    .WithModifiers(TokenList(modifiers))
                    .WithParameterList(handleMethod!.ParameterList)
                    .WithBody(Block(GenerateRunMethodBody()));
            }

            IEnumerable<StatementSyntax> GenerateRunMethodBody()
            {
                yield return ParseStatement($"var host = new LambdaHost<{className}, {inputType}, {returnType}, {startupTypeName}, OptionsConfigurator>();");
                yield return ParseStatement($"return await host.Run({inputParameter!.Identifier.ValueText}, {contextParameter!.Identifier.ValueText});");
            }

            return ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(PartialKeyword))
                .AddMembers(
                    GenerateRunMethod(),
                    GenerateOptionsConfigurator(optionClasses)
                );
        }

        public static ClassDeclarationSyntax GenerateOptionsConfigurator(IEnumerable<(string, ClassDeclarationSyntax)> optionClasses)
        {
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName("ILambdaOptionsConfigurator")) };
            var publicModifiersList = new SyntaxToken[] { Token(PublicKeyword) };
            var configureMethodParameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IConfiguration"), Identifier("configuration"), null),
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IServiceCollection"), Identifier("services"), null),
            });

            IEnumerable<StatementSyntax> GenerateConfigureMethodBody()
            {
                foreach (var (sectionName, optionClass) in optionClasses)
                {
                    var optionClassName = optionClass.Identifier.ValueText;
                    var namespac = optionClass.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0);
                    var fullName = namespac.Name + "." + optionClassName;

                    yield return ParseStatement($"services.Configure<{fullName}>(configuration.GetSection(\"{sectionName}\"));");
                }
            }

            MemberDeclarationSyntax GenerateConfigureMethod()
            {
                return MethodDeclaration(ParseTypeName("void"), "ConfigureOptions")
                    .WithModifiers(TokenList(publicModifiersList))
                    .WithParameterList(ParameterList(configureMethodParameters))
                    .WithBody(Block(GenerateConfigureMethodBody()));
            };

            return ClassDeclaration("OptionsConfigurator")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GenerateConfigureMethod()
                );
        }
    }
}
