using System.Collections.Generic;
using System.Linq;

using Lambdajection.Framework;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    internal class ConfiguratorGenerator
    {
        private static readonly SyntaxToken[] PublicModifiersList =
            new SyntaxToken[] { Token(PublicKeyword) };

        private static readonly BaseTypeSyntax[] TypeConstraints =
            new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName("ILambdaConfigurator")) };

        private readonly GenerationContext context;
        private readonly LambdaCompilationScanResult scanResults;
        private readonly ProgramContext programContext;

        public ConfiguratorGenerator(
            GenerationContext context,
            LambdaCompilationScanResult scanResults,
            ProgramContext programContext
        )
        {
            this.context = context;
            this.scanResults = scanResults;
            this.programContext = programContext;
        }

        public ClassDeclarationSyntax Generate()
        {
            var classDeclaration = ClassDeclaration("LambdajectionConfigurator")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(TypeConstraints)))
                .AddMembers(
                    GenerateConfigureOptionsMethod(),
                    GenerateConfigureAwsServicesMethod());

            if (context.Settings.IncludeAmazonFactories)
            {
                var services = scanResults.AwsServices;

                if (!services.Any(service => service.ServiceName == "SecurityTokenService"))
                {
                    services = services.Prepend(new AwsServiceMetadata("SecurityTokenService", "IAmazonSecurityTokenService", "AmazonSecurityTokenServiceClient", "Amazon.SecurityToken"));
                }

                if (services.Any())
                {
                    context.Usings.Add("Amazon.SecurityToken");
                    context.Usings.Add("Amazon.SecurityToken.Model");
                    context.ExtraIamPermissionsRequired.Add("sts:AssumeRole");
                }

                foreach (var service in services)
                {
                    var awsFactoryGenerator = new AwsFactoryGenerator(context, service.ServiceName, service.InterfaceName, service.ImplementationName);
                    var factory = awsFactoryGenerator.Generate();
                    classDeclaration = classDeclaration.AddMembers(factory);
                }
            }

            if (scanResults.IncludeDecryptionFacade)
            {
                context.Usings.Add("Lambdajection.Encryption");
                context.Usings.Add("Microsoft.Extensions.Options");

                foreach (var optionClass in scanResults.OptionClasses)
                {
                    context.ExtraIamPermissionsRequired.Add("kms:Decrypt");
                    var decryptorGenerator = new OptionsDecryptorGenerator(optionClass);
                    var decryptorClass = decryptorGenerator.Generate();
                    classDeclaration = classDeclaration.AddMembers(decryptorClass);
                }
            }

            return classDeclaration;
        }

        public IEnumerable<StatementSyntax> GenerateConfigureMethodBody()
        {
            if (context.Settings.EnableTracing)
            {
                yield return ParseStatement("Amazon.XRay.Recorder.Handlers.AwsSdk.AWSSDKHandler.RegisterXRayForAllServices();");
                yield return ParseStatement("services.TryAddSingleton(typeof(Amazon.XRay.Recorder.Core.IAWSXRayRecorder), typeof(Amazon.XRay.Recorder.Core.AWSXRayRecorder));");
            }

            if (scanResults.IncludeDecryptionFacade)
            {
                context.Usings.Add("Amazon.KeyManagementService");

                yield return ParseStatement($"services.TryAddSingleton(typeof(IAmazonKeyManagementService), typeof(AmazonKeyManagementServiceClient));");
                yield return ParseStatement($"services.TryAddSingleton(typeof(IDecryptionService), typeof(DefaultDecryptionService));\n\n");
            }

            foreach (var optionClass in scanResults.OptionClasses)
            {
                var classDeclaration = optionClass.ClassDeclaration;
                var sectionName = optionClass.ConfigSectionName;
                var optionClassName = classDeclaration.Identifier.ValueText;
                var namespaceName = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0);
                var fullName = namespaceName.Name + "." + optionClassName;

                if (optionClass.EncryptedProperties.Any())
                {
                    yield return ParseStatement($"services.AddSingleton<ILambdaInitializationService, {sectionName}Decryptor>();");
                }

                yield return ParseStatement($"services.Configure<{fullName}>(configuration.GetSection(\"{optionClass.ConfigSectionName}\"));");
            }
        }

        public MemberDeclarationSyntax GenerateConfigureAwsServicesMethod()
        {
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IServiceCollection"), Identifier("services"), null),
            });

            return MethodDeclaration(ParseTypeName("void"), "ConfigureAwsServices")
                .WithModifiers(TokenList(PublicModifiersList))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateConfigureAwsServicesMethodBody()));
        }

        public MemberDeclarationSyntax GenerateConfigureOptionsMethod()
        {
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IConfiguration"), Identifier("configuration"), null),
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IServiceCollection"), Identifier("services"), null),
            });

            return MethodDeclaration(ParseTypeName("void"), "ConfigureOptions")
                .WithModifiers(TokenList(PublicModifiersList))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateConfigureMethodBody()));
        }

        public IEnumerable<StatementSyntax> GenerateConfigureAwsServicesMethodBody()
        {
            var services = scanResults.AwsServices;

            if (context.Settings.IncludeAmazonFactories && !services.Any(service => service.ServiceName == "SecurityTokenService"))
            {
                services = services.Prepend(new AwsServiceMetadata("SecurityTokenService", "IAmazonSecurityTokenService", "AmazonSecurityTokenServiceClient", "Amazon.SecurityToken"));
            }

            foreach (var service in services)
            {
                context.Usings.Add(service.NamespaceName);

                yield return ParseStatement($"services.AddSingleton<{service.InterfaceName}, {service.ImplementationName}>();");

                if (context.Settings.IncludeAmazonFactories)
                {
                    yield return ParseStatement($"services.AddSingleton<IAwsFactory<{service.InterfaceName}>, {service.ServiceName}Factory>();");
                }
            }
        }
    }
}
