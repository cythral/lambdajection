﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Encryption;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Lambdajection.Generator
{
    [Generator]
    public class LambdaGenerator : ISourceGenerator
    {
        private readonly UsingsGenerator usingsGenerator = new UsingsGenerator();

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var settings = GenerationSettings.FromContext(context);
                var syntaxTrees = context.Compilation.SyntaxTrees;
                var generations = from tree in syntaxTrees.AsParallel()
                                  let semanticModel = context.Compilation.GetSemanticModel(tree)

                                  from node in tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                                  from attr in semanticModel.GetDeclaredSymbol(node)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>()
                                  where attr.AttributeClass?.Name == nameof(LambdaAttribute)

                                  let startupType = GetAttributeArgument(attr, "Startup")!
                                  let generationContext = new GenerationContext
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
                                      Settings = settings,
                                  }

                                  let unit = GenerateUnit(generationContext)
                                  let document = unit.NormalizeWhitespace().GetText(Encoding.UTF8)
                                  let name = node.Identifier.Text
                                  select (name, document);

                foreach (var (name, document) in generations)
                {
                    context.AddSource(name, document);
                }
            }
            catch (AggregateException e)
            {
                var failureExceptions = e.InnerExceptions.OfType<GenerationFailureException>();
                var nonFailureExceptions = e.InnerExceptions.Where(e => e is not GenerationFailureException);

                foreach (var failure in failureExceptions)
                {
                    context.ReportDiagnostic(failure.Diagnostic);
                }

                using var source = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
                source.Cancel();

                if (nonFailureExceptions.Any())
                {
                    throw new AggregateException(nonFailureExceptions);
                }
            }
        }

        public static INamedTypeSymbol? GetAttributeArgument(AttributeData attributeData, string argName)
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

        public CompilationUnitSyntax GenerateUnit(GenerationContext context)
        {
            var namespaceNode = (NamespaceDeclarationSyntax?)context.Declaration.Parent;
            var unitRoot = context.SyntaxTree.GetCompilationUnitRoot();
            var existingUsings = unitRoot.Usings.Select(x => x.WithoutTrivia().Name.ToString());
            var members = GenerateMembers(context);
            var usings = usingsGenerator.Generate(context);

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
            var declaration = context.Declaration;
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
                throw new GenerationFailureException
                {
                    Id = "LJ0001",
                    Title = "Handle Method Not Implemented",
                    Description = "Implement the Handle method to provide Lambda Function Handler code.",
                    Location = Location.Create(declaration.SyntaxTree, declaration.Span),
                };
            }

            if (!context.Settings.IncludeAmazonFactories && constructorArgs.Any())
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

                    if (qualifiedName != typeof(IAwsFactory<>).AssemblyQualifiedName)
                    {
                        continue;
                    }

                    throw new GenerationFailureException
                    {
                        Id = "LJ0002",
                        Title = "Factories Not Enabled",
                        Description = "Add AWSSDK.SecurityToken as a dependency of your project to use AWS Factories.",
                        Location = Location.Create(declaration.SyntaxTree, declaration.Span),
                    };
                }
            }

            var scanner = new LambdaCompilationScanner(context.SourceGeneratorContext.Compilation, context.SourceGeneratorContext.Compilation.SyntaxTrees, $"{namespaceName}.{className}", context.StartupTypeDisplayName);
            var scanResults = scanner.Scan();

            IEnumerable<MemberDeclarationSyntax> GenerateMembers()
            {
                yield return GenerateLambda(context, className!, handleMember!, scanResults);
            }

            var result = List(GenerateMembers());
            return result;
        }

        public static ClassDeclarationSyntax GenerateLambda(GenerationContext context, string className, MethodDeclarationSyntax handleMethod, LambdaCompilationScanResult scanResults)
        {
            var inputParameter = handleMethod.ParameterList.Parameters[0];
            var inputType = inputParameter?.Type?.ToString() ?? string.Empty;
            var returnType = handleMethod.ReturnType.ChildNodes().ElementAt(0).ChildNodes().ElementAt(0);
            var typeConstraints = new BaseTypeSyntax[] { SimpleBaseType(ParseTypeName($"ILambda<{inputType},{returnType}>")) };

            string? GetSerializerName()
            {
                var result = context.SerializerType?.Name;
                result ??= context.Settings.IncludeDefaultSerializer ? "DefaultLambdaJsonSerializer" : null;
                return result;
            }

            string? GetSerializerNamespace()
            {
                var result = context.SerializerType?.ContainingNamespace?.ToString();
                result ??= context.Settings.IncludeDefaultSerializer ? "Amazon.Lambda.Serialization.SystemTextJson" : null;
                return result;
            }

            MemberDeclarationSyntax GenerateRunMethod()
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
                var method = MethodDeclaration(ParseTypeName($"Task<{returnType}>"), "Run")
                    .WithModifiers(TokenList(modifiers))
                    .WithParameterList(parameterList)
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
                yield return ParseStatement($"return await host.Run({inputParameter!.Identifier.ValueText}, context);");
            }

            MemberDeclarationSyntax GenerateMainMethod()
            {
                context.Usings.Add("Amazon.Lambda.RuntimeSupport");
                context.Usings.Add("Amazon.Lambda.Serialization.SystemTextJson");

                IEnumerable<StatementSyntax> GenerateBody()
                {
                    yield return ParseStatement($"using var wrapper = HandlerWrapper.GetHandlerWrapper((Func<{inputType}, ILambdaContext, Task<{returnType}>>)Run, new DefaultLambdaJsonSerializer());");
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
                    GenerateConfigurator(context, scanResults));

            if (context.Settings.GenerateEntrypoint)
            {
                result = result.AddMembers(GenerateMainMethod());
            }

            return result;
        }

        public static ClassDeclarationSyntax GenerateConfigurator(GenerationContext context, LambdaCompilationScanResult scanResults)
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
            }

            IEnumerable<StatementSyntax> GenerateConfigureAwsServicesMethodBody()
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
    }
}
