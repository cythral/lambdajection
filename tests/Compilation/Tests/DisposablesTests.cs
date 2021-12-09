using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
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
            var disposeWasCalledProperty = handlerType.GetProperty("DisposeWasCalled", BindingFlags.Static | BindingFlags.Public);
            var disposeWasCalledGetter = disposeWasCalledProperty!.GetGetMethod();

            var disposeWasCalledBefore = (bool)disposeWasCalledGetter!.Invoke(null, Array.Empty<object>())!;
            disposeWasCalledBefore.Should().BeFalse();

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;

            var disposeWasCalledAfter = (bool)disposeWasCalledGetter!.Invoke(null, Array.Empty<object>())!;
            disposeWasCalledAfter.Should().BeTrue();
        }

        [Test]
        public async Task Run_DisposesLambdaAsynchronously()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Disposables.AsyncDisposableHandler")!;
            var runMethod = handlerType.GetMethod("Run")!;
            var disposeWasCalledProperty = handlerType.GetProperty("DisposeAsyncWasCalled", BindingFlags.Static | BindingFlags.Public);
            var disposeWasCalledGetter = disposeWasCalledProperty!.GetGetMethod();

            var disposeWasCalledBefore = (bool)disposeWasCalledGetter!.Invoke(null, Array.Empty<object>())!;
            disposeWasCalledBefore.Should().BeFalse();

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;

            var disposeWasCalledAfter = (bool)disposeWasCalledGetter!.Invoke(null, Array.Empty<object>())!;
            disposeWasCalledAfter.Should().BeTrue();
        }
    }
}
