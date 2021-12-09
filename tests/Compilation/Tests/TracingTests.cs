using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Lambdajection.Generator.TemplateGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

using YamlDotNet.Serialization;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class TracingTests
    {
        private const string projectPath = "Compilation/Projects/Tracing/Tracing.csproj";
        private static readonly string TemplatePath = $"{TestMetadata.BaseOutputPath}/CompilationTestProjects/Tracing/Debug/{TestMetadata.TargetFramework}/Handler.template.yml";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task TracingShouldBeEnabled()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handler = new HandlerWrapper<bool>(assembly, "Lambdajection.CompilationTests.Tracing.Handler");

            using var inputStream = await StreamUtils.CreateJsonStream(string.Empty);
            bool result = await handler.Run(inputStream, null!);

            result.Should().BeTrue();
        }

        [Test, Auto]
        public async Task TemplateShouldHaveRoleWithXRayPolicy()
        {
            using var generation = await project.GenerateAssembly();
            var templateText = await File.ReadAllTextAsync(TemplatePath);
            var template = new DeserializerBuilder()
                .WithTagMapping("!GetAtt", typeof(GetAttTag))
                .WithTagMapping("!Sub", typeof(SubTag))
                .WithTypeConverter(new GetAttTagConverter())
                .WithTypeConverter(new SubTagConverter())
                .Build()
                .Deserialize<dynamic>(templateText.ToString());

            var props = (Dictionary<object, object>)template["Resources"]["HandlerRole"]["Properties"]!;
            var tracingConfig = (IEnumerable<object>)props["ManagedPolicyArns"];

            tracingConfig.Should().Contain("arn:aws:iam::aws:policy/AWSXRayDaemonWriteAccess");
        }

        [Test, Auto]
        public async Task TemplateShouldHaveActiveTracingEnabled()
        {
            using var generation = await project.GenerateAssembly();
            var templateText = await File.ReadAllTextAsync(TemplatePath);
            var template = new DeserializerBuilder()
                .WithTagMapping("!GetAtt", typeof(GetAttTag))
                .WithTagMapping("!Sub", typeof(SubTag))
                .WithTypeConverter(new GetAttTagConverter())
                .WithTypeConverter(new SubTagConverter())
                .Build()
                .Deserialize<dynamic>(templateText.ToString());

            var props = (Dictionary<object, object>)template["Resources"]["HandlerLambda"]["Properties"]!;
            var tracingConfig = (Dictionary<object, object>)props["TracingConfig"];
            var tracingMode = (string)tracingConfig["Mode"];

            tracingMode.Should().Be("Active");
        }
    }
}
