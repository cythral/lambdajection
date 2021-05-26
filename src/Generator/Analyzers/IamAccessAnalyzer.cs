using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Lambdajection.Framework;
using Lambdajection.Framework.Utils;
using Lambdajection.Generator.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    internal class IamAccessAnalyzer
    {
        private readonly string lambdaName;
        private readonly GenerationContext context;
        private readonly TypeUtils typeUtils;

        public IamAccessAnalyzer(
            string lambdaName,
            GenerationContext context,
            TypeUtils typeUtils
        )
        {
            this.lambdaName = lambdaName;
            this.context = context;
            this.typeUtils = typeUtils;
        }

        public async Task<HashSet<string>> Analyze()
        {
            var collector = new TreeWalker(
                (CSharpCompilation)context.Compilation,
                typeUtils
            );

            try
            {
                var root = context.SyntaxTree.GetRoot();
                collector.Visit(root);
            }
            catch (Exception)
            {
            }

            collector.Permissions.UnionWith(context.ExtraIamPermissionsRequired);

            var options = context.SourceGeneratorContext.AnalyzerConfigOptions.GlobalOptions;
            if (options.TryGetValue("build_property.LambdajectionIamPermissionsOutputPath", out var iamPermissionsPath))
            {
                var permissionsFileContent = string.Join('\n', collector.Permissions);
                await File.WriteAllTextAsync(iamPermissionsPath + lambdaName + ".iam.txt", permissionsFileContent);
            }

            return collector.Permissions;
        }

        public class TreeWalker : CSharpSyntaxWalker
        {
            private readonly CSharpCompilation compilation;
            private readonly TypeUtils typeUtils;

            public TreeWalker(CSharpCompilation compilation, TypeUtils typeUtils)
            {
                this.compilation = compilation;
                this.typeUtils = typeUtils;
            }

            public HashSet<string> Permissions { get; private set; } = new HashSet<string>();

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                try
                {
                    AddPermissionsFromAttributes(node);

                    var type = GetClientType(node);
                    if (type == null || !IsAmazonResponseType(type))
                    {
                        VisitChildren(node);
                        return;
                    }

                    var assembly = LoadAssemblyForType(type);
                    var iamPrefix = GetIamPrefix(type, assembly);
                    var apiCallName = GetApiCallName(node);
                    var permission = iamPrefix + ":" + apiCallName;
                    permission = permission == "lambda:Invoke" ? "lambda:InvokeFunction" : permission;
                    Permissions.Add(permission);
                }
                catch (Exception)
                {
                    VisitChildren(node);
                }
            }

            private static string GetConfigClassName(ITypeSymbol type)
            {
                var fullNS = type.ContainingNamespace.ToString();
                var namespaceParts = fullNS?.Split('.').Take(2) ?? Array.Empty<string>();
                var baseNS = string.Join(".", namespaceParts);
                var squashedNS = string.Join(string.Empty, namespaceParts);
                return baseNS + "." + squashedNS + "Config";
            }

            private void AddPermissionsFromAttributes(InvocationExpressionSyntax node)
            {
                var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
                var accessExpression = node.Expression as MemberAccessExpressionSyntax;
                var methodName = Regex.Replace(accessExpression?.Name.ToString() ?? string.Empty, @"\<(.*?)\>", string.Empty);
                var methods = accessExpression?.Expression == null
                    ? Array.Empty<ISymbol>()
                    : (IEnumerable<ISymbol>?)semanticModel.GetTypeInfo(accessExpression.Expression).Type?.GetMembers(methodName);

                var permissionsFromAttrs = from method in methods ?? Array.Empty<ISymbol>()
                                           from attr in (IEnumerable<AttributeData>?)method?.GetAttributes() ?? Array.Empty<AttributeData>()
                                           where typeUtils.IsSymbolEqualToType(attr.AttributeClass!, typeof(RequiresIamPermissionAttribute))
                                           select AttributeFactory.Create<RequiresIamPermissionAttribute>(attr)?.Permission;

                foreach (var permissionFromAttr in permissionsFromAttrs)
                {
                    Permissions.Add(permissionFromAttr);
                }
            }

            private void VisitChildren(SyntaxNode node)
            {
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }

                var declaredSymbol = compilation
                .GetSemanticModel(node.SyntaxTree)
                .GetSymbolInfo(node)
                .Symbol;

                foreach (var syntaxReference in declaredSymbol?.DeclaringSyntaxReferences ?? ImmutableArray.Create<SyntaxReference>())
                {
                    var declaration = syntaxReference.GetSyntax();
                    Visit(declaration);
                }
            }

            private string? GetIamPrefix(ITypeSymbol type, Assembly assembly)
            {
                try
                {
                    var configClassName = GetConfigClassName(type);
                    var metadataType = assembly.GetType(configClassName, true);

                    if (metadataType == null)
                    {
                        return null;
                    }

                    // See Amazon.Runtime.ClientConfig
                    var clientConfig = Activator.CreateInstance(metadataType);
                    return clientConfig.GetPublicProperty<string>("AuthenticationServiceName");
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private ITypeSymbol? GetClientType(ExpressionSyntax? node)
            {
                if (node == null)
                {
                    return null;
                }

                var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
                var info = semanticModel.GetTypeInfo(node);
                var symbol = info.ConvertedType as INamedTypeSymbol;
                return symbol?.TypeArguments.Count() > 0 ? symbol.TypeArguments[0] : null;
            }

            private string? GetApiCallName(InvocationExpressionSyntax node)
            {
                try
                {
                    var accessExpression = node.Expression as MemberAccessExpressionSyntax;
                    var matcher = new Regex("Async$");
                    var name = accessExpression?.Name.ToString();
                    return name != null ? matcher.Replace(name, string.Empty) : null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private bool IsAmazonResponseType(ITypeSymbol type)
            {
                try
                {
                    return type.ContainingAssembly.Name.Split('.')[0] == "AWSSDK" &&
                        type.Name.EndsWith("Response");
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private Assembly LoadAssemblyForType(ITypeSymbol type)
            {
                var assemblyReferences = from reference in compilation.ExternalReferences
                                         where Path.GetFileNameWithoutExtension(reference.Display) == type.ContainingAssembly.Name
                                         select reference.Display;

                var referenceLocation = assemblyReferences.First();
                return Assembly.LoadFile(referenceLocation);
            }
        }
    }
}
