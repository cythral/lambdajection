using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

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
    public class PermissionlessTests
    {
        private const string ProjectPath = "Compilation/Projects/Permissionless/Permissionless.csproj";

        private static readonly string TemplatePath = $"{TestMetadata.BaseOutputPath}/CompilationTestProjects/Permissionless/Debug/{TestMetadata.TargetFramework}/Handler.template.yml";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(ProjectPath);
        }

        [Test, Auto]
        public async Task Role_ShouldNotHaveEmptyPolicy(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
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
            props.Should().NotContainKey("Policies");
        }
    }
}
