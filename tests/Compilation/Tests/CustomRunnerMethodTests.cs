using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class CustomRunnerMethodTests
    {
        private const string projectPath = "Compilation/Projects/CustomRunnerMethod/CustomRunnerMethod.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test]
        public async Task Run_IsNotGenerated()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.CustomRunnerMethod.Handler")!;
            var runMethod = handlerType.GetMethod("Run");

            runMethod.Should().BeNull();
        }

        [Test]
        public async Task Process_IsGenerated()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.CustomRunnerMethod.Handler")!;
            var processMethod = handlerType.GetMethod("Process");

            processMethod.Should().NotBeNull();
        }
    }
}
