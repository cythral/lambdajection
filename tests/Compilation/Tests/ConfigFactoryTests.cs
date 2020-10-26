using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class ConfigFactoryTests
    {
        private const string projectPath = "Compilation/Projects/ConfigFactory/ConfigFactory.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Handler_RunsWithoutError()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.ConfigFactory.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var result = await (Task<string>)runMethod.Invoke(null, new[] { string.Empty, null })!;

            result.Should().Be("TestValue");
        }
    }
}
