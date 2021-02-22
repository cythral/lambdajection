using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator.Utils
{
    public class TypeUtils
    {
        public bool IsSymbolEqualToType(INamedTypeSymbol symbol, Type type)
        {
            if (symbol == null)
            {
                return false;
            }

            var assembly = symbol.ContainingAssembly;
            var typeAssemblyInfo = type.Assembly.GetName();
            var format = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
            var name = symbol.ToDisplayString(format);
            var assemblyName = assembly?.MetadataName;
            var version = assembly?.Identity.Version;
            var publicKeyToken = assembly?.Identity.PublicKeyToken;

#pragma warning disable IDE0078
            if (assembly?.MetadataName == "System.Runtime" || assembly?.MetadataName == "System.Private.CoreLib")
            {
                var runtimeAssemblyInfo = typeof(object).Assembly.GetName();
                assemblyName = runtimeAssemblyInfo.Name;
                version = runtimeAssemblyInfo.Version;
                publicKeyToken = runtimeAssemblyInfo.GetPublicKeyToken().ToImmutableArray();
            }
#pragma warning restore IDE0078

            return
                name == type.FullName &&
                assemblyName == typeAssemblyInfo.Name &&
                version == typeAssemblyInfo.Version &&
                (publicKeyToken?.SequenceEqual(typeAssemblyInfo.GetPublicKeyToken()) ?? typeAssemblyInfo == null);
        }
    }
}
