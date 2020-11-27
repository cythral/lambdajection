using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NSubstitute;

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

        [Test]
        public async Task Run_ReturnsContext()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.LambdaContext.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var context = Substitute.For<ILambdaContext>();
            var result = await (Task<ILambdaContext>)runMethod.Invoke(null, new object[] { string.Empty, context })!;
            result.Should().BeSameAs(context);
        }
    }
}
