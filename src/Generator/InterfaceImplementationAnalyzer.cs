using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public class InterfaceImplementationAnalyzer
    {
        private readonly ClassDeclarationSyntax classDeclaration;
        private readonly GenerationContext context;

        public InterfaceImplementationAnalyzer(
            ClassDeclarationSyntax classDeclaration,
            GenerationContext context
        )
        {
            this.classDeclaration = classDeclaration;
            this.context = context;
        }

        public Results Validate()
        {
            var results = new Results();
            var classType = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
            var attr = context.LambdaInterfaceAttribute;
            var interfaceTypeName = $"{attr.AssemblyName}.{attr.InterfaceName}`2";
            var type = context.Compilation.GetTypeByMetadataName(interfaceTypeName);

            if (type is null)
            {
                throw new GenerationFailureException
                {
                    Id = "LJ0003",
                    Title = "Lambda Interface Not Found",
                    Description = $"{attr.InterfaceName} was not found in ${attr.AssemblyName}.",
                    Location = classDeclaration.GetLocation(),
                };
            }

            var classMembers = classType!.GetMembers();
            var interfaceMembers = type.GetMembers();

            foreach (var interfaceMember in interfaceMembers)
            {
                var found = false;

                foreach (var classMember in classMembers)
                {
                    if (classMember.Name != interfaceMember.Name ||
                        interfaceMember is not IMethodSymbol interfaceMemberMethod ||
                        classMember is not IMethodSymbol classMemberMethod ||
                        classMemberMethod.Parameters.Length != interfaceMemberMethod.Parameters.Length)
                    {
                        continue;
                    }

                    var classMethodReturnType = classMemberMethod.ReturnType;
                    var interfaceMethodReturnType = interfaceMemberMethod.ReturnType;
                    var classMethodParameters = classMemberMethod.Parameters;
                    var interfaceMethodParameters = interfaceMemberMethod.Parameters;

                    if (IsIncompatibleGeneric(
                        classType: classMethodReturnType,
                        interfaceType: interfaceMethodReturnType,
                        ordinal: 1,
                        typeName: out var outputTypeName,
                        namespaceName: out var outputNamespaceName,
                        newInterfaceType: out var genericInterfaceReturnType
                    ))
                    {
                        continue;
                    }

                    if (genericInterfaceReturnType != null)
                    {
                        interfaceMethodReturnType = genericInterfaceReturnType;
                        results.OutputTypeName ??= outputTypeName;
                        results.OutputNamespaceName ??= outputNamespaceName;
                    }

                    if (interfaceMethodReturnType is ITypeParameterSymbol returnTypeParameterSymbol
                        && returnTypeParameterSymbol.Ordinal == 1)
                    {
                        interfaceMethodReturnType = classMethodReturnType;
                        results.OutputTypeName ??= interfaceMethodReturnType.Name;
                        results.OutputNamespaceName ??= interfaceMethodReturnType.ContainingNamespace.Name;
                    }

                    var parametersMatch = ParametersMatch(classMethodParameters, interfaceMethodParameters, out var inputTypeName, out var inputNamespaceName);
                    var returnTypeMatches = interfaceMethodReturnType.ToDisplayString() == classMethodReturnType.ToDisplayString();

                    results.InputTypeName ??= inputTypeName;
                    results.InputNamespaceName ??= inputNamespaceName;

                    if (parametersMatch && returnTypeMatches)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new GenerationFailureException
                    {
                        Id = "LJ0001",
                        Title = "Lambda Does Not Implement Interface",
                        Description = $"{classDeclaration.Identifier.ValueText} does not implement {interfaceTypeName}.{interfaceMember.Name}",
                        Location = classDeclaration.GetLocation(),
                    };
                }
            }

            return results;
        }

        private bool IsIncompatibleGeneric(ITypeSymbol classType, ITypeSymbol interfaceType, int ordinal, out string? typeName, out string? namespaceName, out INamedTypeSymbol? newInterfaceType)
        {
            typeName = null;
            namespaceName = null;
            newInterfaceType = null;

            if (interfaceType is INamedTypeSymbol namedInterfaceType
                            && classType is INamedTypeSymbol namedClassType
                            && namedInterfaceType.TypeArguments.Any(arg => arg is ITypeParameterSymbol))
            {
                if (!namedClassType.IsGenericType ||
                    namedClassType.TypeParameters.Length != namedInterfaceType.TypeParameters.Length)
                {
                    return true;
                }

                var args = namedClassType.TypeArguments.ToArray();
                newInterfaceType = namedInterfaceType.ConstructedFrom.Construct(args);

                var position = namedInterfaceType.TypeArguments
                .ToList()
                .FindIndex(param =>
                    param is ITypeParameterSymbol typeParameterSymbol
                    && typeParameterSymbol.Ordinal == ordinal
                );

                var type = position != -1
                    ? newInterfaceType.TypeArguments[position]
                    : null;

                typeName = type?.Name;
                namespaceName = type?.ContainingNamespace.Name;
            }

            return false;
        }

        private bool ParametersMatch(
            ImmutableArray<IParameterSymbol> classMethodParameters,
            ImmutableArray<IParameterSymbol> interfaceMethodParameters,
            out string? inputTypeName,
            out string? inputNamespaceName
        )
        {
            inputTypeName = null;
            inputNamespaceName = null;

            for (var i = 0; i < classMethodParameters.Length; i++)
            {
                var classMethodParameterType = classMethodParameters[i].Type;
                var interfaceMethodParameterType = interfaceMethodParameters[i].Type;

                if (IsIncompatibleGeneric(
                    classType: classMethodParameterType,
                    interfaceType: interfaceMethodParameterType,
                    ordinal: 0,
                    typeName: out var newInputTypeName,
                    namespaceName: out var newInputNamespaceName,
                    newInterfaceType: out var genericInterfaceParameter
                ))
                {
                    continue;
                }

                if (genericInterfaceParameter != null)
                {
                    interfaceMethodParameterType = genericInterfaceParameter;
                    inputTypeName = newInputTypeName;
                    inputNamespaceName = newInputNamespaceName;
                }

                if (interfaceMethodParameterType is ITypeParameterSymbol typeParameterSymbol
                    && typeParameterSymbol.Ordinal == 0)
                {
                    interfaceMethodParameterType = classMethodParameterType;
                    inputTypeName = classMethodParameterType.Name;
                    inputNamespaceName = classMethodParameterType.ContainingNamespace.Name;
                }

                if (interfaceMethodParameterType.ToDisplayString() != classMethodParameterType.ToDisplayString())
                {
                    return false;
                }
            }

            return true;
        }

        public class Results
        {
            public string? EncapsulatedTypeName { get; set; }

            public string? EncapsulatedNamespaceName { get; set; }

            public string? InputNamespaceName { get; set; }

            public string? InputTypeName { get; set; }

            public string? OutputNamespaceName { get; set; }

            public string? OutputTypeName { get; set; }
        }
    }
}
