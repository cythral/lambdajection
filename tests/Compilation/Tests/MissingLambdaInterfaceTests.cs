using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class MissingLambdaInterfaceTests
    {
        private const string projectPath = "Compilation/Projects/MissingLambdaInterface/MissingLambdaInterface.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test]
        public async Task ShouldFailCompilationWithLJ0003()
        {
            var diagnostics = await project.GetGeneratorDiagnostics();
            diagnostics.Should().Contain(diagnostic => diagnostic.Id == "LJ0003");
        }
    }
}
