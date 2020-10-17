using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public readonly struct OptionClass : IEquatable<OptionClass>
    {
        public OptionClass(string configSectionName, ClassDeclarationSyntax classDeclaration, IEnumerable<string> encryptedProperties)
        {
            this.ConfigSectionName = configSectionName;
            this.ClassDeclaration = classDeclaration;
            this.EncryptedProperties = encryptedProperties;
        }

        public string ConfigSectionName { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public IEnumerable<string> EncryptedProperties { get; }


        public override bool Equals(object? obj)
        {
            return obj is OptionClass result && result.ConfigSectionName.Equals(ConfigSectionName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(OptionClass result)
        {
            return result.ConfigSectionName.Equals(ConfigSectionName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return ConfigSectionName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator ==(OptionClass optionClassA, OptionClass optionClassB)
        {
            return optionClassA.Equals(optionClassB);
        }

        public static bool operator !=(OptionClass optionClassA, OptionClass optionClassB)
        {
            return !optionClassA.Equals(optionClassB);
        }
    }
}
