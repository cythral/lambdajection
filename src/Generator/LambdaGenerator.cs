using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

using Lambdajection.Attributes;
using Lambdajection.Encryption;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lambdajection.Generator
{
    public class GenerationContext
    {
        public ClassDeclarationSyntax Declaration { get; set; } = null!;
        public SyntaxTree SyntaxTree { get; set; } = null!;
        public SemanticModel SemanticModel { get; set; } = null!;
        public AttributeData AttributeData { get; set; } = null!;
        public GeneratorExecutionContext SourceGeneratorContext { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public INamedTypeSymbol StartupType { get; set; } = default!;
        public INamedTypeSymbol? SerializerType { get; set; }
        public INamedTypeSymbol? ConfigFactoryType { get; set; }
        public string StartupTypeName { get; set; }
        public string StartupTypeDisplayName { get; set; }
        public HashSet<string> Usings { get; } = new HashSet<string>();
        public bool Nullable { get; set; }
        public bool GenerateProgramEntrypoint { get; set; }
    }

    [Generator]
    public class LambdaGenerator : ISourceGenerator
    {
        private readonly string[] usings = new string[]
        {
            "System",
            "System.Threading.Tasks",
            "System.IO",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Extensions",
            "Microsoft.Extensions.Configuration",
            "Amazon.Lambda.Core",
            "Lambdajection.Core"
        };

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine("Initializing...");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var options = context.AnalyzerConfigOptions.GlobalOptions;
            options.TryGetValue("build_property.GenerateLambdajectionEntrypoint", out var generateLambdajectionEntrypoint);
            options.TryGetValue("build_property.Nullable", out var nullable);

            generateLambdajectionEntrypoint ??= "false";
            nullable ??= "disable";

            var syntaxTrees = context.Compilation.SyntaxTrees;
            var generations = from tree in syntaxTrees
                              let semanticModel = context.Compilation.GetSemanticModel(tree)
                              from node in tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                              from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                              where attr.AttributeClass?.Name == nameof(LambdaAttribute)
                              let startupType = GetAttributeArgument(attr, "Startup")!
                              select new GenerationContext
                              {
                                  SourceGeneratorContext = context,
                                  Declaration = node,
                                  SyntaxTree = tree,
                                  SemanticModel = semanticModel,
                                  AttributeData = attr,
                                  CancellationToken = context.CancellationToken,
                                  StartupType = startupType,
                                  SerializerType = GetAttributeArgument(attr, "Serializer"),
                                  ConfigFactoryType = GetAttributeArgument(attr, "ConfigFactory"),
                                  StartupTypeName = startupType.Name,
                                  StartupTypeDisplayName = startupType.ToDisplayString(),
                                  GenerateProgramEntrypoint = generateLambdajectionEntrypoint.Equals("true", StringComparison.OrdinalIgnoreCase),
                                  Nullable = nullable.Equals("enable", StringComparison.OrdinalIgnoreCase),
                              };


            try
            {
                var units = generations.Select(generation => (((ClassDeclarationSyntax)generation.Declaration).Identifier.Text, GenerateUnit(generation)));

                foreach (var (name, unit) in units)
                {
                    context.AddSource(name, unit.NormalizeWhitespace().GetText(Encoding.UTF8));
                }
            }
            catch (GenerationFailureException)
            {
                // (exit)
            }
        }
        private static INamedTypeSymbol? GetAttributeArgument(AttributeData attributeData, string argName)
        {
            if (argName == "Startup")
            {
                return (INamedTypeSymbol?)attributeData.ConstructorArguments[0].Value;
            }

            var query = from arg in attributeData.NamedArguments
                        where arg.Key == argName
                        select (INamedTypeSymbol?)arg.Value.Value;

            return query.FirstOrDefault();
        }

        public IEnumerable<UsingDirectiveSyntax> GenerateUsings(HashSet<string> usingsAddedDuringGeneration, IEnumerable<string> existingUsings)
        {
            var emittedUsings = new HashSet<string>(usings);
            emittedUsings.UnionWith(usingsAddedDuringGeneration);
            emittedUsings.UnionWith(existingUsings);

            return emittedUsings.Select(name => UsingDirective(ParseName(name)));
        }

        public CompilationUnitSyntax GenerateUnit(GenerationContext context)
        {
            var namespaceNode = (NamespaceDeclarationSyntax?)context.Declaration.Parent;
            var unitRoot = context.SyntaxTree.GetCompilationUnitRoot();
            var existingUsings = unitRoot.Usings.Select(x => x.WithoutTrivia().Name.ToString());
            var members = GenerateMembers(context);
            var usings = GenerateUsings(context.Usings, existingUsings);

            if (namespaceNode != null)
            {
                var namespacedMembers = NamespaceDeclaration(namespaceNode.Name, List<ExternAliasDirectiveSyntax>(), List<UsingDirectiveSyntax>(), members);
                members = List(new MemberDeclarationSyntax[] { namespacedMembers });
            }

            var codes = new ExpressionSyntax[] { ParseExpression("CA2007"), ParseExpression("CA1812"), ParseExpression("CS1591") };
            var ignoreWarningsTrivia = Trivia(PragmaWarningDirectiveTrivia(Token(DisableKeyword), SeparatedList(codes), true));
            return CompilationUnit(unitRoot.Externs, List(usings), unitRoot.AttributeLists, members)
                .WithLeadingTrivia(TriviaList(ignoreWarningsTrivia));
        }

        public static SyntaxList<MemberDeclarationSyntax> GenerateMembers(GenerationContext context)
        {
            var includeFactories = context.SourceGeneratorContext.Compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == "AWSSDK.SecurityToken");
            var includeDefaultSerializer = context.SourceGeneratorContext.Compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == "Amazon.Lambda.Serialization.SystemTextJson");
            var declaration = (ClassDeclarationSyntax)context.Declaration;
            var namespaceName = declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().ElementAt(0).Name;
            var className = declaration.Identifier.ValueText;
            var handleMember = (from member in declaration!.Members
                                where (member as MethodDeclarationSyntax)?.Identifier.ValueText == "Handle"
                                select (MethodDeclarationSyntax)member).FirstOrDefault();
            var constructorArgs = from tree in context.SourceGeneratorContext.Compilation.SyntaxTrees
                                  from constructor in tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                                  from parameter in constructor.ParameterList.Parameters
                                  select parameter;

            if (handleMember == null)
            {
                var descriptor = new DiagnosticDescriptor("LJ0001", "Handle Method Not Implemented", "Implement the Handle method to provide Lambda Function Handler code.", "Lambdajection", DiagnosticSeverity.Error, true);
                var diagnostic = Diagnostic.Create(descriptor, Location.Create(declaration.SyntaxTree, declaration.Span));
                context.SourceGeneratorContext.ReportDiagnostic(diagnostic);
                Cancel(context.CancellationToken);
                throw new GenerationFailureException();
            }

            if (!includeFactories && constructorArgs.Any())
            {
                foreach (var arg in constructorArgs)
                {
                    if (arg?.Type == null)
                    {
                        continue;
                    }

                    var semanticModel = context.SourceGeneratorContext.Compilation.GetSemanticModel(arg.SyntaxTree);
                    var typeDefinition = semanticModel.GetTypeInfo(arg.Type).Type?.OriginalDefinition;
                    var qualifiedName = typeDefinition?.ContainingNamespace + "." + typeDefinition?.MetadataName + ", " + typeDefinition?.ContainingAssembly;

                    if (qualifiedName != $"Lambdajection.Core.IAwsFactory`1, Lambdajection.Core, Version={ThisAssembly.AssemblyVersion}, Culture=neutral, PublicKeyToken=null")
                    {
                        continue;
                    }

                    var descriptor = new DiagnosticDescriptor("LJ0002", "Factories Not Enabled", "[LJ0002] Add AWSSDK.SecurityToken as a dependency of your project to use AWS Factories.", "Lambdajection", DiagnosticSeverity.Error, true);
                    var diagnostic = Diagnostic.Create(descriptor, Location.Create(declaration.SyntaxTree, declaration.Span));
                    context.SourceGeneratorContext.ReportDiagnostic(diagnostic);
                    Cancel(context.CancellationToken);
                    throw new GenerationFailureException();
                }
            }

            var scanner = new LambdaCompilationScanner(context.SourceGeneratorContext.Compilation, context.SourceGeneratorContext.Compilation.SyntaxTrees, $"{namespaceName}.{className}", context.StartupTypeDisplayName);
            var scanResults = scanner.Scan();

            IEnumerable<MemberDeclarationSyntax> GenerateMembers()
            {
                yield return GenerateLambda(context, className!, handleMember!, scanResults, includeFactories, includeDefaultSerializer);
            }

            var result = List(GenerateMembers());
            return result;
        }

        public static ClassDeclarationSyntax GenerateLambda(GenerationContext context, string className, MethodDeclarationSyntax handleMethod, LambdaCompilationScanResult scanResults, bool includeFactories, bool includeDefaultSerializer)
        {
            var inputParameter = handleMethod.ParameterList.Parameters[0];
            var contextParameter = handleMethod.ParameterList.Parameters[1];
            var inputType = inputParameter?.Type?.ToString() ?? "";
            var contextType = contextParameter?.Type?.ToString() ?? "";
            var returnType = handleMethod.ReturnType.ChildNodes().ElementAt(0).ChildNodes().ElementAt(0);
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"ILambda<{inputType},{returnType}>")) };

            string? GetSerializerName()
            {
                if (context.SerializerType != null)
                {
                    return context.SerializerType.Name;
                }

                return includeDefaultSerializer ? "DefaultLambdaJsonSerializer" : null;
            }

            string? GetSerializerNamespace()
            {
                if (context.SerializerType != null)
                {
                    return context.SerializerType.ContainingNamespace?.ToString();
                }

                return includeDefaultSerializer ? "Amazon.Lambda.Serialization.SystemTextJson" : null;
            }

            MemberDeclarationSyntax GenerateRunMethod()
            {
                var modifiers = new SyntaxToken[]
                {
                    Token(PublicKeyword),
                    Token(StaticKeyword),
                    Token(AsyncKeyword),
                };

                var method = MethodDeclaration(ParseTypeName($"Task<{returnType}>"), "Run")
                    .WithModifiers(TokenList(modifiers))
                    .WithParameterList(handleMethod!.ParameterList)
                    .WithBody(Block(GenerateRunMethodBody()));

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

            IEnumerable<StatementSyntax> GenerateRunMethodBody()
            {
                var configFactory = context.ConfigFactoryType?.Name ?? "LambdaConfigFactory";
                var configFactoryNamespace = context.ConfigFactoryType?.ContainingNamespace?.ToString() ?? "Lambdajection.Core";
                context.Usings.Add(configFactoryNamespace);

                yield return ParseStatement($"await using var host = new LambdaHost<{className}, {inputType}, {returnType}, {context.StartupTypeName}, LambdajectionConfigurator, {configFactory}>();");
                yield return ParseStatement($"return await host.Run({inputParameter!.Identifier.ValueText}, {contextParameter!.Identifier.ValueText});");
            }

            MemberDeclarationSyntax GenerateMainMethod()
            {
                context.Usings.Add("Amazon.Lambda.RuntimeSupport");
                context.Usings.Add("Amazon.Lambda.Serialization.SystemTextJson");

                IEnumerable<StatementSyntax> GenerateBody()
                {
                    yield return ParseStatement($"using var wrapper = HandlerWrapper.GetHandlerWrapper((Func<{inputType}, {contextType}, Task<{returnType}>>)Run, new DefaultLambdaJsonSerializer());");
                    yield return ParseStatement($"using var bootstrap = new LambdaBootstrap(wrapper);");
                    yield return ParseStatement($"await bootstrap.RunAsync();");
                }

                return MethodDeclaration(ParseTypeName($"Task"), "Main")
                    .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword), Token(AsyncKeyword)))
                    .WithBody(Block(GenerateBody()));
            }

            var result = ClassDeclaration(className)
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddModifiers(Token(PartialKeyword))
                .AddMembers(
                    GenerateRunMethod(),
                    GenerateConfigurator(context, scanResults, includeFactories)
                );

            if (context.GenerateProgramEntrypoint)
            {
                result = result.AddMembers(GenerateMainMethod());
            }

            return result;
        }

        public static ClassDeclarationSyntax GenerateConfigurator(GenerationContext context, LambdaCompilationScanResult scanResults, bool includeFactories)
        {
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName("ILambdaConfigurator")) };
            var publicModifiersList = new SyntaxToken[] { Token(PublicKeyword) };

            IEnumerable<StatementSyntax> GenerateConfigureMethodBody()
            {
                if (scanResults.IncludeDecryptionFacade)
                {
                    context.Usings.Add("Amazon.KeyManagementService");

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
                        yield return ParseStatement($"services.AddSingleton<ILambdaInitializationService, {sectionName}Decryptor>();");
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

                if (includeFactories && !services.Any(service => service.ServiceName == "SecurityTokenService"))
                {
                    services = services.Prepend(new AwsServiceMetadata("SecurityTokenService", "IAmazonSecurityTokenService", "AmazonSecurityTokenServiceClient", "Amazon.SecurityToken"));
                }

                foreach (var service in services)
                {
                    context.Usings.Add(service.NamespaceName);

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
                var services = scanResults.AwsServices;

                if (!services.Any(service => service.ServiceName == "SecurityTokenService"))
                {
                    services = services.Prepend(new AwsServiceMetadata("SecurityTokenService", "IAmazonSecurityTokenService", "AmazonSecurityTokenServiceClient", "Amazon.SecurityToken"));
                }

                if (services.Any())
                {
                    context.Usings.Add("Amazon.SecurityToken");
                    context.Usings.Add("Amazon.SecurityToken.Model");
                }

                foreach (var service in services)
                {
                    var factory = GenerateAwsFactory(context, service.ServiceName, service.InterfaceName, service.ImplementationName);
                    classDeclaration = classDeclaration.AddMembers(factory);
                }
            }

            if (scanResults.IncludeDecryptionFacade)
            {
                context.Usings.Add("Lambdajection.Encryption");
                context.Usings.Add("Microsoft.Extensions.Options");

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
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"ILambdaInitializationService")) };

            MemberDeclarationSyntax GenerateConstructor()
            {
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName(nameof(IDecryptionService)), Identifier("decryptionService"), null),
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

            static MemberDeclarationSyntax GeneratePrivateField(string typeName, string name)
            {
                var attributes = List<AttributeListSyntax>();
                var modifiers = TokenList(Token(PrivateKeyword));
                var type = ParseTypeName(typeName);

                var variables = new VariableDeclaratorSyntax[] { VariableDeclarator(name) };
                var variable = VariableDeclaration(type)
                    .WithVariables(SeparatedList(variables));

                return FieldDeclaration(attributes, modifiers, variable);
            }

            MemberDeclarationSyntax GenerateInitializeMethod()
            {
                static IEnumerable<ExpressionSyntax> GenerateDecryptPropertyCalls(IEnumerable<string> properties)
                {
                    foreach (var prop in properties)
                    {
                        yield return ParseExpression($"Decrypt{prop}()");
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

                return MethodDeclaration(ParseTypeName("Task"), "Initialize")
                    .WithModifiers(TokenList(Token(PublicKeyword), Token(AsyncKeyword)))
                    .WithBody(Block(GenerateBody()));
            }

            static MemberDeclarationSyntax GenerateDecryptPropertyMethod(string prop)
            {
                IEnumerable<StatementSyntax> GenerateBody()
                {
                    yield return ParseStatement($"options.{prop} = await decryptionService.Decrypt(options.{prop});");
                }

                return MethodDeclaration(ParseTypeName("Task"), $"Decrypt{prop}")
                    .WithModifiers(TokenList(Token(PrivateKeyword), Token(AsyncKeyword)))
                    .WithBody(Block(GenerateBody()));
            }

            var decryptMethods = optionClass.EncryptedProperties.Select(prop => GenerateDecryptPropertyMethod(prop));

            return ClassDeclaration(optionClass.ConfigSectionName + "Decryptor")
                .WithBaseList(BaseList(Token(ColonToken), SeparatedList(typeConstraints)))
                .AddMembers(
                    GeneratePrivateField(nameof(IDecryptionService), "decryptionService"),
                    GeneratePrivateField($"{optionClassName}", "options"),
                    GenerateConstructor(),
                    GenerateInitializeMethod()
                )
                .AddMembers(decryptMethods.ToArray());
        }

        public static ClassDeclarationSyntax GenerateAwsFactory(GenerationContext context, string service, string interfaceName, string implementationName)
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
                var roleArnTypeName = context.Nullable ? "string?" : "string";
                var roleArnDefaultValue = ParseExpression("null");
                var parameters = SeparatedList(new ParameterSyntax[]
                {
                    Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName(roleArnTypeName), Identifier("roleArn"), EqualsValueClause(roleArnDefaultValue)),
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
