using System.IO;
using System.Text.Json;
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

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            var resultStream = await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;
            var result = await JsonSerializer.DeserializeAsync<JsonElement>(resultStream);
            result.TryGetProperty("ExampleConfigValue", out var value);

            value.ToString().Should().Be(exampleConfigValue);
        }

        [Test]
        public async Task EncryptedConfigValueShouldBeReadFromEnvironmentAndDecrypted()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Configuration.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            var resultStream = await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;
            var result = await JsonSerializer.DeserializeAsync<JsonElement>(resultStream);
            result.TryGetProperty("ExampleEncryptedValue", out var value);

            value.ToString().Should().BeEquivalentTo("[decrypted] " + exampleEncryptedValue);
        }
    }
}
