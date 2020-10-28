using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class NoHandleMethodTests
    {
        private const string projectPath = "Compilation/Projects/NoHandleMethod/NoHandleMethod.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task CompilationFails_WithLJ0001()
        {
            var diagnostics = await project.GetGeneratorDiagnostics();
            diagnostics.Should().Contain(diagnostic => diagnostic.Id == "LJ0001");
        }
    }
}
