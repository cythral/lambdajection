using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Lambdajection.Framework;
using Lambdajection.Framework.Utils;
using Lambdajection.Generator.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    internal class InterfaceImplementationAnalyzer
    {
        private readonly INamedTypeSymbol?[] typeMatches = new INamedTypeSymbol?[2];
        private readonly string?[] typeNameMatches = new string?[2];
        private readonly INamedTypeSymbol?[] typeEncapsulationMatches = new INamedTypeSymbol?[2];
        private readonly string?[] typeEncapsulationNameMatches = new string?[2];
        private readonly TypeUtils typeUtils;

        private readonly List<GeneratedMethodInfo> generatedMethods = new();

        public InterfaceImplementationAnalyzer(TypeUtils typeUtils)
        {
            this.typeUtils = typeUtils;
        }

        internal AnalyzerResults Analyze(ClassDeclarationSyntax classDeclaration, GenerationContext context)
        {
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
                if (IsCompilerGenerated(interfaceMember, out var generatedAttribute)
                    && interfaceMember is IMethodSymbol interfaceMethodSymbol)
                {
                    generatedMethods.Add(new GeneratedMethodInfo
                    {
                        GeneratedAttribute = generatedAttribute!,
                        GeneratedMethod = interfaceMethodSymbol,
                        Location = classDeclaration.GetLocation(),
                    });

                    continue;
                }

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
                    var parametersMatch = ParametersMatch(classMethodParameters, interfaceMethodParameters);
                    var returnTypeMatches = TypesMatch(classMethodReturnType, interfaceMethodReturnType, 1);

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

            return new AnalyzerResults
            {
                InputType = typeMatches[0],
                InputTypeName = typeNameMatches[0],
                InputEncapsulationType = typeEncapsulationMatches[0],
                InputEncapsulationTypeName = typeEncapsulationNameMatches[0],
                OutputTypeName = typeNameMatches[1],
                GeneratedMethods = generatedMethods,
            };
        }

        private bool IsCompilerGenerated(ISymbol member, out GeneratedAttribute? generatedAttribute)
        {
            var query = from attribute in member.GetAttributes()
                        where attribute.AttributeClass != null
                            && typeUtils.IsSymbolEqualToType(attribute.AttributeClass, typeof(GeneratedAttribute))
                        select AttributeFactory.Create<GeneratedAttribute>(attribute);
            generatedAttribute = query.FirstOrDefault();
            return generatedAttribute != null;
        }

        private bool IsIncompatibleGeneric(
            ITypeSymbol classType,
            ITypeSymbol interfaceType,
            int ordinal,
            out INamedTypeSymbol? newInterfaceType
        )
        {
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
                    param is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.Ordinal == ordinal
                );

                var type = position != -1 ? newInterfaceType.TypeArguments[position] : null;
                var typeName = type?.ToDisplayString();

                if (typeNameMatches[ordinal] != null && typeName != typeNameMatches[ordinal])
                {
                    return true;
                }

                typeMatches[ordinal] ??= type as INamedTypeSymbol;
                typeNameMatches[ordinal] ??= typeName;
                typeEncapsulationMatches[ordinal] ??= namedInterfaceType;
                typeEncapsulationNameMatches[ordinal] ??= namedInterfaceType?.Name;
            }

            return false;
        }

        /// <summary>
        /// Runs a check to see if the parameters of a class match
        /// a generic interface.
        /// </summary>
        private bool ParametersMatch(
            ImmutableArray<IParameterSymbol> classParameters,
            ImmutableArray<IParameterSymbol> interfaceParameters
        )
        {
            for (var i = 0; i < classParameters.Length; i++)
            {
                var classParameterType = classParameters[i].Type;
                var interfaceParameterType = interfaceParameters[i].Type;

                if (!TypesMatch(
                    classType: classParameterType,
                    interfaceType: interfaceParameterType,
                    ordinal: 0
                ))
                {
                    return false;
                }
            }

            return true;
        }

        private bool TypesMatch(ITypeSymbol classType, ITypeSymbol interfaceType, int ordinal)
        {
            if (IsIncompatibleGeneric(
                classType: classType,
                interfaceType: interfaceType,
                ordinal: ordinal,
                newInterfaceType: out var genericInterfaceType
            ))
            {
                return false;
            }

            if (genericInterfaceType != null)
            {
                interfaceType = genericInterfaceType;
            }

            var classTypeName = classType.ToDisplayString();
            var interfaceTypeName = interfaceType.ToDisplayString();

            if (interfaceType is ITypeParameterSymbol interfaceTypeParameter
                && interfaceTypeParameter.Ordinal == ordinal)
            {
                typeMatches[ordinal] ??= classType as INamedTypeSymbol;
                typeNameMatches[ordinal] ??= classTypeName;
                interfaceTypeName = typeNameMatches[ordinal];
            }

            return classTypeName == interfaceTypeName;
        }
    }
}
