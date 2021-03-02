using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Lambdajection.Attributes;
using Lambdajection.Framework;
using Lambdajection.Framework.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lambdajection.CustomResource
{
    internal class RequiresReplacementGenerator : IMemberGenerator
    {
        private const string argumentName = "request";
        private readonly AnalyzerResults analyzerResults;
        private readonly TypeUtils typeUtils = new();
        private readonly GenerationContext context;

        public RequiresReplacementGenerator(
            AnalyzerResults analyzerResults,
            GenerationContext context
        )
        {
            this.analyzerResults = analyzerResults;
            this.context = context;
        }

        private enum ResourcePropertiesType
        {
            New = 0,
            Old = 1,
        }

        public MemberDeclarationSyntax GenerateMember()
        {
            var parameters = SeparatedList(new[]
                {
                    Parameter(
                        attributeLists: List<AttributeListSyntax>(),
                        modifiers: TokenList(),
                        type: ParseTypeName(analyzerResults.FlattenedInputType!.ToString()),
                        identifier: Identifier(argumentName),
                        @default: null
                    ),
                });

            var parameterList = ParameterList(Token(SyntaxKind.OpenParenToken), parameters, Token(SyntaxKind.CloseParenToken));
            var body = Block(GenerateBody());

            return MethodDeclaration(ParseTypeName("bool"), Identifier("RequiresReplacement"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(parameterList)
                .WithBody(body);
        }

        public IEnumerable<StatementSyntax> GenerateBody()
        {
            var condition = ParseExpression($"{argumentName}.RequestType != CustomResourceRequestType.Update");
            var returnExpression = ReturnStatement(ParseExpression("false"));

            yield return IfStatement(condition, returnExpression);

            foreach (var check in GenerateReplacementChecks())
            {
                yield return check;
            }

            yield return ReturnStatement(ParseExpression("false"));
        }

        public IEnumerable<StatementSyntax> GenerateReplacementChecks(INamedTypeSymbol? type = null, ExpressionSyntax? resourcePropertiesParent = null, ExpressionSyntax? oldResourcePropertiesParent = null)
        {
            type ??= (from member in analyzerResults.FlattenedInputType!.GetMembers("ResourceProperties").OfType<IPropertySymbol>() select member.Type as INamedTypeSymbol).First();

            var members = from member in type.GetMembers().OfType<IPropertySymbol>()
                          where member.DeclaredAccessibility == Accessibility.Public
                          select member;

            foreach (var member in members ?? ImmutableArray<IPropertySymbol>.Empty)
            {
                if (member.Type is INamedTypeSymbol memberType && !memberType.ContainingAssembly.Name.StartsWith("System"))
                {
                    ExpressionSyntax recurseResourcePropertiesParent = CreateResourcePropertiesAccessExpression(ResourcePropertiesType.New, resourcePropertiesParent, member.Name);
                    ExpressionSyntax recurseOldResourcePropertiesParent = CreateResourcePropertiesAccessExpression(ResourcePropertiesType.Old, resourcePropertiesParent, member.Name);

                    var memberValidations = GenerateReplacementChecks(memberType, recurseResourcePropertiesParent, recurseOldResourcePropertiesParent);
                    foreach (var validation in memberValidations)
                    {
                        yield return validation;
                    }

                    continue;
                }

                var validations = GenerateReplacementChecksForProperty(member, resourcePropertiesParent, oldResourcePropertiesParent);
                foreach (var validation in validations)
                {
                    yield return validation;
                }
            }
        }

        public IEnumerable<StatementSyntax> GenerateReplacementChecksForProperty(IPropertySymbol property, ExpressionSyntax? resourcePropertiesParent = null, ExpressionSyntax? oldResourcePropertiesParent = null)
        {
            var attributesQuery = from attr in property.GetAttributes()
                                  where attr.AttributeClass != null
                                    && typeUtils.IsSymbolEqualToType(attr.AttributeClass, typeof(UpdateRequiresReplacementAttribute))
                                  select 1;

            if (!attributesQuery.Any())
            {
                yield break;
            }

            var resourcePropertiesExpr = CreateResourcePropertiesAccessExpression(ResourcePropertiesType.New, resourcePropertiesParent, property.Name);
            var oldResourcePropertiesExpr = CreateResourcePropertiesAccessExpression(ResourcePropertiesType.Old, oldResourcePropertiesParent, property.Name);

            var condition = ParseExpression($"{resourcePropertiesExpr.ToFullString()} != {oldResourcePropertiesExpr.ToFullString()}");
            var returnStatement = ReturnStatement(ParseExpression("true"));

            yield return IfStatement(condition, returnStatement);
        }

        private ExpressionSyntax CreateResourcePropertiesAccessExpression(ResourcePropertiesType resourcePropertiesType, ExpressionSyntax? parentExpression, string name)
        {
            if (parentExpression == null)
            {
                var resourcePropertiesTypeName = resourcePropertiesType switch
                {
                    ResourcePropertiesType.Old => "OldResourceProperties",
                    ResourcePropertiesType.New => "ResourceProperties",
                    _ => "ResourceProperties",
                };

                parentExpression = ConditionalAccessExpression(
                    IdentifierName(argumentName),
                    MemberBindingExpression(Token(SyntaxKind.DotToken), IdentifierName(resourcePropertiesTypeName))
                );
            }

            return ConditionalAccessExpression(
                parentExpression,
                MemberBindingExpression(Token(SyntaxKind.DotToken), IdentifierName(name))
            );
        }
    }
}
