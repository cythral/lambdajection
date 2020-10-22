using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

namespace Lambdajection.Tests.Compilation
{

    [Category("Integration")]
    public class DisposablesTests
    {
        private const string projectPath = "Compilation/Projects/Disposables/Disposables.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }


        [Test]
        public async Task Run_DisposesLambda()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Disposables.DisposableHandler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var task = (Task)runMethod.Invoke(null, new[] { "", null })!;
            await task;

            var dynamicTask = (dynamic)task;
            var result = dynamicTask.Result;
            object value = result.DisposeWasCalled;

            value.Should().Be(true);
        }

        [Test]
        public async Task Run_DisposesLambdaAsynchronously()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Disposables.AsyncDisposableHandler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var task = (Task)runMethod.Invoke(null, new[] { "", null })!;
            await task;

            var dynamicTask = (dynamic)task;
            var result = dynamicTask.Result;
            object value = result.DisposeAsyncWasCalled;

            value.Should().Be(true);
        }
    }
}
