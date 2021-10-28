using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class LambdaContextTests
    {
        private const string projectPath = "Compilation/Projects/LambdaContext/LambdaContext.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Run_ReturnsContext(
            ILambdaContext context
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.LambdaContext.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            var resultStream = await (Task<Stream>)runMethod.Invoke(null, new object[] { inputStream, context })!;
            var result = await JsonSerializer.DeserializeAsync<string>(resultStream);

            result.Should().Be(context.AwsRequestId);
        }
    }
}
