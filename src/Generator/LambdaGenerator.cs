using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Cythral.CodeGeneration.Roslyn;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lambdajection.Generator
{
    public class LambdaGenerator : IRichCodeGenerator
    {
        private AttributeData attributeData;
        private string startupType;
        private List<string> usings = new List<string>
        {
            "System.Threading.Tasks",
            "System.IO",
            "Microsoft.Extensions.DependencyInjection",
            "Amazon.Lambda.Core",
            "Lambdajection.Core"
        };

        public LambdaGenerator(AttributeData attributeData)
        {
            this.attributeData = attributeData;
            this.startupType = (from arg in attributeData.NamedArguments
                                where arg.Key == "Startup"
                                select ((INamedTypeSymbol?)arg.Value.Value)?.Name).FirstOrDefault();
        }

        public IEnumerable<UsingDirectiveSyntax> GenerateUsings(IEnumerable<UsingDirectiveSyntax> exclusions)
        {
            foreach (var exclusion in exclusions)
            {
                usings.Remove(exclusion.Name.ToString());
            }

            foreach (var use in usings)
            {
                yield return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(use));
            }
        }

        public async Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var processingNode = (ClassDeclarationSyntax)context.ProcessingNode;
            var namespaceNode = (NamespaceDeclarationSyntax)processingNode.Parent;

            var members = await GenerateAsync(context, progress, cancellationToken);
            var namespacedMembers = SyntaxFactory.NamespaceDeclaration(namespaceNode.Name, SyntaxFactory.List<ExternAliasDirectiveSyntax>(), SyntaxFactory.List<UsingDirectiveSyntax>(), members);
            var namespacedMembersList = new List<MemberDeclarationSyntax> { namespacedMembers };


            return new RichGenerationResult
            {
                Usings = SyntaxFactory.List(GenerateUsings(context.CompilationUnitUsings)),
                Members = SyntaxFactory.List(namespacedMembersList),
            };
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var declaration = (ClassDeclarationSyntax)context.ProcessingNode;
            var className = declaration.Identifier.ValueText;
            var handleMember = (from member in declaration!.Members
                                where (member as MethodDeclarationSyntax)?.Identifier.ValueText == "Handle"
                                select (MethodDeclarationSyntax)member).FirstOrDefault();

            if (handleMember == null)
            {
                throw new Exception("Lambda must implement handle method");
            }

            var inputParameter = handleMember?.ParameterList.Parameters[0];
            var contextParameter = handleMember?.ParameterList.Parameters[1];
            var inputType = inputParameter?.Type?.ToString() ?? "";
            var returnType = handleMember?.ReturnType.ChildNodes().ElementAt(0).ChildNodes().ElementAt(0);
            var typeConstraints = new List<BaseTypeSyntax> { SimpleBaseType(ParseTypeName($"ILambda<{inputType},{returnType}>")) };

            IEnumerable<MemberDeclarationSyntax> GeneratePartialClass()
            {
                var partialClass = SyntaxFactory
                .ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(SyntaxKind.PartialKeyword))
                .WithIdentifier(Identifier(className))
                .AddMembers(
                    GenerateRunMethod()
                );

                yield return partialClass;
            }

            MemberDeclarationSyntax GenerateRunMethod()
            {
                var modifiers = new List<SyntaxToken>
                {
                    Token(PublicKeyword),
                    Token(StaticKeyword),
                    Token(AsyncKeyword),
                };

                return MethodDeclaration(ParseTypeName($"Task<{returnType}>"), "Run")
                    .WithModifiers(TokenList(modifiers))
                    .WithParameterList(handleMember!.ParameterList)
                    .WithBody(Block(GenerateRunMethodBody()));
            }

            IEnumerable<StatementSyntax> GenerateRunMethodBody()
            {
                yield return ParseStatement($"var host = new LambdaHost<{className}, {inputType}, {returnType}, {startupType}>();");
                yield return ParseStatement($"return await host.Run({inputParameter!.Identifier.ValueText}, {contextParameter!.Identifier.ValueText});");
            }

            var result = GeneratePartialClass();
            var list = SyntaxFactory.List(result);
            return Task.FromResult(list);
        }
    }
}
