using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public interface IMemberGenerator
    {
        MemberDeclarationSyntax GenerateMember();
    }
}
