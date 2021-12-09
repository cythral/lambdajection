using System.Threading.Tasks;

using FluentAssertions;

using Lambdajection.Sns;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class SnsTests
    {
        private const string projectPath = "Compilation/Projects/Sns/Sns.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Run_ShouldReturnTheId(
            string id
        )
        {
            static SnsRecord<CloudFormationStackEvent> CreateRecord(string id)
            {
                var request = new CloudFormationStackEvent { StackId = id };
                var message = new SnsMessage<CloudFormationStackEvent>(request, default!, string.Empty, string.Empty, string.Empty, default!, string.Empty, default, string.Empty, string.Empty, default!)!;
                var record = new SnsRecord<CloudFormationStackEvent>(string.Empty, string.Empty, string.Empty, message)!;
                return record;
            }

            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handler = new HandlerWrapper<string>(assembly, "Lambdajection.CompilationTests.Sns.Handler");

            var recordArray = new[] { CreateRecord(id) };
            var snsEvent = new SnsEvent<CloudFormationStackEvent>(recordArray);

            using var inputStream = await StreamUtils.CreateJsonStream(snsEvent);
            string result = (await handler.Run(inputStream, null!))!;

            result.Should().Be(id);
        }
    }
}
