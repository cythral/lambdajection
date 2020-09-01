using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Cythral.CodeGeneration.Roslyn;

using Lambdajection.Core;
using Lambdajection.Encryption;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lambdajection.Generator
{
    public class LambdaGenerator : IRichCodeGenerator
    {
        private readonly INamedTypeSymbol startupType;
        private readonly string startupTypeName;
        private readonly string startupTypeDisplayName;


        private readonly string[] usings = new string[]
        {
            "System.Threading.Tasks",
            "System.IO",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Extensions",
            "Microsoft.Extensions.Configuration",
            "Amazon.Lambda.Core",
            "Lambdajection.Core"
        };

        private HashSet<string> usingsAddedDuringGeneration = null!;

        public LambdaGenerator(AttributeData attributeData)
        {
            this.startupType = (from arg in attributeData.NamedArguments
                                where arg.Key == "Startup"
                                select (INamedTypeSymbol)arg.Value.Value!).FirstOrDefault();

            this.startupTypeName = this.startupType.Name;
            this.startupTypeDisplayName = this.startupType.ToDisplayString();
        }

        public IEnumerable<UsingDirectiveSyntax> GenerateUsings(IEnumerable<UsingDirectiveSyntax> exclusions)
        {
            var exclusionNames = exclusions.Select(exclusion => exclusion.Name.ToString());
            var usingsWithoutExclusions = new HashSet<string>(usings);
            usingsWithoutExclusions.UnionWith(usingsAddedDuringGeneration);
            usingsWithoutExclusions.ExceptWith(exclusionNames);

            return usingsWithoutExclusions.Select(name => UsingDirective(ParseName(name)));
        }

        public async Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken = default)
        {
            usingsAddedDuringGeneration = new HashSet<string>();
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
            var includeFactories = context.Compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == "AWSSDK.SecurityToken");
            var declaration = (ClassDeclarationSyntax)context.ProcessingNode;
            var namespaceName = declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0).Name;
            var className = declaration.Identifier.ValueText;
            var handleMember = (from member in declaration!.Members
                                where (member as MethodDeclarationSyntax)?.Identifier.ValueText == "Handle"
                                select (MethodDeclarationSyntax)member).FirstOrDefault();

            var constructorArgs = from constructor in declaration.Members.OfType<ConstructorDeclarationSyntax>()
                                  from parameter in constructor.ParameterList.Parameters
                                  select parameter;

            if (handleMember == null)
            {
                var descriptor = new DiagnosticDescriptor("LJ0001", "Handle Method Not Implemented", "Implement the Handle method to provide Lambda Function Handler code.", "Lambdajection", DiagnosticSeverity.Error, true);
                var diagnostic = Diagnostic.Create(descriptor, Location.Create(declaration.SyntaxTree, declaration.Span));
                progress.Report(diagnostic);

                Cancel(cancellationToken);
                throw new GenerationFailureException();
            }

            if (!includeFactories && constructorArgs.Any())
            {
                foreach (var arg in constructorArgs)
                {
                    var typeDefinition = context.SemanticModel.GetTypeInfo(arg.Type).Type?.OriginalDefinition;
                    var qualifiedName = typeDefinition?.ContainingNamespace + "." + typeDefinition?.MetadataName + ", " + typeDefinition?.ContainingAssembly;

                    if (qualifiedName != typeof(IAwsFactory<>).AssemblyQualifiedName)
                    {
                        continue;
                    }

                    var descriptor = new DiagnosticDescriptor("LJ0002", "Factories Not Enabled", "[LJ0002] Add AWSSDK.SecurityToken as a dependency of your project to use AWS Factories.", "Lambdajection", DiagnosticSeverity.Error, true);
                    var diagnostic = Diagnostic.Create(descriptor, Location.Create(declaration.SyntaxTree, declaration.Span));
                    progress.Report(diagnostic);

                    Cancel(cancellationToken);
                    throw new GenerationFailureException();
                }
            }

            var scanner = new LambdaCompilationScanner(context.Compilation, context.Compilation.SyntaxTrees, $"{namespaceName}.{className}", startupTypeDisplayName);
            var scanResults = scanner.Scan();

            IEnumerable<MemberDeclarationSyntax> GenerateMembers()
            {
                yield return GenerateLambda(className!, handleMember!, scanResults, includeFactories);
            }

            var result = List(GenerateMembers());
            return Task.FromResult(result);
        }

        public ClassDeclarationSyntax GenerateLambda(string className, MethodDeclarationSyntax handleMethod, LambdaCompilationScanResult scanResults, bool includeFactories)
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
                yield return ParseStatement($"var host = new LambdaHost<{className}, {inputType}, {returnType}, {startupTypeName}, LambdajectionConfigurator>();");
                yield return ParseStatement($"return await host.Run({inputParameter!.Identifier.ValueText}, {contextParameter!.Identifier.ValueText});");
            }

            return ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(PartialKeyword))
                .AddMembers(
                    GenerateRunMethod(),
                    GenerateConfigurator(scanResults, includeFactories)
                );
        }

        public ClassDeclarationSyntax GenerateConfigurator(LambdaCompilationScanResult scanResults, bool includeFactories)
        {
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName("ILambdaConfigurator")) };
            var publicModifiersList = new SyntaxToken[] { Token(PublicKeyword) };

            IEnumerable<StatementSyntax> GenerateConfigureMethodBody()
            {
                if (scanResults.IncludeDecryptionFacade)
                {
                    usingsAddedDuringGeneration.Add("Amazon.KeyManagementService");

                    yield return ParseStatement($"services.TryAddSingleton(typeof(IAmazonKeyManagementService), typeof(AmazonKeyManagementServiceClient));");
                    yield return ParseStatement($"services.TryAddSingleton(typeof({nameof(IDecryptionService)}), typeof({nameof(DefaultDecryptionService)}));\n\n");
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
                        yield return ParseStatement($"services.AddSingleton<IPostConfigureOptions<{fullName}>, {sectionName}Decryptor>();");
                    }

                    yield return ParseStatement($"services.Configure<{fullName}>(configuration.GetSection(\"{optionClass.ConfigSectionName}\"));");
                }
            }

            MemberDeclarationSyntax GenerateConfigureOptionsMethod()
            {
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IConfiguration"), Identifier("configuration"), null),
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IServiceCollection"), Identifier("services"), null),
                });

                return MethodDeclaration(ParseTypeName("void"), "ConfigureOptions")
                    .WithModifiers(TokenList(publicModifiersList))
                    .WithParameterList(ParameterList(parameters))
                    .WithBody(Block(GenerateConfigureMethodBody()));
            };

            IEnumerable<StatementSyntax> GenerateConfigureAwsServicesMethodBody()
            {
                var services = scanResults.AwsServices;

                if (includeFactories && services.Any())
                {
                    services = services.Where(service => service.ServiceName != "SecurityTokenService");
                    yield return ParseStatement($"services.AddSingleton<IAmazonSecurityTokenService, AmazonSecurityTokenServiceClient>();");
                }

                foreach (var service in services)
                {
                    usingsAddedDuringGeneration.Add(service.NamespaceName);

                    yield return ParseStatement($"services.AddSingleton<{service.InterfaceName}, {service.ImplementationName}>();");

                    if (includeFactories)
                    {
                        yield return ParseStatement($"services.AddSingleton<IAwsFactory<{service.InterfaceName}>, {service.ServiceName}Factory>();");
                    }
                }
            }

            MemberDeclarationSyntax GenerateConfigureAwsServicesMethod()
            {
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("IServiceCollection"), Identifier("services"), null),
                });

                return MethodDeclaration(ParseTypeName("void"), "ConfigureAwsServices")
                    .WithModifiers(TokenList(publicModifiersList))
                    .WithParameterList(ParameterList(parameters))
                    .WithBody(Block(GenerateConfigureAwsServicesMethodBody()));
            }

            var classDeclaration = ClassDeclaration("LambdajectionConfigurator")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GenerateConfigureOptionsMethod(),
                    GenerateConfigureAwsServicesMethod()
                );

            if (includeFactories)
            {
                if (scanResults.AwsServices.Any())
                {
                    usingsAddedDuringGeneration.Add("Amazon.SecurityToken");
                    usingsAddedDuringGeneration.Add("Amazon.SecurityToken.Model");
                }

                foreach (var service in scanResults.AwsServices)
                {
                    var factory = GenerateAwsFactory(service.ServiceName, service.InterfaceName, service.ImplementationName);
                    classDeclaration = classDeclaration.AddMembers(factory);
                }
            }

            if (scanResults.IncludeDecryptionFacade)
            {
                usingsAddedDuringGeneration.Add("Lambdajection.Encryption");
                usingsAddedDuringGeneration.Add("Microsoft.Extensions.Options");

                foreach (var optionClass in scanResults.OptionClasses)
                {
                    var decryptorClass = GenerateOptionsDecryptor(optionClass);
                    classDeclaration = classDeclaration.AddMembers(decryptorClass);
                }
            }

            return classDeclaration;
        }

        public static ClassDeclarationSyntax GenerateOptionsDecryptor(OptionClass optionClass)
        {
            var classDeclaration = optionClass.ClassDeclaration;
            var optionClassName = classDeclaration.Identifier.ValueText;
            var namespaceName = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0);
            var fullName = namespaceName.Name + "." + optionClassName;
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"IPostConfigureOptions<{fullName}>")) };

            MemberDeclarationSyntax GenerateConstructor()
            {
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName(nameof(IDecryptionService)), Identifier("decryptionService"), null),
                });

                static IEnumerable<StatementSyntax> GenerateBody()
                {
                    yield return ParseStatement("this.decryptionService = decryptionService;");
                }

                return ConstructorDeclaration($"{optionClass.ConfigSectionName}Decryptor")
                    .WithModifiers(TokenList(Token(PublicKeyword)))
                    .WithParameterList(ParameterList(parameters))
                    .WithBody(Block(GenerateBody()));
            }

            static MemberDeclarationSyntax GenerateDecryptionServiceField()
            {
                var attributes = List<AttributeListSyntax>();
                var modifiers = TokenList(Token(PrivateKeyword));
                var type = ParseTypeName(nameof(IDecryptionService));

                var variables = new VariableDeclaratorSyntax[] { VariableDeclarator("decryptionService") };
                var variable = VariableDeclaration(type)
                    .WithVariables(SeparatedList(variables));

                return FieldDeclaration(attributes, modifiers, variable);
            }

            MemberDeclarationSyntax GeneratePostConfigureMethod()
            {
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("string"), Identifier("name"), null),
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName(fullName), Identifier("options"), null),
                });

                IEnumerable<StatementSyntax> GenerateBody()
                {

                    var taskRunner = "Task.WaitAll(new Task[]\n{\n";

                    foreach (var prop in optionClass.EncryptedProperties)
                    {
                        taskRunner += $"Task.Run(async () => options.{prop} = await decryptionService.Decrypt(options.{prop})),\n";
                    }

                    taskRunner += "});";
                    yield return ParseStatement(taskRunner).NormalizeWhitespace();
                }

                return MethodDeclaration(ParseTypeName("void"), "PostConfigure")
                    .WithModifiers(TokenList(Token(PublicKeyword)))
                    .WithParameterList(ParameterList(parameters))
                    .WithBody(Block(GenerateBody()));
            }

            return ClassDeclaration(optionClass.ConfigSectionName + "Decryptor")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GenerateDecryptionServiceField(),
                    GenerateConstructor(),
                    GeneratePostConfigureMethod()
                );
        }

        public static ClassDeclarationSyntax GenerateAwsFactory(string service, string interfaceName, string implementationName)
        {
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"IAwsFactory<{interfaceName}>")) };
            var className = $"{service}Factory";

            MemberDeclarationSyntax GenerateConstructor()
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

            MemberDeclarationSyntax GenerateCreateMethod()
            {
                var roleArnDefaultValue = ParseExpression("null");
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("string"), Identifier("roleArn"), EqualsValueClause(roleArnDefaultValue)),
                });

                IEnumerable<StatementSyntax> GenerateBody()
                {
                    yield return IfStatement(ParseExpression("roleArn != null"),
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

            static MemberDeclarationSyntax GenerateStsClientField()
            {
                var attributes = List<AttributeListSyntax>();
                var modifiers = TokenList(Token(PrivateKeyword));
                var type = ParseTypeName("IAmazonSecurityTokenService");

                var variables = new VariableDeclaratorSyntax[] { VariableDeclarator("stsClient") };
                var variable = VariableDeclaration(type)
                    .WithVariables(SeparatedList(variables));

                return FieldDeclaration(attributes, modifiers, variable);
            }

            return ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GenerateStsClientField(),
                    GenerateConstructor(),
                    GenerateCreateMethod()
                );
        }

        private static void Cancel(CancellationToken token)
        {
            using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
            source.Cancel();
        }
    }
}
