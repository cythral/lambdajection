using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

using static System.Environment;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class ConfigurationTests
    {
        private const string exampleConfigValue = "example config value";

        private const string exampleEncryptedValue = "example encrypted config value";

        private const string projectPath = "Compilation/Projects/Configuration/Configuration.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SetEnvironmentVariable("Example:ExampleConfigValue", exampleConfigValue);
            SetEnvironmentVariable("Example:ExampleEncryptedValue", exampleEncryptedValue);
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test]
        public async Task Handle_ReturnsConfiguration()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, context) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Configuration.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var task = (Task)runMethod.Invoke(null, new[] { string.Empty, null })!;
            await task;

            var dynamicTask = (dynamic)task;
            var result = dynamicTask.Result;
            object value = result.ExampleConfigValue;

            value.Should().Be(exampleConfigValue);
        }

        [Test]
        public async Task EncryptedConfigValueShouldBeReadFromEnvironmentAndDecrypted()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Configuration.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var task = (Task)runMethod.Invoke(null, new[] { string.Empty, null })!;
            await task;

            var dynamicTask = (dynamic)task;
            var result = dynamicTask.Result;
            object value = result.ExampleEncryptedValue;

            value.Should().BeEquivalentTo("[decrypted] " + exampleEncryptedValue);
        }
    }
}
