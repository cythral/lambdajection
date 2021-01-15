using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    public class LambdaGenerator
    {
        private readonly GenerationContext context;
        private readonly string className;
        private readonly LambdaCompilationScanResult scanResults;
        private readonly ParameterSyntax inputParameter;
        private readonly string inputType;
        private readonly SyntaxNode returnType;
        private readonly BaseTypeSyntax[] typeConstraints;

        public LambdaGenerator(
            GenerationContext context,
            string className,
            MethodDeclarationSyntax handleMethod,
            LambdaCompilationScanResult scanResults
        )
        {
            this.context = context;
            this.className = className;
            this.scanResults = scanResults;
            inputParameter = handleMethod.ParameterList.Parameters[0];
            inputType = inputParameter?.Type?.ToString() ?? string.Empty;
            returnType = handleMethod.ReturnType.ChildNodes().ElementAt(0).ChildNodes().ElementAt(0);
            typeConstraints = new[] { SimpleBaseType(ParseTypeName($"ILambda<{inputType},{returnType}>")) };
        }

        public ClassDeclarationSyntax Generate()
        {
            var configuratorGenerator = new ConfiguratorGenerator(context, scanResults);
            var configurator = configuratorGenerator.Generate();
            var result = ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(PartialKeyword))
                .AddMembers(
                    GenerateRunnerMethod(),
                    configurator);

            if (context.Settings.GenerateEntrypoint)
            {
                result = result.AddMembers(GenerateMainMethod());
            }

            return result;
        }

        public string? GetSerializerName()
        {
            var result = context.SerializerType?.Name;
            result ??= context.Settings.IncludeDefaultSerializer ? "DefaultLambdaJsonSerializer" : null;
            return result;
        }

        public string? GetSerializerNamespace()
        {
            var result = context.SerializerType?.ContainingNamespace?.ToString();
            result ??= context.Settings.IncludeDefaultSerializer ? "Amazon.Lambda.Serialization.SystemTextJson" : null;
            return result;
        }

        public MemberDeclarationSyntax GenerateRunnerMethod()
        {
            var modifiers = new SyntaxToken[]
            {
                Token(PublicKeyword),
                Token(StaticKeyword),
                Token(AsyncKeyword),
            };

            var parameters = SeparatedList(new ParameterSyntax[]
            {
                inputParameter!,
                Parameter(
                    attributeLists: List<AttributeListSyntax>(),
                    modifiers: TokenList(),
                    type: ParseTypeName("ILambdaContext"),
                    identifier: ParseToken("context"),
                    @default: default
                ),
            });

            var parameterList = ParameterList(parameters);
            var body = Block(GenerateRunnerMethodBody());
            var method = MethodDeclaration(ParseTypeName($"Task<{returnType}>"), context.RunnerMethodName)
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(parameterList)
                .WithBody(body);

            var serializerName = GetSerializerName();
            var serializerNamespace = GetSerializerNamespace();

            if (serializerName != null)
            {
                var argumentList = ParseAttributeArgumentList($"(typeof({serializerName}))");
                var attribute = Attribute(ParseName("LambdaSerializer"), argumentList);
                var attributeList = AttributeList(SeparatedList(new AttributeSyntax[] { attribute }));
                var attributeLists = List(new AttributeListSyntax[] { attributeList });

                method = method.WithAttributeLists(attributeLists);
            }

            if (serializerName != null && serializerNamespace != null)
            {
                context.Usings.Add(serializerNamespace);
            }

            return method;
        }

        public IEnumerable<StatementSyntax> GenerateRunnerMethodBody()
        {
            var configFactory = context.ConfigFactoryType?.Name ?? "LambdaConfigFactory";
            var configFactoryNamespace = context.ConfigFactoryType?.ContainingNamespace?.ToString() ?? "Lambdajection.Core";
            context.Usings.Add(configFactoryNamespace);

            var inputParameterName = inputParameter!.Identifier.ValueText;
            yield return ParseStatement($"await using var host = new DefaultLambdaHost<{className}, {inputType}, {returnType}, {context.StartupTypeName}, LambdajectionConfigurator, {configFactory}>();");
            yield return ParseStatement($"return await host.Run({inputParameterName}, context);");
        }

        public MemberDeclarationSyntax GenerateMainMethod()
        {
            context.Usings.Add("Amazon.Lambda.RuntimeSupport");
            context.Usings.Add("Amazon.Lambda.Serialization.SystemTextJson");

            IEnumerable<StatementSyntax> GenerateBody()
            {
                var runnerMethodName = context.RunnerMethodName;
                yield return ParseStatement($"using var wrapper = HandlerWrapper.GetHandlerWrapper((Func<{inputType}, ILambdaContext, Task<{returnType}>>){runnerMethodName}, new DefaultLambdaJsonSerializer());");
                yield return ParseStatement($"using var bootstrap = new LambdaBootstrap(wrapper);");
                yield return ParseStatement($"await bootstrap.RunAsync();");
            }

            return MethodDeclaration(ParseTypeName($"Task"), "Main")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword), Token(AsyncKeyword)))
                .WithBody(Block(GenerateBody()));
        }
    }
}
