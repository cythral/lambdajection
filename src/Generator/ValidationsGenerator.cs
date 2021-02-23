using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lambdajection.Generator
{
    public class ValidationsGenerator
    {
        private const string argumentName = "input";
        private readonly INamedTypeSymbol typeToValidate;
        private readonly GenerationContext context;

        public ValidationsGenerator(
            INamedTypeSymbol typeToValidate,
            GenerationContext context
        )
        {
            this.typeToValidate = typeToValidate;
            this.context = context;
        }

        public MemberDeclarationSyntax GenerateValidationMethod()
        {
            var validations = GenerateValidations();

            context.Usings.Add("System.ComponentModel.DataAnnotations");
            var parameters = SeparatedList(new[]
                {
                    Parameter(
                        attributeLists: List<AttributeListSyntax>(),
                        modifiers: TokenList(),
                        type: ParseTypeName(typeToValidate.ToString()),
                        identifier: Identifier(argumentName),
                        @default: null
                    ),
                });

            var parameterList = ParameterList(Token(SyntaxKind.OpenParenToken), parameters, Token(SyntaxKind.CloseParenToken));
            var body = Block(validations);

            return MethodDeclaration(ParseTypeName("void"), Identifier("Validate"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(parameterList)
                .WithBody(body);
        }

        public IEnumerable<StatementSyntax> GenerateValidations(INamedTypeSymbol? typeToValidate = null, ExpressionSyntax? parentExpression = null)
        {
            typeToValidate ??= this.typeToValidate;

            var members = from member in typeToValidate.GetMembers().OfType<IPropertySymbol>()
                          where member.DeclaredAccessibility == Accessibility.Public
                          select member;

            foreach (var member in members ?? ImmutableArray<IPropertySymbol>.Empty)
            {
                if (member.Type is INamedTypeSymbol memberType && !memberType.ContainingAssembly.Name.StartsWith("System"))
                {
                    ExpressionSyntax newParentExpression = parentExpression == null
                        ? ConditionalAccessExpression(
                            IdentifierName(argumentName),
                            MemberBindingExpression(Token(SyntaxKind.DotToken), IdentifierName(member.Name))
                         )
                        : ConditionalAccessExpression(
                            parentExpression,
                            MemberBindingExpression(Token(SyntaxKind.DotToken), IdentifierName(member.Name))
                         );

                    var memberValidations = GenerateValidations(memberType, newParentExpression);
                    foreach (var validation in memberValidations)
                    {
                        yield return validation;
                    }
                }

                var validations = GenerateValidationsForProperty(member, parentExpression);
                foreach (var validation in validations)
                {
                    yield return validation;
                }
            }
        }

        public IEnumerable<StatementSyntax> GenerateValidationsForProperty(IPropertySymbol property, ExpressionSyntax? parentExpresssion = null)
        {
            var validationAttributes = from attr in property.GetAttributes()
                                       where attr.AttributeClass?.BaseType?.Name == nameof(ValidationAttribute)
                                       select attr;

            parentExpresssion ??= IdentifierName(argumentName);
            var propName = IdentifierName(property.Name);
            var propertyAccessor = ConditionalAccessExpression(
                parentExpresssion,
                MemberBindingExpression(Token(SyntaxKind.DotToken), propName)
            );

            foreach (var attribute in validationAttributes)
            {
                var attributeConstructorArgs = from arg in attribute.ConstructorArguments
                                               let value = ParseExpression(arg.ToCSharpString())
                                               select Argument(value);

                var initializerExpressions = from arg in attribute.NamedArguments
                                             select (ExpressionSyntax)AssignmentExpression(
                                                 SyntaxKind.SimpleAssignmentExpression,
                                                 IdentifierName(arg.Key),
                                                 Token(SyntaxKind.EqualsToken),
                                                 ParseExpression(arg.Value.ToCSharpString())
                                             );
                var initializerExpression = InitializerExpression(SyntaxKind.ObjectInitializerExpression, Token(SyntaxKind.OpenBraceToken), SeparatedList(initializerExpressions), Token(SyntaxKind.CloseBraceToken));
                var attributeArgList = ArgumentList(SeparatedList(attributeConstructorArgs.ToArray()));
                var attributeInstance = ObjectCreationExpression(Token(SyntaxKind.NewKeyword), IdentifierName(attribute.AttributeClass!.Name), attributeArgList, initializerExpression);
                var parenthesizedAttributeInstance = ParenthesizedExpression(attributeInstance);

                var validateMethodName = IdentifierName(nameof(ValidationAttribute.Validate));
                var methodBeingInvoked = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, parenthesizedAttributeInstance, validateMethodName);
                var nameExpression = LiteralExpression(SyntaxKind.StringLiteralExpression, ParseToken($"\"{property.Name}\""));

                var args = SeparatedList(new[] { Argument(propertyAccessor), Argument(nameExpression) });
                var argList = ArgumentList(Token(SyntaxKind.OpenParenToken), args, Token(SyntaxKind.CloseParenToken));
                var invocation = InvocationExpression(methodBeingInvoked, argList);

                yield return ExpressionStatement(invocation);
            }
        }
    }
}
