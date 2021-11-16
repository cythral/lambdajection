using System;
using System.Collections.Generic;
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
        public async Task Run_ShouldReturnListOfAllRelevantIds(
            string id1,
            string id2
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var requestType = assembly.GetType("Lambdajection.CompilationTests.Sns.Request")!;
            var handler = new HandlerWrapper<string[]>(assembly, "Lambdajection.CompilationTests.Sns.Handler");
            var recordType = typeof(SnsRecord<>).MakeGenericType(requestType)!;
            var messageType = typeof(SnsMessage<>).MakeGenericType(requestType)!;
            var recordArrayType = recordType.MakeArrayType();

            dynamic CreateRecord(string id)
            {
                var request = (dynamic)Activator.CreateInstance(requestType!)!;
                var message = (dynamic)Activator.CreateInstance(messageType, request, default(Dictionary<string, SnsMessageAttribute>), string.Empty, string.Empty, string.Empty, default(Uri), string.Empty, default(DateTime), string.Empty, string.Empty, default(Uri))!;
                var record = (dynamic)Activator.CreateInstance(recordType, string.Empty, string.Empty, string.Empty, message)!;
                request.Id = id;
                return record;
            }

            var snsEventType = typeof(SnsEvent<>).MakeGenericType(requestType);
            var recordArray = (dynamic)Activator.CreateInstance(recordArrayType, 2)!;
            recordArray[0] = CreateRecord(id1);
            recordArray[1] = CreateRecord(id2);

            var snsEvent = (dynamic)Activator.CreateInstance(snsEventType, args: new[] { recordArray })!;

            using var inputStream = await StreamUtils.CreateJsonStream(snsEvent);
            string[] result = await handler.Run(inputStream, null);

            result.Should().BeEquivalentTo(new[] { id1, id2 });
        }
    }
}
