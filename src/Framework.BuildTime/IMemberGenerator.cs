using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Framework
{
    /// <summary>
    /// Interface to describe generators that generate class members.
    /// </summary>
    internal interface IMemberGenerator
    {
        /// <summary>
        /// Generate a class member.
        /// </summary>
        /// <returns>The generated class member.</returns>
        MemberDeclarationSyntax GenerateMember();
    }
}
