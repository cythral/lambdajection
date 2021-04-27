using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    public class OptionsDecryptorGenerator
    {
        private readonly OptionClass optionClass;
        private readonly ClassDeclarationSyntax classDeclaration;
        private readonly string optionClassName;
        private readonly NamespaceDeclarationSyntax namespaceName;
        private readonly string fullName;
        private static readonly BaseTypeSyntax[] TypeConstraints = new[] { SimpleBaseType(ParseTypeName($"ILambdaInitializationService")) };

        public OptionsDecryptorGenerator(OptionClass optionClass)
        {
            this.optionClass = optionClass;
            classDeclaration = optionClass.ClassDeclaration;
            optionClassName = classDeclaration.Identifier.ValueText;
            namespaceName = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0);
            fullName = namespaceName.Name + "." + optionClassName;
        }

        public ClassDeclarationSyntax Generate()
        {
            var decryptMethods = optionClass.EncryptedProperties.Select(prop => GenerateDecryptPropertyMethod(prop));

            return ClassDeclaration(optionClass.ConfigSectionName + "Decryptor")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(TypeConstraints)))
                .AddMembers(
                    GeneratePrivateField("IDecryptionService", "decryptionService"),
                    GeneratePrivateField($"{optionClassName}", "options"),
                    GenerateConstructor(),
                    GenerateInitializeMethod()
                )
                .AddMembers(decryptMethods.ToArray());
        }

        public MemberDeclarationSyntax GenerateConstructor()
        {
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IDecryptionService"), Identifier("decryptionService"), null),
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName($"IOptions<{optionClassName}>"), Identifier("options"), null),
            });

            static IEnumerable<StatementSyntax> GenerateBody()
            {
                yield return ParseStatement("this.decryptionService = decryptionService;");
                yield return ParseStatement("this.options = options.Value;");
            }

            return ConstructorDeclaration($"{optionClass.ConfigSectionName}Decryptor")
                .WithModifiers(TokenList(Token(PublicKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }

        public MemberDeclarationSyntax GenerateInitializeMethod()
        {
            static IEnumerable<ExpressionSyntax> GenerateDecryptPropertyCalls(IEnumerable<string> properties)
            {
                foreach (var prop in properties)
                {
                    yield return ParseExpression($"Decrypt{prop}(cancellationToken)");
                }
            }

            IEnumerable<StatementSyntax> GenerateBody()
            {
                var calls = GenerateDecryptPropertyCalls(optionClass.EncryptedProperties);
                var initializerList = SeparatedList(calls);
                var initializer = InitializerExpression(ArrayInitializerExpression, initializerList);
                var creationExpression = ObjectCreationExpression(ParseTypeName("Task[]"), null, initializer);

                var args = SeparatedList(new ArgumentSyntax[] { Argument(creationExpression) });
                var argList = ArgumentList(args);
                var whenAllExpression = InvocationExpression(ParseExpression("Task.WhenAll"), argList);
                var awaitExpression = AwaitExpression(whenAllExpression);

                yield return ExpressionStatement(awaitExpression);
            }

            var defaultValue = EqualsValueClause(ParseToken("="), ParseExpression("default"));
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("CancellationToken"), ParseToken("cancellationToken"), defaultValue),
            });

            return MethodDeclaration(ParseTypeName("Task"), "Initialize")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(AsyncKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }

        public static MemberDeclarationSyntax GeneratePrivateField(string typeName, string name)
        {
            var attributes = List<AttributeListSyntax>();
            var modifiers = TokenList(Token(PrivateKeyword));
            var type = ParseTypeName(typeName);

            var variables = new VariableDeclaratorSyntax[] { VariableDeclarator(name) };
            var variable = VariableDeclaration(type)
                .WithVariables(SeparatedList(variables));

            return FieldDeclaration(attributes, modifiers, variable);
        }

        public static MemberDeclarationSyntax GenerateDecryptPropertyMethod(string prop)
        {
            IEnumerable<StatementSyntax> GenerateBody()
            {
                yield return ParseStatement($"options.{prop} = await decryptionService.Decrypt(options.{prop}, cancellationToken);");
            }

            var defaultValue = EqualsValueClause(ParseToken("="), ParseExpression("default"));
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("CancellationToken"), ParseToken("cancellationToken"), defaultValue),
            });

            return MethodDeclaration(ParseTypeName("Task"), $"Decrypt{prop}")
                .WithModifiers(TokenList(Token(PrivateKeyword), Token(AsyncKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }
    }
}
