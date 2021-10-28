using System.Collections.Generic;

using Lambdajection.Framework;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    internal class LambdaGenerator
    {
        private readonly AnalyzerResults interfaceAnalyzerResults;
        private readonly GenerationContext context;
        private readonly string className;
        private readonly LambdaCompilationScanResult scanResults;
        private readonly string inputTypeName;
        private readonly string inputParameterType;
        private readonly string returnType;
        private readonly BaseTypeSyntax[] typeConstraints;
        private readonly ProgramContext programContext;

        public LambdaGenerator(
            AnalyzerResults interfaceAnalyzerResults,
            GenerationContext context,
            string className,
            LambdaCompilationScanResult scanResults,
            ProgramContext programContext
        )
        {
            this.interfaceAnalyzerResults = interfaceAnalyzerResults;
            this.context = context;
            this.className = className;
            this.scanResults = scanResults;
            this.programContext = programContext;
            inputTypeName = interfaceAnalyzerResults.InputTypeName!;
            returnType = interfaceAnalyzerResults.OutputTypeName!;

            inputParameterType = interfaceAnalyzerResults.InputEncapsulationTypeName != null
                ? $"{interfaceAnalyzerResults.InputEncapsulationTypeName}<{inputTypeName}>"
                : inputTypeName;

            var interfaceName = context.LambdaInterfaceAttribute.InterfaceName;
            typeConstraints = new[] { SimpleBaseType(ParseTypeName($"{interfaceName}<{inputTypeName},{returnType}>")) };
            context.Usings.Add(context.LambdaInterfaceAttribute.AssemblyName);
        }

        public ClassDeclarationSyntax Generate()
        {
            var configuratorGenerator = new ConfiguratorGenerator(context, scanResults, programContext);
            var configurator = configuratorGenerator.Generate();

            var result = ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(PartialKeyword))
                .AddMembers(
                    GenerateRunnerMethod(),
                    configurator);

            foreach (var member in GenerateMembers())
            {
                result = result.AddMembers(member);
            }

            if (context.Settings.GenerateEntrypoint)
            {
                result = result.AddMembers(GenerateMainMethod());
            }

            return result;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            foreach (var generatedMethodInfo in interfaceAnalyzerResults.GeneratedMethods)
            {
                var generator = generatedMethodInfo.GetGenerator(interfaceAnalyzerResults, context);
                yield return generator.GenerateMember();
            }
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
                Parameter(
                    attributeLists: List<AttributeListSyntax>(),
                    modifiers: TokenList(),
                    type: ParseTypeName("Stream"),
                    identifier: ParseToken("input"),
                    @default: null
                ),
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
            var method = MethodDeclaration(ParseTypeName($"Task<Stream>"), context.RunnerMethodName)
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(parameterList)
                .WithBody(body);

            return method;
        }

        public IEnumerable<StatementSyntax> GenerateRunnerMethodBody()
        {
            var configFactory = context.ConfigFactoryType?.Name ?? "LambdaConfigFactory";
            var configFactoryNamespace = context.ConfigFactoryType?.ContainingNamespace?.ToString() ?? "Lambdajection.Core";
            context.Usings.Add(configFactoryNamespace);

            var hostClass = context.LambdaHostAttribute.ClassName;
            yield return ParseStatement($"await using var host = new {hostClass}<{className}, {inputTypeName}, {returnType}, {context.StartupTypeName}, LambdajectionConfigurator, {configFactory}>();");
            yield return ParseStatement($"return await host.Run(input, context);");
        }

        public MemberDeclarationSyntax GenerateMainMethod()
        {
            context.Usings.Add("Amazon.Lambda.RuntimeSupport");

            IEnumerable<StatementSyntax> GenerateBody()
            {
                var runnerMethodName = context.RunnerMethodName;
                yield return ParseStatement($"using var wrapper = HandlerWrapper.GetHandlerWrapper((Func<Stream, ILambdaContext, Task<Stream>>){runnerMethodName});");
                yield return ParseStatement($"using var bootstrap = new LambdaBootstrap(wrapper);");
                yield return ParseStatement($"await bootstrap.RunAsync();");
            }

            return MethodDeclaration(ParseTypeName($"Task"), "Main")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword), Token(AsyncKeyword)))
                .WithBody(Block(GenerateBody()));
        }
    }
}
