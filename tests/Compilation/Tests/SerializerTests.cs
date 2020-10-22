using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

namespace Lambdajection.Tests.Compilation
{

    [Category("Integration")]
    public class SerializerTests
    {
        private const string projectPath = "Compilation/Projects/Serializer/Serializer.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test]
        public async Task CustomSerializer_ShouldBeTestSerializer()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Serializer.CustomSerializerHandler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var attribute = (LambdaSerializerAttribute?)runMethod.GetCustomAttributes(typeof(LambdaSerializerAttribute), false).FirstOrDefault();

            attribute.Should().NotBeNull();
            attribute!.SerializerType.FullName.Should().Be("Lambdajection.CompilationTests.Serializer.TestSerializer");
        }

        [Test]
        public async Task DefaultSerializer_ShouldBeDefaultLambdaJsonSerializer()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.Serializer.DefaultSerializerHandler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var attribute = (LambdaSerializerAttribute?)runMethod.GetCustomAttributes(typeof(LambdaSerializerAttribute), false).FirstOrDefault();

            attribute.Should().NotBeNull();
            attribute!.SerializerType.Should().BeSameAs(typeof(DefaultLambdaJsonSerializer));
        }
    }
}
