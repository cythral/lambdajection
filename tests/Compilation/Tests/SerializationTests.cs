using System;
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
    public class SerializationTests
    {
        private const string projectPath = "Compilation/Projects/Serialization/Serialization.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Run_ShouldReturnEmptyStringIfNotUsingCamelCase(
            string id
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var requestType = assembly.GetType("Lambdajection.CompilationTests.Serialization.Request")!;
            var handler = new HandlerWrapper<string>(assembly, "Lambdajection.CompilationTests.Serialization.Handler");
            var request = (dynamic)Activator.CreateInstance(requestType)!;
            request.Id = id;

            using var inputStream = await StreamUtils.CreateJsonStream(request);
            string result = await handler.Run(inputStream, null);

            result.Should().Be(string.Empty);
        }

        [Test, Auto]
        public async Task Run_ShouldReturnIdIfUsingCamelCase(
            string id
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var requestType = assembly.GetType("Lambdajection.CompilationTests.Serialization.Request")!;
            var handler = new HandlerWrapper<string>(assembly, "Lambdajection.CompilationTests.Serialization.Handler");
            var request = (dynamic)Activator.CreateInstance(requestType)!;
            request.Id = id;

            using var inputStream = await StreamUtils.CreateJsonStream(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            string result = await handler.Run(inputStream, null);

            result.Should().Be(id);
        }
    }
}
