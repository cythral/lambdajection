using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Lambdajection.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public class LambdaCompilationScanner
    {
        private readonly ImmutableArray<SyntaxTree> syntaxTrees;
        private readonly CSharpCompilation compilation;
        private readonly string lambdaTypeName;
        private readonly Dictionary<string, ClassDeclarationSyntax> optionClasses = new Dictionary<string, ClassDeclarationSyntax>();

        public LambdaCompilationScanner(CSharpCompilation compilation, ImmutableArray<SyntaxTree> syntaxTrees, string lambdaTypeName)
        {
            this.compilation = compilation;
            this.syntaxTrees = syntaxTrees;
            this.lambdaTypeName = lambdaTypeName;
        }

        public LambdaCompilationScanResult Scan()
        {
            foreach (var tree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                ScanTree(tree, semanticModel);
            }

            return new LambdaCompilationScanResult(optionClasses);
        }

        public void ScanTree(SyntaxTree tree, SemanticModel semanticModel)
        {
            var classes = tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();

            foreach (var classNode in classes)
            {
                var attributes = semanticModel.GetDeclaredSymbol(classNode)?.GetAttributes() ?? ImmutableArray.Create<AttributeData>();
                ScanForOptionClass(classNode, attributes);
            }
        }

        public void ScanForOptionClass(ClassDeclarationSyntax classNode, IEnumerable<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass?.Name != nameof(LambdaOptionsAttribute))
                {
                    continue;
                }

                var typeArg = (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;
                var typeArgName = typeArg.ToDisplayString();
                var configSectionName = (string)attribute.ConstructorArguments[1].Value!;


                if (typeArgName == lambdaTypeName)
                {
                    optionClasses.Add(configSectionName, classNode);
                    return;
                }
            }
        }
    }
}
