using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    public class AwsFactoryGenerator
    {
        private readonly GenerationContext context;
        private readonly string interfaceName;
        private readonly string implementationName;
        private readonly string className;
        private readonly BaseTypeSyntax[] typeConstraints;

        public AwsFactoryGenerator(GenerationContext context, string service, string interfaceName, string implementationName)
        {
            this.context = context;
            this.interfaceName = interfaceName;
            this.implementationName = implementationName;
            className = $"{service}Factory";
            typeConstraints = new[] { SimpleBaseType(ParseTypeName($"IAwsFactory<{interfaceName}>")) };
        }

        public ClassDeclarationSyntax Generate()
        {
            return ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GenerateStsClientField(),
                    GenerateConstructor(),
                    GenerateCreateMethod()
                );
        }

        public MemberDeclarationSyntax GenerateConstructor()
        {
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IAmazonSecurityTokenService"), Identifier("stsClient"), null),
            });

            static IEnumerable<StatementSyntax> GenerateBody()
            {
                yield return ParseStatement("this.stsClient = stsClient;");
            }

            return ConstructorDeclaration(className!)
                .WithModifiers(TokenList(Token(PublicKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }

        public MemberDeclarationSyntax GenerateCreateMethod()
        {
            var roleArnTypeName = context.Settings.Nullable ? "string?" : "string";
            var roleArnDefaultValue = ParseExpression("null");
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName(roleArnTypeName), Identifier("roleArn"), EqualsValueClause(roleArnDefaultValue)),
            });

            IEnumerable<StatementSyntax> GenerateBody()
            {
                yield return IfStatement(
                    ParseExpression("roleArn != null"),
                    Block(
                        ParseStatement("var request = new AssumeRoleRequest { RoleArn = roleArn, RoleSessionName = \"lambdajection-assume-role\" };"),
                        ParseStatement("var response = await stsClient.AssumeRoleAsync(request);"),
                        ParseStatement("var credentials = response.Credentials;"),
                        ParseStatement($"return new {implementationName}(credentials);")
                    )
                );

                yield return ParseStatement($"return new {implementationName}();");
            }

            return MethodDeclaration(ParseTypeName($"Task<{interfaceName}>"), "Create")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(AsyncKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateBody()));
        }

        public static MemberDeclarationSyntax GenerateStsClientField()
        {
            var attributes = List<AttributeListSyntax>();
            var modifiers = TokenList(Token(PrivateKeyword));
            var type = ParseTypeName("IAmazonSecurityTokenService");

            var variables = new VariableDeclaratorSyntax[] { VariableDeclarator("stsClient") };
            var variable = VariableDeclaration(type)
                .WithVariables(SeparatedList(variables));

            return FieldDeclaration(attributes, modifiers, variable);
        }
    }
}
