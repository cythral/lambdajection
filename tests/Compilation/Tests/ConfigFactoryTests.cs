using System.IO;
using System.Text.Json;
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

            var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            var resultStream = await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;
            var result = await JsonSerializer.DeserializeAsync<string>(resultStream);

            result.Should().Be("TestValue");
        }
    }
}
